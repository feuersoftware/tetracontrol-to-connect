using FeuerSoftware.TetraControl2Connect.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FeuerSoftware.TetraControl2Connect.Endpoints;

public static class BackupEndpoints
{
    public static WebApplication MapBackupEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/backups").WithTags("Backups");

        // GET /api/backups — List all backups (without snapshot data for performance)
        group.MapGet("/", async (AppDbContext db) =>
        {
            var backups = await db.SettingsBackups
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new { b.Id, b.CreatedAt, b.Description })
                .ToListAsync();
            return Results.Ok(backups);
        }).WithName("ListBackups");

        // POST /api/backups — Create a manual backup
        group.MapPost("/", async (AppDbContext db) =>
        {
            var backup = await CreateBackupAsync(db, "Manuelle Sicherung");
            return Results.Ok(new { backup.Id, backup.CreatedAt, backup.Description });
        }).WithName("CreateBackup");

        // POST /api/backups/{id}/restore — Restore from a backup
        group.MapPost("/{id:int}/restore", async (int id, AppDbContext db) =>
        {
            var backup = await db.SettingsBackups.FindAsync(id);
            if (backup is null) return Results.NotFound(new { error = "Backup not found." });

            await RestoreFromBackupAsync(db, backup);
            return Results.Ok(new { message = "Settings restored.", backup.Id, backup.CreatedAt, backup.Description });
        }).WithName("RestoreBackup");

        // DELETE /api/backups/{id} — Delete a backup
        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var backup = await db.SettingsBackups.FindAsync(id);
            if (backup is null) return Results.NotFound(new { error = "Backup not found." });

            db.SettingsBackups.Remove(backup);
            await db.SaveChangesAsync();
            return Results.Ok(new { message = "Backup deleted." });
        }).WithName("DeleteBackup");

        return app;
    }

    /// <summary>
    /// Ensures a daily backup exists. Called automatically before any settings change.
    /// Creates a backup only if none exists for the current UTC date.
    /// </summary>
    public static async Task EnsureDailyBackupAsync(AppDbContext db)
    {
        var today = DateTime.UtcNow.Date;
        var hasBackupToday = await db.SettingsBackups
            .AnyAsync(b => b.CreatedAt >= today && b.CreatedAt < today.AddDays(1));

        if (!hasBackupToday)
        {
            await CreateBackupAsync(db, "Automatische Sicherung");
        }
    }

    /// <summary>
    /// Creates a snapshot of all current settings and stores it as a backup.
    /// </summary>
    public static async Task<SettingsBackupEntity> CreateBackupAsync(AppDbContext db, string description)
    {
        var snapshot = new SettingsSnapshot
        {
            ProgramSettings = await db.ProgramSettings.FirstOrDefaultAsync(p => p.Id == 1),
            TetraControlSettings = await db.TetraControlSettings.FirstOrDefaultAsync(t => t.Id == 1),
            StatusSettings = await db.StatusSettings.FirstOrDefaultAsync(s => s.Id == 1),
            PatternSettings = await db.PatternSettings
                .Include(p => p.AdditionalProperties)
                .FirstOrDefaultAsync(p => p.Id == 1),
            SeveritySettings = await db.SeveritySettings
                .Include(s => s.SeverityTranslations)
                .FirstOrDefaultAsync(s => s.Id == 1),
            SirenCalloutSettings = await db.SirenCalloutSettings
                .Include(s => s.SirenCodeTranslations)
                .FirstOrDefaultAsync(s => s.Id == 1),
            SirenStatusSettings = await db.SirenStatusSettings
                .Include(s => s.FailureTranslations)
                .FirstOrDefaultAsync(s => s.Id == 1),
            Sites = await db.Sites
                .Include(s => s.SubnetAddresses)
                .Include(s => s.Sirens)
                .OrderBy(s => s.Id)
                .ToListAsync(),
        };

        var backup = new SettingsBackupEntity
        {
            CreatedAt = DateTime.UtcNow,
            Description = description,
            SnapshotJson = JsonSerializer.Serialize(snapshot, JsonOptions),
        };

        db.SettingsBackups.Add(backup);
        await db.SaveChangesAsync();
        return backup;
    }

    /// <summary>
    /// Restores all settings from a backup snapshot.
    /// </summary>
    private static async Task RestoreFromBackupAsync(AppDbContext db, SettingsBackupEntity backup)
    {
        var snapshot = JsonSerializer.Deserialize<SettingsSnapshot>(backup.SnapshotJson, JsonOptions);
        if (snapshot is null) throw new InvalidOperationException("Failed to deserialize backup snapshot.");

        // Clear all existing settings
        db.Sites.RemoveRange(db.Sites);
        db.FailureTranslations.RemoveRange(db.FailureTranslations);
        db.SirenCodeTranslations.RemoveRange(db.SirenCodeTranslations);
        db.SeverityTranslations.RemoveRange(db.SeverityTranslations);
        db.AdditionalPatterns.RemoveRange(db.AdditionalPatterns);

        if (await db.SirenStatusSettings.AnyAsync())
            db.SirenStatusSettings.RemoveRange(db.SirenStatusSettings);
        if (await db.SirenCalloutSettings.AnyAsync())
            db.SirenCalloutSettings.RemoveRange(db.SirenCalloutSettings);
        if (await db.SeveritySettings.AnyAsync())
            db.SeveritySettings.RemoveRange(db.SeveritySettings);
        if (await db.PatternSettings.AnyAsync())
            db.PatternSettings.RemoveRange(db.PatternSettings);
        if (await db.StatusSettings.AnyAsync())
            db.StatusSettings.RemoveRange(db.StatusSettings);
        if (await db.TetraControlSettings.AnyAsync())
            db.TetraControlSettings.RemoveRange(db.TetraControlSettings);
        if (await db.ProgramSettings.AnyAsync())
            db.ProgramSettings.RemoveRange(db.ProgramSettings);

        await db.SaveChangesAsync();

        // Restore from snapshot
        if (snapshot.ProgramSettings is not null)
        {
            snapshot.ProgramSettings.Id = 1;
            db.ProgramSettings.Add(snapshot.ProgramSettings);
        }
        if (snapshot.TetraControlSettings is not null)
        {
            snapshot.TetraControlSettings.Id = 1;
            db.TetraControlSettings.Add(snapshot.TetraControlSettings);
        }
        if (snapshot.StatusSettings is not null)
        {
            snapshot.StatusSettings.Id = 1;
            db.StatusSettings.Add(snapshot.StatusSettings);
        }
        if (snapshot.PatternSettings is not null)
        {
            snapshot.PatternSettings.Id = 1;
            foreach (var ap in snapshot.PatternSettings.AdditionalProperties)
            {
                ap.Id = 0;
                ap.PatternSettingsId = 1;
            }
            db.PatternSettings.Add(snapshot.PatternSettings);
        }
        if (snapshot.SeveritySettings is not null)
        {
            snapshot.SeveritySettings.Id = 1;
            foreach (var st in snapshot.SeveritySettings.SeverityTranslations)
            {
                st.Id = 0;
                st.SeveritySettingsId = 1;
            }
            db.SeveritySettings.Add(snapshot.SeveritySettings);
        }
        if (snapshot.SirenCalloutSettings is not null)
        {
            snapshot.SirenCalloutSettings.Id = 1;
            foreach (var sct in snapshot.SirenCalloutSettings.SirenCodeTranslations)
            {
                sct.Id = 0;
                sct.SirenCalloutSettingsId = 1;
            }
            db.SirenCalloutSettings.Add(snapshot.SirenCalloutSettings);
        }
        if (snapshot.SirenStatusSettings is not null)
        {
            snapshot.SirenStatusSettings.Id = 1;
            foreach (var ft in snapshot.SirenStatusSettings.FailureTranslations)
            {
                ft.Id = 0;
                ft.SirenStatusSettingsId = 1;
            }
            db.SirenStatusSettings.Add(snapshot.SirenStatusSettings);
        }
        foreach (var site in snapshot.Sites)
        {
            site.Id = 0;
            foreach (var sa in site.SubnetAddresses)
            {
                sa.Id = 0;
                sa.SiteId = 0;
            }
            foreach (var si in site.Sirens)
            {
                si.Id = 0;
                si.SiteId = 0;
            }
            db.Sites.Add(site);
        }

        await db.SaveChangesAsync();
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private record SettingsSnapshot
    {
        public ProgramSettingsEntity? ProgramSettings { get; set; }
        public TetraControlSettingsEntity? TetraControlSettings { get; set; }
        public StatusSettingsEntity? StatusSettings { get; set; }
        public PatternSettingsEntity? PatternSettings { get; set; }
        public SeveritySettingsEntity? SeveritySettings { get; set; }
        public SirenCalloutSettingsEntity? SirenCalloutSettings { get; set; }
        public SirenStatusSettingsEntity? SirenStatusSettings { get; set; }
        public List<SiteEntity> Sites { get; set; } = [];
    }
}
