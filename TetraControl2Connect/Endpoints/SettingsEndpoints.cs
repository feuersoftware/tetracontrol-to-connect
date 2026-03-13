using FeuerSoftware.TetraControl2Connect.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FeuerSoftware.TetraControl2Connect.Endpoints;

/// <summary>
/// Minimal API endpoints for settings CRUD operations.
/// </summary>
public static class SettingsEndpoints
{
    public static WebApplication MapSettingsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/settings").WithTags("Settings");

        // Auto-create daily backup before any settings change (PUT)
        group.AddEndpointFilterFactory((factoryContext, next) =>
        {
            return async (invocationContext) =>
            {
                if (invocationContext.HttpContext.Request.Method == "PUT")
                {
                    var backupDb = invocationContext.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                    await BackupEndpoints.EnsureDailyBackupAsync(backupDb);
                }
                return await next(invocationContext);
            };
        });

        // GET /api/settings — Overview of all sections
        group.MapGet("/", async (AppDbContext db) =>
        {
            var sections = new List<object>();

            if (await db.ProgramSettings.AnyAsync())
                sections.Add(new { Section = "program", Exists = true });
            else
                sections.Add(new { Section = "program", Exists = false });

            if (await db.TetraControlSettings.AnyAsync())
                sections.Add(new { Section = "tetracontrol", Exists = true });
            else
                sections.Add(new { Section = "tetracontrol", Exists = false });

            if (await db.StatusSettings.AnyAsync())
                sections.Add(new { Section = "status", Exists = true });
            else
                sections.Add(new { Section = "status", Exists = false });

            if (await db.PatternSettings.AnyAsync())
                sections.Add(new { Section = "pattern", Exists = true });
            else
                sections.Add(new { Section = "pattern", Exists = false });

            if (await db.SeveritySettings.AnyAsync())
                sections.Add(new { Section = "severity", Exists = true });
            else
                sections.Add(new { Section = "severity", Exists = false });

            if (await db.SirenCalloutSettings.AnyAsync())
                sections.Add(new { Section = "siren-callout", Exists = true });
            else
                sections.Add(new { Section = "siren-callout", Exists = false });

            if (await db.SirenStatusSettings.AnyAsync())
                sections.Add(new { Section = "siren-status", Exists = true });
            else
                sections.Add(new { Section = "siren-status", Exists = false });

            if (await db.Sites.AnyAsync())
                sections.Add(new { Section = "connect", Exists = true });
            else
                sections.Add(new { Section = "connect", Exists = false });

            return Results.Ok(new { sections });
        })
        .WithName("GetAllSettings");

        // POST /api/settings/import — Import settings from appsettings.json (for migrating existing configurations)
        group.MapPost("/import", async (AppDbContext db, IConfiguration configuration, ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger<AppDbContext>();

            // Safety net: back up current settings before the import wipes the DB
            var hasData = await db.ProgramSettings.AnyAsync()
                       || await db.TetraControlSettings.AnyAsync()
                       || await db.Sites.AnyAsync();

            if (hasData)
            {
                await BackupEndpoints.CreateBackupAsync(db, "Automatische Sicherung vor Import");
                logger.LogInformation("Created automatic backup before importing settings from appsettings.json.");
            }

            await DatabaseConfigurationProvider.ImportFromConfiguration(db, configuration, logger);
            ((IConfigurationRoot)configuration).Reload();
            return Results.Ok(new { message = "Settings imported from appsettings.json." });
        })
        .WithName("ImportSettings");

        MapProgramEndpoints(group);
        MapTetraControlEndpoints(group);
        MapStatusEndpoints(group);
        MapPatternEndpoints(group);
        MapSeverityEndpoints(group);
        MapSirenCalloutEndpoints(group);
        MapSirenStatusEndpoints(group);
        MapConnectEndpoints(group);

        return app;
    }

    private static void MapProgramEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/program", async (AppDbContext db) =>
        {
            var entity = await db.ProgramSettings.FindAsync(1);
            return Results.Ok(entity ?? new ProgramSettingsEntity { Id = 1 });
        }).WithName("GetProgramSettings");

        group.MapPut("/program", async (ProgramSettingsEntity dto, AppDbContext db, IConfiguration configuration) =>
        {
            var entity = await db.ProgramSettings.FindAsync(1);
            if (entity is null)
            {
                dto.Id = 1;
                db.ProgramSettings.Add(dto);
                entity = dto;
            }
            else
            {
                entity.SendVehicleStatus = dto.SendVehicleStatus;
                entity.SendVehiclePositions = dto.SendVehiclePositions;
                entity.SendUserOperationStatus = dto.SendUserOperationStatus;
                entity.SendUserAvailability = dto.SendUserAvailability;
                entity.SendAlarms = dto.SendAlarms;
                entity.UpdateExistingOperations = dto.UpdateExistingOperations;
                entity.UserAvailabilityLifetimeDays = dto.UserAvailabilityLifetimeDays;
                entity.WebSocketReconnectTimeoutMinutes = dto.WebSocketReconnectTimeoutMinutes;
                entity.PollForActiveOperationBeforeFallbackMaxRetryCount = dto.PollForActiveOperationBeforeFallbackMaxRetryCount;
                entity.PollForActiveOperationBeforeFallbackDelay = dto.PollForActiveOperationBeforeFallbackDelay;
                entity.HeartbeatEndpointUrl = dto.HeartbeatEndpointUrl;
                entity.HeartbeatInterval = dto.HeartbeatInterval;
                entity.IgnoreStatus5 = dto.IgnoreStatus5;
                entity.IgnoreStatus0 = dto.IgnoreStatus0;
                entity.IgnoreStatus9 = dto.IgnoreStatus9;
                entity.AddPropertyForAlarmTexts = dto.AddPropertyForAlarmTexts;
                entity.UseFullyQualifiedSubnetAddressForConnect = dto.UseFullyQualifiedSubnetAddressForConnect;
                entity.IgnoreAlarmWithoutSubnetAddresses = dto.IgnoreAlarmWithoutSubnetAddresses;
                entity.AcceptCalloutsForSirens = dto.AcceptCalloutsForSirens;
                entity.AcceptSDSAsCalloutsWithPattern = dto.AcceptSDSAsCalloutsWithPattern;
            }
            await db.SaveChangesAsync();
            ((IConfigurationRoot)configuration).Reload();
            return Results.Ok(entity);
        }).WithName("UpdateProgramSettings");
    }

    private static void MapTetraControlEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/tetracontrol", async (AppDbContext db) =>
        {
            var entity = await db.TetraControlSettings.FindAsync(1);
            return Results.Ok(entity ?? new TetraControlSettingsEntity { Id = 1 });
        }).WithName("GetTetraControlSettings");

        group.MapPut("/tetracontrol", async (TetraControlSettingsEntity dto, AppDbContext db, IConfiguration configuration) =>
        {
            var entity = await db.TetraControlSettings.FindAsync(1);
            if (entity is null)
            {
                dto.Id = 1;
                db.TetraControlSettings.Add(dto);
                entity = dto;
            }
            else
            {
                entity.TetraControlHost = dto.TetraControlHost;
                entity.TetraControlPort = dto.TetraControlPort;
                entity.TetraControlUsername = dto.TetraControlUsername;
                entity.TetraControlPassword = dto.TetraControlPassword;
            }
            await db.SaveChangesAsync();
            ((IConfigurationRoot)configuration).Reload();
            return Results.Ok(entity);
        }).WithName("UpdateTetraControlSettings");
    }

    private static void MapStatusEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/status", async (AppDbContext db) =>
        {
            var entity = await db.StatusSettings.FindAsync(1);
            return Results.Ok(entity ?? new StatusSettingsEntity { Id = 1 });
        }).WithName("GetStatusSettings");

        group.MapPut("/status", async (StatusSettingsEntity dto, AppDbContext db, IConfiguration configuration) =>
        {
            var entity = await db.StatusSettings.FindAsync(1);
            if (entity is null)
            {
                dto.Id = 1;
                db.StatusSettings.Add(dto);
                entity = dto;
            }
            else
            {
                entity.AvailableStatus = dto.AvailableStatus;
                entity.LimitedAvailableStatus = dto.LimitedAvailableStatus;
                entity.NotAvailableStatus = dto.NotAvailableStatus;
                entity.ComingStatus = dto.ComingStatus;
                entity.NotComingStatus = dto.NotComingStatus;
                entity.ComingLaterStatus = dto.ComingLaterStatus;
            }
            await db.SaveChangesAsync();
            ((IConfigurationRoot)configuration).Reload();
            return Results.Ok(entity);
        }).WithName("UpdateStatusSettings");
    }

    private static void MapPatternEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/pattern", async (AppDbContext db) =>
        {
            var entity = await db.PatternSettings
                .Include(p => p.AdditionalProperties)
                .FirstOrDefaultAsync(p => p.Id == 1);
            return Results.Ok(entity ?? new PatternSettingsEntity { Id = 1 });
        }).WithName("GetPatternSettings");

        group.MapPut("/pattern", async (PatternSettingsEntity dto, AppDbContext db, IConfiguration configuration) =>
        {
            var entity = await db.PatternSettings
                .Include(p => p.AdditionalProperties)
                .FirstOrDefaultAsync(p => p.Id == 1);

            if (entity is null)
            {
                dto.Id = 1;
                db.PatternSettings.Add(dto);
            }
            else
            {
                entity.NumberPattern = dto.NumberPattern;
                entity.KeywordPattern = dto.KeywordPattern;
                entity.FactsPattern = dto.FactsPattern;
                entity.ReporterNamePattern = dto.ReporterNamePattern;
                entity.ReporterPhoneNumberPattern = dto.ReporterPhoneNumberPattern;
                entity.CityPattern = dto.CityPattern;
                entity.StreetPattern = dto.StreetPattern;
                entity.HouseNumberPattern = dto.HouseNumberPattern;
                entity.ZipCodePattern = dto.ZipCodePattern;
                entity.DistrictPattern = dto.DistrictPattern;
                entity.LatitudePattern = dto.LatitudePattern;
                entity.LongitudePattern = dto.LongitudePattern;
                entity.RicPattern = dto.RicPattern;

                db.AdditionalPatterns.RemoveRange(entity.AdditionalProperties);
                entity.AdditionalProperties = dto.AdditionalProperties
                    .Select(ap => new AdditionalPatternEntity
                    {
                        PatternSettingsId = 1,
                        Name = ap.Name,
                        Pattern = ap.Pattern,
                    }).ToList();
            }
            await db.SaveChangesAsync();
            ((IConfigurationRoot)configuration).Reload();
            var result = await db.PatternSettings
                .Include(p => p.AdditionalProperties)
                .FirstOrDefaultAsync(p => p.Id == 1);
            return Results.Ok(result);
        }).WithName("UpdatePatternSettings");
    }

    private static void MapSeverityEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/severity", async (AppDbContext db) =>
        {
            var entity = await db.SeveritySettings
                .Include(s => s.SeverityTranslations)
                .FirstOrDefaultAsync(s => s.Id == 1);

            return Results.Ok(new
            {
                UseServerityTranslationAsKeyword = entity?.UseServerityTranslationAsKeyword ?? false,
                SeverityTranslations = entity?.SeverityTranslations.ToDictionary(t => t.Code, t => t.Translation) ?? new Dictionary<string, string>(),
            });
        }).WithName("GetSeveritySettings");

        group.MapPut("/severity", async (SeveritySettingsDto dto, AppDbContext db, IConfiguration configuration) =>
        {
            var entity = await db.SeveritySettings
                .Include(s => s.SeverityTranslations)
                .FirstOrDefaultAsync(s => s.Id == 1);

            if (entity is null)
            {
                entity = new SeveritySettingsEntity { Id = 1 };
                db.SeveritySettings.Add(entity);
            }

            entity.UseServerityTranslationAsKeyword = dto.UseServerityTranslationAsKeyword;
            db.SeverityTranslations.RemoveRange(entity.SeverityTranslations);
            entity.SeverityTranslations = dto.SeverityTranslations
                .Select(kv => new SeverityTranslationEntity
                {
                    SeveritySettingsId = 1,
                    Code = kv.Key,
                    Translation = kv.Value,
                }).ToList();

            await db.SaveChangesAsync();
            ((IConfigurationRoot)configuration).Reload();
            return Results.Ok(new
            {
                entity.UseServerityTranslationAsKeyword,
                SeverityTranslations = entity.SeverityTranslations.ToDictionary(t => t.Code, t => t.Translation),
            });
        }).WithName("UpdateSeveritySettings");
    }

    private static void MapSirenCalloutEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/siren-callout", async (AppDbContext db) =>
        {
            var entity = await db.SirenCalloutSettings
                .Include(s => s.SirenCodeTranslations)
                .FirstOrDefaultAsync(s => s.Id == 1);

            return Results.Ok(new
            {
                UseSirenCodeTranslationAsKeyword = entity?.UseSirenCodeTranslationAsKeyword ?? false,
                SirenCodeTranslations = entity?.SirenCodeTranslations.ToDictionary(t => t.Code, t => t.Translation) ?? new Dictionary<string, string>(),
            });
        }).WithName("GetSirenCalloutSettings");

        group.MapPut("/siren-callout", async (SirenCalloutSettingsDto dto, AppDbContext db, IConfiguration configuration) =>
        {
            var entity = await db.SirenCalloutSettings
                .Include(s => s.SirenCodeTranslations)
                .FirstOrDefaultAsync(s => s.Id == 1);

            if (entity is null)
            {
                entity = new SirenCalloutSettingsEntity { Id = 1 };
                db.SirenCalloutSettings.Add(entity);
            }

            entity.UseSirenCodeTranslationAsKeyword = dto.UseSirenCodeTranslationAsKeyword;
            db.SirenCodeTranslations.RemoveRange(entity.SirenCodeTranslations);
            entity.SirenCodeTranslations = dto.SirenCodeTranslations
                .Select(kv => new SirenCodeTranslationEntity
                {
                    SirenCalloutSettingsId = 1,
                    Code = kv.Key,
                    Translation = kv.Value,
                }).ToList();

            await db.SaveChangesAsync();
            ((IConfigurationRoot)configuration).Reload();
            return Results.Ok(new
            {
                entity.UseSirenCodeTranslationAsKeyword,
                SirenCodeTranslations = entity.SirenCodeTranslations.ToDictionary(t => t.Code, t => t.Translation),
            });
        }).WithName("UpdateSirenCalloutSettings");
    }

    private static void MapSirenStatusEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/siren-status", async (AppDbContext db) =>
        {
            var entity = await db.SirenStatusSettings
                .Include(s => s.FailureTranslations)
                .FirstOrDefaultAsync(s => s.Id == 1);

            return Results.Ok(new
            {
                FailureTranslations = entity?.FailureTranslations.ToDictionary(t => t.Code, t => t.Translation) ?? new Dictionary<string, string>(),
            });
        }).WithName("GetSirenStatusSettings");

        group.MapPut("/siren-status", async (SirenStatusSettingsDto dto, AppDbContext db, IConfiguration configuration) =>
        {
            var entity = await db.SirenStatusSettings
                .Include(s => s.FailureTranslations)
                .FirstOrDefaultAsync(s => s.Id == 1);

            if (entity is null)
            {
                entity = new SirenStatusSettingsEntity { Id = 1 };
                db.SirenStatusSettings.Add(entity);
            }

            db.FailureTranslations.RemoveRange(entity.FailureTranslations);
            entity.FailureTranslations = dto.FailureTranslations
                .Select(kv => new FailureTranslationEntity
                {
                    SirenStatusSettingsId = 1,
                    Code = kv.Key,
                    Translation = kv.Value,
                }).ToList();

            await db.SaveChangesAsync();
            ((IConfigurationRoot)configuration).Reload();
            return Results.Ok(new
            {
                FailureTranslations = entity.FailureTranslations.ToDictionary(t => t.Code, t => t.Translation),
            });
        }).WithName("UpdateSirenStatusSettings");
    }

    private static void MapConnectEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/connect", async (AppDbContext db) =>
        {
            var sites = await db.Sites
                .Include(s => s.SubnetAddresses)
                .Include(s => s.Sirens)
                .OrderBy(s => s.Id)
                .ToListAsync();
            return Results.Ok(new { Sites = sites });
        }).WithName("GetConnectSettings");

        group.MapPut("/connect", async (ConnectSettingsDto dto, AppDbContext db, IConfiguration configuration) =>
        {
            // Remove all existing sites (cascade deletes children)
            db.Sites.RemoveRange(db.Sites);
            await db.SaveChangesAsync();

            foreach (var siteDto in dto.Sites)
            {
                var site = new SiteEntity
                {
                    Name = siteDto.Name,
                    Key = siteDto.Key,
                    SubnetAddresses = siteDto.SubnetAddresses.Select(sa => new SubnetAddressEntity
                    {
                        Name = sa.Name,
                        SNA = sa.SNA,
                        AlarmDirectly = sa.AlarmDirectly,
                        GSSI = sa.GSSI,
                    }).ToList(),
                    Sirens = siteDto.Sirens.Select(si => new SirenEntity
                    {
                        Name = si.Name,
                        Issi = si.Issi,
                        ExpectedHeartbeatInterval = si.ExpectedHeartbeatInterval,
                    }).ToList(),
                };
                db.Sites.Add(site);
            }

            await db.SaveChangesAsync();
            ((IConfigurationRoot)configuration).Reload();

            var sites = await db.Sites
                .Include(s => s.SubnetAddresses)
                .Include(s => s.Sirens)
                .OrderBy(s => s.Id)
                .ToListAsync();
            return Results.Ok(new { Sites = sites });
        }).WithName("UpdateConnectSettings");
    }

    // DTOs for dictionary-based endpoints
    public record SeveritySettingsDto
    {
        public bool UseServerityTranslationAsKeyword { get; set; }
        public Dictionary<string, string> SeverityTranslations { get; set; } = new();
    }

    public record SirenCalloutSettingsDto
    {
        public bool UseSirenCodeTranslationAsKeyword { get; set; }
        public Dictionary<string, string> SirenCodeTranslations { get; set; } = new();
    }

    public record SirenStatusSettingsDto
    {
        public Dictionary<string, string> FailureTranslations { get; set; } = new();
    }

    public record ConnectSettingsDto
    {
        public List<SiteEntity> Sites { get; set; } = [];
    }
}
