using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FeuerSoftware.TetraControl2Connect.Data;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace FeuerSoftware.TetraControl2Connect.Test.Data;

public class AppDbContextTest
{
    [Theory]
    [InlineData("20260312211112_InitialCreate", false)]
    [InlineData("20260629210027_ChangeBackupCreatedAtToDateTimeOffset", true)]
    public async Task Migrate_ExistingBackupTimestamps_PreservesUtcInstantsAndSnapshots(string migration, bool hasOffsets)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
        var migrator = db.GetService<IMigrator>();
        await migrator.MigrateAsync(migration);

        var timestamps = hasOffsets
            ? new[]
            {
                "2026-10-18 23:59:59.9999999+00:00",
                "2026-10-19 00:00:00+00:00",
                "2026-10-19 00:00:00.0000001+00:00",
                "2026-10-19 02:00:00.1234567+02:00",
                "2026-10-18 21:00:00.1234568-03:00",
                "2026-10-19T00:00:00.1000000Z",
                "2026-10-20 00:00:00.0000000+00:00"
            }
            : new[]
            {
                "2026-10-18 23:59:59.9999999",
                "2026-10-19 00:00:00",
                "2026-10-19 00:00:00.0000001",
                "2026-10-19 00:00:00.1234567",
                "2026-10-20 00:00:00"
            };
        const string snapshotJson = """{"programSettings":{"id":1,"sendVehicleStatus":true}}""";
        foreach (var timestamp in timestamps)
        {
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"""INSERT INTO "SettingsBackups" ("CreatedAt", "Description", "SnapshotJson") VALUES ({timestamp}, {"Existing"}, {snapshotJson})""");
        }
        var expected = timestamps.Select(t => DateTimeOffset.Parse(t, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal)).OrderByDescending(t => t).ToArray();

        await migrator.MigrateAsync();

        db.Database.HasPendingModelChanges().Should().BeFalse();
        var backups = await db.SettingsBackups.AsNoTracking().OrderByDescending(b => b.CreatedAt).ToListAsync();
        backups.Select(b => b.CreatedAt).Should().Equal(expected);
        backups.Should().OnlyContain(b => b.CreatedAt.Offset == TimeSpan.Zero);
        backups.Should().OnlyContain(b => b.Description == "Existing" && b.SnapshotJson == snapshotJson);
        var today = new DateTimeOffset(2026, 10, 19, 0, 0, 0, TimeSpan.Zero);
        var tomorrow = today.AddDays(1);
        (await db.SettingsBackups.CountAsync(b => b.CreatedAt >= today && b.CreatedAt < tomorrow))
            .Should().Be(expected.Count(t => t >= today && t < tomorrow));

        await migrator.MigrateAsync("20260629210027_ChangeBackupCreatedAtToDateTimeOffset");
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = """SELECT COUNT(*) FROM "SettingsBackups" WHERE "CreatedAt" LIKE '%+00:00'""";
            Convert.ToInt32(await command.ExecuteScalarAsync()).Should().Be(timestamps.Length);
        }
        await migrator.MigrateAsync();
        (await db.SettingsBackups.AsNoTracking().OrderByDescending(b => b.CreatedAt).Select(b => b.CreatedAt).ToArrayAsync())
            .Should().Equal(expected);
    }
}
