using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FeuerSoftware.TetraControl2Connect.Data;
using FeuerSoftware.TetraControl2Connect.Endpoints;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace FeuerSoftware.TetraControl2Connect.Test.Endpoints;

public class BackupEndpointsTest : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        await using var db = CreateDbContext();
        await db.Database.MigrateAsync();

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
        _app = builder.Build();
        _app.MapBackupEndpoints();
        _app.MapSettingsEndpoints();
        await _app.StartAsync();
        _client = new HttpClient { BaseAddress = new Uri(_app.Urls.Single()) };
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        if (_app is not null)
            await _app.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task ListBackups_WithNoBackups_ReturnsEmptyList()
    {
        var backups = await _client.GetFromJsonAsync<JsonElement[]>("/api/backups");

        backups.Should().BeEmpty();
    }

    [Fact]
    public async Task ListBackups_WithDifferentOffsets_OrdersByUtcInstant()
    {
        var older = new DateTimeOffset(2026, 10, 19, 10, 0, 0, TimeSpan.FromHours(2)).AddTicks(1234567);
        var newer = older.AddTicks(1).ToOffset(TimeSpan.FromHours(-3));
        await using (var db = CreateDbContext())
        {
            db.SettingsBackups.AddRange(
                new SettingsBackupEntity { CreatedAt = older, Description = "Older", SnapshotJson = "{}" },
                new SettingsBackupEntity { CreatedAt = newer, Description = "Newer", SnapshotJson = "{}" });
            await db.SaveChangesAsync();
        }

        var backups = await _client.GetFromJsonAsync<JsonElement[]>("/api/backups");

        backups.Should().HaveCount(2);
        backups![0].GetProperty("description").GetString().Should().Be("Newer");
        backups[0].GetProperty("createdAt").GetDateTimeOffset().Should().Be(newer);
        backups[1].GetProperty("createdAt").GetDateTimeOffset().Should().Be(older);
        backups.Should().OnlyContain(b => b.GetProperty("createdAt").GetDateTimeOffset().Offset == TimeSpan.Zero);
        foreach (var backup in backups)
            backup.EnumerateObject().Should().NotContain(p => p.Name == "snapshotJson");
    }

    [Fact]
    public async Task CreateBackup_ThenList_ReturnsManualBackup()
    {
        using var response = await _client.PostAsync("/api/backups", null);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<JsonElement>();

        var backups = await _client.GetFromJsonAsync<JsonElement[]>("/api/backups");

        backups.Should().ContainSingle();
        backups![0].GetProperty("id").GetInt32().Should().Be(created.GetProperty("id").GetInt32());
        backups[0].GetProperty("description").GetString().Should().Be("Manuelle Sicherung");
        backups[0].GetProperty("createdAt").GetDateTimeOffset().Offset.Should().Be(TimeSpan.Zero);
    }

    [Theory]
    [InlineData(-1, 2)]
    [InlineData(0, 1)]
    [InlineData(TimeSpan.TicksPerDay - 1, 1)]
    [InlineData(TimeSpan.TicksPerDay, 2)]
    public async Task EnsureDailyBackup_WithUtcDayBoundaries_CreatesOnlyWhenNeeded(long ticksFromMidnight, int expectedCount)
    {
        var today = new DateTimeOffset(DateTimeOffset.UtcNow.Date, TimeSpan.Zero);
        await using var db = CreateDbContext();
        db.SettingsBackups.Add(new SettingsBackupEntity
        {
            CreatedAt = today.AddTicks(ticksFromMidnight).ToOffset(TimeSpan.FromHours(2)),
            Description = "Existing",
            SnapshotJson = "{}"
        });
        await db.SaveChangesAsync();

        await BackupEndpoints.EnsureDailyBackupAsync(db);
        await BackupEndpoints.EnsureDailyBackupAsync(db);

        (await db.SettingsBackups.CountAsync()).Should().Be(expectedCount);
        (await db.SettingsBackups.CountAsync(b => b.Description == "Automatische Sicherung"))
            .Should().Be(expectedCount - 1);
    }

    [Fact]
    public async Task UpdateSettings_CreatesDailyBackupBeforeChange_AndSavesSettings()
    {
        await using (var db = CreateDbContext())
        {
            db.ProgramSettings.Add(new ProgramSettingsEntity { SendVehicleStatus = false });
            await db.SaveChangesAsync();
        }

        using var firstResponse = await _client.PutAsJsonAsync("/api/settings/program",
            new ProgramSettingsEntity { SendVehicleStatus = true });
        firstResponse.EnsureSuccessStatusCode();
        using var secondResponse = await _client.PutAsJsonAsync("/api/settings/program",
            new ProgramSettingsEntity { SendVehicleStatus = true, WebSocketReconnectTimeoutMinutes = 5 });
        secondResponse.EnsureSuccessStatusCode();

        await using var verificationDb = CreateDbContext();
        var settings = await verificationDb.ProgramSettings.SingleAsync();
        settings.SendVehicleStatus.Should().BeTrue();
        settings.WebSocketReconnectTimeoutMinutes.Should().Be(5);
        var backup = await verificationDb.SettingsBackups.SingleAsync();
        backup.Description.Should().Be("Automatische Sicherung");
        using var snapshot = JsonDocument.Parse(backup.SnapshotJson);
        snapshot.RootElement.GetProperty("programSettings").GetProperty("sendVehicleStatus").GetBoolean()
            .Should().BeFalse();
    }

    private AppDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
}
