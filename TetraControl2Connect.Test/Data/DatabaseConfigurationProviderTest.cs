using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeuerSoftware.TetraControl2Connect.Data;
using FeuerSoftware.TetraControl2Connect.Endpoints;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace FeuerSoftware.TetraControl2Connect.Test.Data;

public class DatabaseConfigurationProviderTest
{
    [Fact]
    public async Task ImportFromConfiguration_ConnectOnly_PreservesOtherSettingsAndBackup()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        await DatabaseConfigurationProvider.SeedDefaults(db);
        (await db.TetraControlSettings.FindAsync(1))!.TetraControlHost = "existing-host";
        db.Sites.Add(new SiteEntity { Name = "Old site", Key = "old-test-value" });
        await db.SaveChangesAsync();
        var backup = await BackupEndpoints.CreateBackupAsync(db, "Before import");

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("""
            {
              // Legacy appsettings files may contain comments.
              "ConnectOptions": {
                "Sites": [{
                  "Name": "Imported site", "Key": "import-test-value",
                  "SubnetAddresses": [{ "Name": "Subnet", "SNA": "123", "AlarmDirectly": true }],
                  "Sirens": [{ "Name": "Siren", "Issi": "1234567" }]
                }]
              }
            }
            """));
        var source = new ConfigurationBuilder().AddJsonStream(stream).Build();

        await DatabaseConfigurationProvider.ImportFromConfiguration(db, source);
        db.ChangeTracker.Clear();

        (await db.TetraControlSettings.SingleAsync()).TetraControlHost.Should().Be("existing-host");
        (await db.ProgramSettings.SingleAsync()).SendVehicleStatus.Should().BeTrue();
        var site = await db.Sites.Include(s => s.SubnetAddresses).Include(s => s.Sirens).SingleAsync();
        site.Name.Should().Be("Imported site");
        site.SubnetAddresses.Should().ContainSingle(s => s.SNA == "123" && s.AlarmDirectly);
        site.Sirens.Should().ContainSingle(s => s.Issi == "1234567");
        (await db.SettingsBackups.SingleAsync()).SnapshotJson.Should().Be(backup.SnapshotJson);
        backup.SnapshotJson.Should().Contain("Old site");
    }

    [Theory]
    [InlineData("""{"Logging":{"LogLevel":{"Default":"Information"}}}""", true)]
    [InlineData("""{"ProgramOptions":"invalid","ConnectOptions":{"Sites":[]}}""", true)]
    [InlineData("""{"ProgramOptions":{"SendVehicleStatus":"invalid"},"ConnectOptions":{"Sites":[]}}""", false)]
    [InlineData("""{"ProgramOptions":{"HeartbeatInterval":"invalid"}}""", false)]
    [InlineData("""{"ProgramOptions":{"PollForActiveOperationBeforeFallbackDelay":"invalid"}}""", false)]
    [InlineData("""{"ConnectOptions":{"Sites":[{"Name":"Site","Key":"test-value","Sirens":[{"ExpectedHeartbeatInterval":"invalid"}]}]}}""", false)]
    public async Task ImportFromConfiguration_InvalidSource_DoesNotChangeDatabase(string json, bool noSections)
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        await DatabaseConfigurationProvider.SeedDefaults(db);
        db.Sites.Add(new SiteEntity { Name = "Existing site", Key = "existing-test-value" });
        await db.SaveChangesAsync();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var source = new ConfigurationBuilder().AddJsonStream(stream).Build();

        Func<Task> import = () => DatabaseConfigurationProvider.ImportFromConfiguration(db, source);
        if (noSections)
            await import.Should().ThrowAsync<ArgumentException>();
        else
            await import.Should().ThrowAsync<InvalidOperationException>();
        db.ChangeTracker.Clear();

        (await db.Sites.SingleAsync()).Name.Should().Be("Existing site");
        (await db.ProgramSettings.SingleAsync()).SendVehicleStatus.Should().BeTrue();
        (await db.TetraControlSettings.SingleAsync()).Should().NotBeNull();
    }

    [Fact]
    public async Task ImportFromConfiguration_ExistingSingleton_ReplacesIncludedSectionOnly()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        await DatabaseConfigurationProvider.SeedDefaults(db);
        db.Sites.Add(new SiteEntity { Name = "Existing site", Key = "existing-test-value" });
        await db.SaveChangesAsync();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("""
            {"TetraControlOptions":{"TetraControlHost":"imported-host","TetraControlPort":8080}}
            """));
        var source = new ConfigurationBuilder().AddJsonStream(stream).Build();

        await DatabaseConfigurationProvider.ImportFromConfiguration(db, source);
        db.ChangeTracker.Clear();

        (await db.TetraControlSettings.SingleAsync()).TetraControlHost.Should().Be("imported-host");
        (await db.TetraControlSettings.SingleAsync()).TetraControlPort.Should().Be(8080);
        (await db.Sites.SingleAsync()).Name.Should().Be("Existing site");
    }

    [Fact]
    public async Task ImportFromConfiguration_EmptyConnectSection_ClearsSitesButPreservesOtherSettings()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        await DatabaseConfigurationProvider.SeedDefaults(db);
        db.Sites.Add(new SiteEntity { Name = "Existing site", Key = "existing-test-value" });
        await db.SaveChangesAsync();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("""{"ConnectOptions":{}}"""));
        var source = new ConfigurationBuilder().AddJsonStream(stream).Build();

        await DatabaseConfigurationProvider.ImportFromConfiguration(db, source);
        db.ChangeTracker.Clear();

        (await db.Sites.AnyAsync()).Should().BeFalse();
        (await db.ProgramSettings.SingleAsync()).SendVehicleStatus.Should().BeTrue();
    }
}
