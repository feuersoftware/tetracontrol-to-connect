using System.Diagnostics;
using Destructurama;
using FeuerSoftware.TetraControl2Connect.Data;
using FeuerSoftware.TetraControl2Connect.Endpoints;
using FeuerSoftware.TetraControl2Connect.Hubs;
using FeuerSoftware.TetraControl2Connect.Services;
using FeuerSoftware.TetraControl2Connect.Shared;
using FeuerSoftware.TetraControl2Connect.Shared.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using Serilog;
using Serilog.Events;
using System.Net.Http.Headers;

namespace FeuerSoftware.TetraControl2Connect
{
    public static class Program
    {
        private const string DatabaseFileName = "settings.db";

        public static async Task Main(string[] args)
        {
            try
            {
                PrintLogo();

                var dbPath = Path.Combine(Directory.GetCurrentDirectory(), DatabaseFileName);
                var connectionString = $"Data Source={dbPath}";

                var builder = WebApplication.CreateBuilder(args);

                // Database configuration source (layered on top of the default appsettings.json)
                builder.Configuration.AddDatabaseConfiguration(connectionString);

                // Serilog
                builder.Host.UseSerilog((hostContext, configuration) =>
                {
                    configuration.ReadFrom.Configuration(hostContext.Configuration);
                    configuration.Enrich.FromLogContext();
                    configuration.Filter
                        .ByExcluding(e => e.Level == LogEventLevel.Debug && e.RenderMessage().Contains("HttpMessageHandler cleanup cycle"));
                    configuration.Destructure.ToMaximumDepth(7);
                    configuration.Destructure.ToMaximumStringLength(40);
                    configuration.Destructure.UsingAttributes();
                    configuration.MinimumLevel.Debug();
                    configuration.WriteTo.File(@"logs/TetraControl2Connect_Log_.txt",
                        restrictedToMinimumLevel: LogEventLevel.Debug,
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        retainedFileCountLimit: 31);
                    configuration.WriteTo.Console(
                        restrictedToMinimumLevel: LogEventLevel.Debug);
                    configuration.WriteTo.Debug(LogEventLevel.Verbose);
                });

                // Siren-specific logger
                var sirenLogger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.File(@"logs/siren_events_.txt",
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 31)
                    .CreateLogger();

                // Core services
                builder.Services
                    .AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString), ServiceLifetime.Scoped)
                    .Configure<ProgramOptions>(builder.Configuration.GetSection(ProgramOptions.SectionName))
                    .Configure<TetraControlOptions>(builder.Configuration.GetSection(TetraControlOptions.SectionName))
                    .Configure<ConnectOptions>(builder.Configuration.GetSection(ConnectOptions.SectionName))
                    .Configure<StatusOptions>(builder.Configuration.GetSection(StatusOptions.SectionName))
                    .Configure<PatternOptions>(builder.Configuration.GetSection(PatternOptions.SectionName))
                    .Configure<SeverityOptions>(builder.Configuration.GetSection(SeverityOptions.SectionName))
                    .Configure<SirenCalloutOptions>(builder.Configuration.GetSection(SirenCalloutOptions.SectionName))
                    .Configure<SirenStatusOptions>(builder.Configuration.GetSection(SirenStatusOptions.SectionName))
                    .AddSingleton<ITetraControlClient, TetraControlClient>()
                    .AddSingleton<IConnectApiService, ConnectApiService>()
                    .AddSingleton<IVehicleService, VehicleService>()
                    .AddSingleton<IUserService, UserService>()
                    .AddSingleton<ISDSService, SDSService>()
                    .AddSingleton<ISitesService, SitesService>()
                    .AddSingleton<ISirenService, SirenService>()
                    .AddSingleton<Serilog.ILogger>(sirenLogger)
                    .AddHostedService<Agent>();

                // Connect HTTP client
                builder.Services
                    .AddHttpClient(nameof(IConnectApiService), c =>
                    {
                        c.BaseAddress = new Uri(Constants.ConnectBaseUrl);
                        c.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("TetraControl2Connect", Constants.Version));
                    })
                    .AddPolicyHandler(GetRetryPolicy());

                // Heartbeat HTTP client
                builder.Services
                    .AddHttpClient(nameof(Agent), (s, c) =>
                    {
                        var options = s.GetRequiredService<IOptions<ProgramOptions>>().Value;

                        c.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("TetraControl2Connect", Constants.Version));

                        if (options.IsHeartbeatConfigured())
                        {
                            c.BaseAddress = new Uri(options.HeartbeatEndpointUrl);
                        }
                    })
                    .AddPolicyHandler(HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .WaitAndRetryAsync(1, retryAttempt => TimeSpan.FromSeconds(Math.Pow(3, retryAttempt))));

                // Swagger / OpenAPI
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                // CORS (allow credentials for SignalR)
                builder.Services.AddCors(options =>
                {
                    options.AddDefaultPolicy(policy => policy
                        .SetIsOriginAllowed(_ => true)
                        .AllowCredentials()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
                });

                // SignalR
                builder.Services.AddSignalR();

                // Kestrel on port 5050
                builder.WebHost.UseUrls("http://localhost:5050");

                var app = builder.Build();

                // Middleware pipeline
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseCors();

                // Settings API endpoints
                app.MapSettingsEndpoints();
                app.MapBackupEndpoints();

                // SignalR hub
                app.MapHub<MessageHub>("/hubs/messages");

                // Serve Admin UI SPA
                // In Development the SPA proxy (Microsoft.AspNetCore.SpaProxy) automatically
                // proxies to the Nuxt dev server on http://localhost:3000.
                // In Production, static files are served from wwwroot (populated by dotnet publish).
                app.UseStaticFiles();
                app.MapFallbackToFile("index.html");

                // Apply EF Core migrations and seed database on first start
                using (var scope = app.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    context.Database.Migrate();

                    var hasData = context.ProgramSettings.Any()
                               || context.TetraControlSettings.Any()
                               || context.Sites.Any();

                    if (!hasData)
                    {
                        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
                            .CreateLogger(nameof(DatabaseConfigurationProvider));
                        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                        // Scenario 1: Existing installation with appsettings.json from previous version
                        var hasAppSettings = config.GetSection(ProgramOptions.SectionName).Exists()
                                          || config.GetSection(TetraControlOptions.SectionName).Exists()
                                          || config.GetSection(ConnectOptions.SectionName).Exists();

                        if (hasAppSettings)
                        {
                            logger.LogInformation("Found existing settings in appsettings.json — importing into database...");
                            await DatabaseConfigurationProvider.ImportFromConfiguration(context, config, logger);
                        }
                        else
                        {
                            // Scenario 2: Fresh install — seed with sensible defaults
                            await DatabaseConfigurationProvider.SeedDefaults(context, logger);
                        }

                        // Reload configuration so IOptions<T> bindings pick up the new DB values
                        ((IConfigurationRoot)app.Configuration).Reload();
                    }
                }

                // Open browser after startup
                app.Lifetime.ApplicationStarted.Register(() =>
                {
                    var url = "http://localhost:5050";
                    try
                    {
                        if (OperatingSystem.IsWindows())
                            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                        else if (OperatingSystem.IsMacOS())
                            Process.Start("open", url);
                        else if (OperatingSystem.IsLinux())
                            Process.Start("xdg-open", url);
                    }
                    catch
                    {
                        // Ignore errors when opening browser
                    }
                });

                await app.RunAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine("######### FAILED TO START OR UNHANDLED RUNTIME EXCEPTION #########");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("######### WAITING 10s BEFORE SHUTTING DOWN #########");

                await Task.Delay(TimeSpan.FromSeconds(10));

                throw;
            }
        }

        private static void PrintLogo()
        {
            string logo = $@"
                   ,,
                  ,,,,*
                *,,,,,
               ,,,,,,,   .
             ,**,,,,,,   ,,
            **** ,,,,,   ,,,,
           /**** ,,,,,,   ,,,,                          #) Tetracontrol muss lokal laufen
           //**   ,,,,,,   ,,,,,
       (   ///**   ,,,,,,   *,,,,                       #) WebServer auf Host <TetraControlHost> und
      ((   ///**    ,,,,,,.  ,,,,,                         mit Port <TetraControlPort> starten
      ((    ///**,    ,,,,,,  ,,,,                         Default: localhost:80
      #((    ///***    ,,,,,* ,,,,                      
      #(((*   *//***    ,,,,, ,,,,                      #) Benutzer: <TetraControlUsername> mit
       #((((    /****    ,,,,,,,,                          Passwort: <TetraControlPassword> anlegen
         ((((/   /****    ,,,,,,                           Default: Connect und Connect
          ((((//  /****   ,,,,
            (((//  /***   ,,,                           #) Konfiguration über Admin UI auf Port 5050
             (((// /***   ,                                             
               (/////*                                          Version: {Constants.Version} 
                (/////                              TetraControl Connector für unbegrenzt viele Connect-Standorte
                  ////                                Feuer Software GmbH | Karlsbaderstr. 16 | 65760 Eschborn
                   //";

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(logo);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
        }

        private static AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(r => r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .OrResult(r => r.StatusCode == System.Net.HttpStatusCode.Conflict)
                .WaitAndRetryAsync(
                    3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(retryAttempt, 2)));
        }
    }
}
