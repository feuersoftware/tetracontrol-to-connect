using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FeuerSoftware.TetraControl2Connect.Data;
using FeuerSoftware.TetraControl2Connect.Endpoints;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FeuerSoftware.TetraControl2Connect.Test.Endpoints;

public class SettingsImportTest
{
    [Fact]
    public async Task Import_UploadAndLocalFile_UsesSourceFileAndPreservesOtherSections()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(directory);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(directory, "appsettings.json"), """
                {"TetraControlOptions":{"TetraControlHost":"local-file-host","TetraControlPort":8080}}
                """);
            using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { ContentRootPath = directory });
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
            await using var app = builder.Build();
            app.MapSettingsEndpoints();
            app.MapBackupEndpoints();
            await using (var scope = app.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.EnsureCreatedAsync();
                await DatabaseConfigurationProvider.SeedDefaults(db);
            }
            await app.StartAsync();
            try
            {
                var address = app.Services.GetRequiredService<IServer>()
                    .Features.Get<IServerAddressesFeature>()!.Addresses.Single();
                using var client = new HttpClient { BaseAddress = new Uri(address) };
                using var upload = new StringContent("""
                    {"ConnectOptions":{"Sites":[{"Name":"Uploaded site","Key":"test-value"}]}}
                    """, Encoding.UTF8, "application/json");
                using var response = await client.PostAsync("/api/settings/import", upload);
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                using var localResponse = await client.PostAsync("/api/settings/import", null);
                localResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                using var plainText = new StringContent("""{"ConnectOptions":{"Sites":[]}}""");
                using var unsupportedResponse = await client.PostAsync("/api/settings/import", plainText);
                unsupportedResponse.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);

                // An empty JSON upload (Content-Length: 0) must not fall back to the local appsettings.json.
                foreach (var (json, error) in new[] { ("", "invalid_json"), ("{invalid", "invalid_json"),
                    ("""{"Logging":{"LogLevel":{"Default":"Information"}}}""", "invalid_settings"),
                    ("""{"TetraControlOptions":{"TetraControlPort":"invalid"}}""", "invalid_settings"),
                    ("""{"ProgramOptions":{"HeartbeatInterval":"invalid"}}""", "invalid_settings") })
                {
                    using var invalid = new StringContent(json, Encoding.UTF8, "application/json");
                    using var invalidResponse = await client.PostAsync("/api/settings/import", invalid);
                    invalidResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                    (await invalidResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("error").GetString()
                        .Should().Be(error, $"payload: {json}");
                }

                await using var scope = app.Services.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                (await db.Sites.SingleAsync()).Name.Should().Be("Uploaded site");
                (await db.TetraControlSettings.SingleAsync()).TetraControlHost.Should().Be("local-file-host");
                (await db.ProgramSettings.SingleAsync()).SendVehicleStatus.Should().BeTrue();
                var backup = await db.SettingsBackups.OrderBy(b => b.Id).FirstAsync();
                backup.Description.Should().Be("automatic_before_import");
                backup.SnapshotJson.Should().NotContain("Uploaded site");
                using var backupsResponse = await client.GetAsync("/api/backups");
                backupsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                (await backupsResponse.Content.ReadAsStringAsync()).Should().Contain("automatic_before_import");

                using var restoreResponse = await client.PostAsync($"/api/backups/{backup.Id}/restore", null);
                restoreResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                db.ChangeTracker.Clear();
                (await db.Sites.AnyAsync()).Should().BeFalse();
            }
            finally
            {
                await app.StopAsync();
            }
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
