using FeuerSoftware.TetraControl2Connect.Extensions;
using FeuerSoftware.TetraControl2Connect.Models.Connect;
using FeuerSoftware.TetraControl2Connect.Models.TetraControl;
using FeuerSoftware.TetraControl2Connect.Shared.Options;
using FeuerSoftware.TetraControl2Connect.Shared.Options.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;

namespace FeuerSoftware.TetraControl2Connect.Services
{
    public class SirenService : ISirenService
    {
        private const string SirenDefectReportCategory = "TETRA-Sirenen";
        private const string SonnenburgHeartbeatStatus = "E004";
        private const string Sirene24HeartbeatStatus = "STATUS=0";
        private const string SirenMalfunctionDescriptionKeyword = "Sirenen-Störung";
        private const string HeartbeatFileName = "heartbeats.json";

        private readonly ILogger<SirenService> _log;
        private readonly Serilog.ILogger _sirenLog;
        private readonly IConnectApiService _connectApiService;
        private readonly IOptionsMonitor<ConnectOptions> _connectOptions;
        private readonly IOptionsMonitor<SirenStatusOptions> _sirenStatusOptions;
        private Dictionary<string, DateTime> _sirenHeartbeats = [];
        private readonly IDisposable? _sirenWatchdogSubscription;

        public SirenService(ILogger<SirenService> log,
        IConnectApiService connectApiService,
        IOptionsMonitor<ConnectOptions> connectOptions,
        IOptionsMonitor<SirenStatusOptions> sirenStatusOptions,
        Serilog.ILogger sirenLog)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _connectApiService = connectApiService ?? throw new ArgumentNullException(nameof(connectApiService));
            _connectOptions = connectOptions ?? throw new ArgumentNullException(nameof(connectOptions));
            _sirenStatusOptions = sirenStatusOptions ?? throw new ArgumentNullException(nameof(sirenStatusOptions));
            _sirenLog = sirenLog ?? throw new ArgumentNullException(nameof(sirenLog));

            _sirenWatchdogSubscription = Observable.Interval(TimeSpan.FromMinutes(5)).SubscribeAsyncSafe(async _ =>
            {
                foreach (var siren in GetConfiguredSirensWithHeartbeatInterval())
                {
                    var lastHeartbeat = _sirenHeartbeats[siren.Issi];
                    var timeRemaining = lastHeartbeat.Add(siren.ExpectedHeartbeatInterval!.Value) - DateTime.Now;
                    var tolerance = TimeSpan.FromMinutes(5);
                    var heartbeatIsOverdue = timeRemaining < -tolerance;

                    if (!heartbeatIsOverdue)
                    {
                        _log.LogDebug("Heartbeat for siren {SirenName} (ISSI {Issi}) is on schedule. Last heartbeat was {LastHeartbeat}. Time remaining {TimeRemaining} – no issues detected.", siren.Name, siren.Issi, lastHeartbeat, timeRemaining);
                        continue;
                    }

                    _log.LogInformation("Siren {SirenName} (ISSI {Issi}) missed its heartbeat – issue detected! Last heartbeat was {LastHeartbeat}", siren.Name, siren.Issi, lastHeartbeat);

                    var sirenSites = _connectOptions.CurrentValue.Sites
                        .Where(site => site.Sirens.Any(siren => siren.Issi == siren.Issi));

                    foreach (var site in sirenSites)
                    {
                        var siteSiren = site.Sirens.Single(s => s.Issi == siren.Issi);
                        await CreateDefectReportForSiteIfNotExists(site, siteSiren, "Sirene offline oder gestört", $"Ausgebliebener Heartbeat. Letzte Meldung der Sirene war {lastHeartbeat}.");
                    }
                }
            },
            ex => _log.LogError(ex, "Error on siren heartbeat watchdog."),
            () => _log.LogDebug("Siren heartbeat watchdog subscription completed."));
        }

        private List<Siren> GetConfiguredSirensWithHeartbeatInterval()
        {
            return [.. _connectOptions.CurrentValue.Sites
                .SelectMany(s => s.Sirens)
                .Where(s => s.ExpectedHeartbeatInterval != null)
                .DistinctBy(s => s.Issi)];
        }

        public async Task Initialize()
        {
            _log.LogDebug($"Initializing {nameof(SirenService)}.");

            var savedHeartbeats = await ReadHeartbeatsFromFileOrDefault();

            if (savedHeartbeats is null || savedHeartbeats.Keys.Count != GetConfiguredSirensWithHeartbeatInterval().Count)
            {
                _log.LogWarning("No saved heartbeats found or configuration has changed. Looked for file {Filename} in current working directory.", HeartbeatFileName);

                foreach (var siren in GetConfiguredSirensWithHeartbeatInterval())
                {
                    _log.LogDebug("Setting last heartbeat for siren {SirenName} with ISSI {SirenISSI} to {Now}", siren.Name, siren.Issi, DateTime.Now);
                    _sirenHeartbeats[siren.Issi] = DateTime.Now;
                }

                await SaveHeartbeatsToFile();
            }
            else
            {
                _sirenHeartbeats = savedHeartbeats;
                _log.LogDebug("Restored saved heartbeats: {@SavedHeartbeats}", savedHeartbeats);
            }
        }

        public async Task HandleSirenStatuscode(TetraControlDto dto)
        {
            var malfunctionTranslationKey = _sirenStatusOptions.CurrentValue.FailureTranslations.Keys
                .Where(k => dto.Text.Contains(k, StringComparison.InvariantCultureIgnoreCase) || dto.StatusCode.Equals(k, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault();

            _sirenLog.Information("Received [{StatusCode}] {Text} from {SourceName} ({Issi})", dto.StatusCode, dto.Text, dto.SourceName, dto.SourceSSI);

            if (malfunctionTranslationKey is null)
            {
                if (dto.StatusCode.Equals(SonnenburgHeartbeatStatus, StringComparison.InvariantCultureIgnoreCase) || dto.Text.Contains(Sirene24HeartbeatStatus, StringComparison.InvariantCultureIgnoreCase))
                {
                    _log.LogInformation("Siren status message is heartbeat.");

                    var sirenWithHeartbeatConfigured = GetConfiguredSirensWithHeartbeatInterval().SingleOrDefault(s => s.Issi == dto.SourceSSI);

                    if (sirenWithHeartbeatConfigured is null)
                    {
                        _log.LogWarning("Siren with ISSI {Issi} is not configured for acception heartbeats. Ignoring.", dto.SourceSSI);
                        return;
                    }

                    _log.LogDebug("Setting {Now} as last heartbeat for siren {Name} with ISSI {Issi}", DateTime.Now, sirenWithHeartbeatConfigured.Name, sirenWithHeartbeatConfigured.Issi);
                    _sirenHeartbeats[sirenWithHeartbeatConfigured.Issi] = DateTime.Now;

                    await SaveHeartbeatsToFile();
                    await CheckAndResolveAllDefectReportsForSiren(sirenWithHeartbeatConfigured.Issi);
                }

                _log.LogWarning("Siren status message {@Message} is not known. Skipping.", dto);

                return;
            }

            var translation = _sirenStatusOptions.CurrentValue.FailureTranslations[malfunctionTranslationKey];
            _log.LogInformation("Siren status message is malfunction for translation {Translation}.", translation);

            var sirenSites = _connectOptions.CurrentValue.Sites.Where(site => site.Sirens.Any(siren => siren.Issi == dto.SourceSSI));

            _log.LogDebug("Sites for siren status are: {@Sites}", sirenSites);

            foreach (var site in sirenSites)
            {
                var siren = site.Sirens.Single(s => s.Issi == dto.SourceSSI);

                var originalData = string.IsNullOrWhiteSpace(dto.StatusCode) ? dto.Text : dto.StatusCode;
                await CreateDefectReportForSiteIfNotExists(site, siren, translation, originalData);
            }
        }

        private async Task CheckAndResolveAllDefectReportsForSiren(string issi)
        {
            var sirenSites = _connectOptions.CurrentValue.Sites.Where(site => site.Sirens.Any(siren => siren.Issi == issi));

            foreach (var site in sirenSites)
            {
                var siren = site.Sirens.Single(s => s.Issi == issi);

                var defectReports = await _connectApiService.GetDefectReports(site.Key);
                var activeDefectReportsForThisSiren = defectReports
                    .Where(d => !d.IsClosed())
                    .Where(d => d.ShortDescription.Contains(SirenMalfunctionDescriptionKeyword, StringComparison.InvariantCultureIgnoreCase) &&
                    d.ShortDescription.Contains(siren.Name, StringComparison.InvariantCultureIgnoreCase))
                    .ToList();

                if (activeDefectReportsForThisSiren.Count == 0) return;

                _log.LogInformation("There are {Count} unresolved defect reports for this siren. Siren is sending heartbeat again, resolving defect reports...", activeDefectReportsForThisSiren.Count);

                foreach (var defectReport in activeDefectReportsForThisSiren)
                {
                    defectReport.Status = DefectReportStatus.Resolved;
                    defectReport.DetailedDescription += "\n Störung automatisch durch TetraControl2Connect wieder aufgelöst, nachdem Sirene sich wieder klar gemeldet hat.";

                    await _connectApiService.PutDefectReport(defectReport.Id, defectReport, site.Key);
                }
            }

        }

        private async Task CreateDefectReportForSiteIfNotExists(Site site, Siren siren, string errorCause, string description)
        {
            var siteInfo = await _connectApiService.GetOrganizationInfo(site.Key) ?? throw new InvalidOperationException("Could not retrieve SiteInfo");
            var shortDescription = $"{SirenMalfunctionDescriptionKeyword}: {siren.Name} | {errorCause}";

            var defectReports = await _connectApiService.GetDefectReports(site.Key);

            var existingDefectReport = defectReports
                .FirstOrDefault(d => !d.IsClosed() &&
                (d.CreatedAt - DateTime.Now).Duration() <= TimeSpan.FromDays(7) &&
                d.ShortDescription.Contains(errorCause, StringComparison.InvariantCultureIgnoreCase) &&
                d.ShortDescription.Contains(SirenMalfunctionDescriptionKeyword, StringComparison.InvariantCultureIgnoreCase) &&
                d.ShortDescription.Contains(siren.Name, StringComparison.InvariantCultureIgnoreCase));

            if (existingDefectReport is not null)
            {
                _log.LogInformation("There is already an active defect report for this failure reason ({DefectReportSequenceNumber}) in site {SiteName}. Skipping", existingDefectReport.SequenceNumber, siteInfo.Name);

                return;
            }

            var categories = await _connectApiService.GetDefectReportCategories(site.Key);
            var sirenCategory = categories.FirstOrDefault(c => c.SiteId.HasValue && c.Name == SirenDefectReportCategory);

            if (sirenCategory is null)
            {
                _log.LogInformation("Default DefectReportCategory {Category} for sirens does not exist. Creating...", SirenDefectReportCategory);

                await _connectApiService.PostDefectReportCategory(new DefectReportCategoryModel
                {
                    Name = SirenDefectReportCategory,
                    SiteId = siteInfo!.Sites.First().Id,
                }, site.Key);

                categories = await _connectApiService.GetDefectReportCategories(site.Key);
                sirenCategory = categories.FirstOrDefault(c => c.SiteId.HasValue && c.Name == SirenDefectReportCategory);
            }

            var bobTheBuilder = new StringBuilder();
            bobTheBuilder.AppendLine($"Es besteht eine Störung bei Sirene '{siren.Name}' (ISSI: {siren.Issi}).");
            bobTheBuilder.AppendLine();
            bobTheBuilder.AppendLine($"Störungsgrund: {errorCause}");
            bobTheBuilder.AppendLine($"Originalmeldung: {description}");
            bobTheBuilder.AppendLine();
            bobTheBuilder.AppendLine($"Zeitstempel Störungseingang: {DateTime.Now:G}");
            bobTheBuilder.AppendLine($"Quelle: TetraControl2Connect {Constants.Version}");
            bobTheBuilder.AppendLine();
            bobTheBuilder.AppendLine($"Gleichartige Störungen dieser Sirene werden solange unterdrückt, bis diese Mängelmeldung geschlossen wird oder 7 Tage verstrichen sind.");

            var defectReport = new DefectReportModel
            {
                SiteId = siteInfo!.Sites.First().Id,
                Priority = Priority.High,
                Status = DefectReportStatus.Reported,
                ShortDescription = shortDescription,
                DetailedDescription = bobTheBuilder.ToString(),
                CategoryId = sirenCategory?.Id,
            };

            _log.LogDebug("Defect report: {@DefectReport}", defectReport);

            _log.LogInformation("Creating defect report for site {SiteName}...", site.Name);

            await _connectApiService.PostDefectReport(defectReport, site.Key);
        }

        private async Task SaveHeartbeatsToFile()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };

            var jsonString = JsonSerializer.Serialize(_sirenHeartbeats, options);
            var currentDirectory = Directory.GetCurrentDirectory();
            var fullPath = Path.Combine(currentDirectory, HeartbeatFileName);

            await File.WriteAllTextAsync(fullPath, jsonString);
        }

        private async Task<Dictionary<string, DateTime>?> ReadHeartbeatsFromFileOrDefault()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string fullPath = Path.Combine(currentDirectory, HeartbeatFileName);

            if (!File.Exists(fullPath))
            {
                return null;
            }

            var jsonString = await File.ReadAllTextAsync(fullPath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<Dictionary<string, DateTime>>(jsonString, options);
        }

        public void Dispose()
        {
            _sirenWatchdogSubscription?.Dispose();
        }
    }
}
