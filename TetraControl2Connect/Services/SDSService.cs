using FeuerSoftware.TetraControl2Connect.Extensions;
using FeuerSoftware.TetraControl2Connect.Models.Connect;
using FeuerSoftware.TetraControl2Connect.Models.TetraControl;
using FeuerSoftware.TetraControl2Connect.Shared;
using FeuerSoftware.TetraControl2Connect.Shared.Options;
using FeuerSoftware.TetraControl2Connect.Shared.Options.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reactive;
using System.Text.RegularExpressions;

namespace FeuerSoftware.TetraControl2Connect.Services
{
    public class SDSService : ISDSService
    {
        private sealed record RegexCache(
            Regex Keyword,
            Regex Facts,
            Regex Street,
            Regex HouseNumber,
            Regex City,
            Regex District,
            Regex ZipCode,
            Regex Longitude,
            Regex Latitude,
            Regex ReporterName,
            Regex ReporterPhoneNumber,
            Regex Ric,
            Regex Number);

        private static readonly RegexCache EmptyRegexCache = new(
            new(string.Empty), new(string.Empty), new(string.Empty),
            new(string.Empty), new(string.Empty), new(string.Empty),
            new(string.Empty), new(string.Empty), new(string.Empty),
            new(string.Empty), new(string.Empty), new(string.Empty),
            new(string.Empty));

        private readonly ILogger<SDSService> _log;
        private readonly IConnectApiService _connectApiService;
        private readonly IOptionsMonitor<ConnectOptions> _connectOptions;
        private readonly IOptionsMonitor<PatternOptions> _patternOptions;
        private readonly IOptionsMonitor<ProgramOptions> _programOptions;
        private readonly IOptionsMonitor<StatusOptions> _statusOptions;
        private readonly IUserService _userService;
        private readonly ISitesService _sitesService;
        private readonly IOptionsMonitor<SeverityOptions> _severityOptions;
        private readonly IOptionsMonitor<SirenCalloutOptions> _sirenCalloutOptions;
        private volatile RegexCache _regexCache = EmptyRegexCache;
        private volatile AsyncRetryPolicy _updateOperationRetryPolicy = null!;
        private IDisposable? _patternOptionsChangeSubscription;
        private IDisposable? _programOptionsChangeSubscription;
        private readonly ConcurrentDictionary<int, DateTime> _seenCalloutsByReference = new();
        private readonly ConcurrentDictionary<string, DateTime> _fallbackPublishes = new();

        public SDSService(
            ILogger<SDSService> log,
            IConnectApiService connectApiService,
            IUserService userService,
            IOptionsMonitor<ConnectOptions> connectOptions,
            IOptionsMonitor<ProgramOptions> programOptions,
            IOptionsMonitor<StatusOptions> statusOptions,
            ISitesService sitesService,
            IOptionsMonitor<SeverityOptions> severityOptions,
            IOptionsMonitor<SirenCalloutOptions> sirenCalloutOptions,
            IOptionsMonitor<PatternOptions> patternOptions)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _connectApiService = connectApiService ?? throw new ArgumentNullException(nameof(connectApiService));
            _connectOptions = connectOptions ?? throw new ArgumentNullException(nameof(connectOptions));
            _patternOptions = patternOptions ?? throw new ArgumentNullException(nameof(patternOptions));
            _programOptions = programOptions ?? throw new ArgumentNullException(nameof(programOptions));
            _statusOptions = statusOptions ?? throw new ArgumentNullException(nameof(statusOptions));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _sitesService = sitesService ?? throw new ArgumentNullException(nameof(sitesService));
            _severityOptions = severityOptions ?? throw new ArgumentNullException(nameof(severityOptions));
            _sirenCalloutOptions = sirenCalloutOptions ?? throw new ArgumentNullException(nameof(sirenCalloutOptions));

            BuildRegexes();
            BuildRetryPolicy();

            _patternOptionsChangeSubscription = _patternOptions.OnChange((_, _) =>
            {
                try { BuildRegexes(); }
                catch (Exception ex) { _log.LogError(ex, "Failed to rebuild regex patterns from updated options."); }
            });
            _programOptionsChangeSubscription = _programOptions.OnChange((_, _) =>
            {
                try
                {
                    BuildRegexes();
                    BuildRetryPolicy();
                }
                catch (Exception ex) { _log.LogError(ex, "Failed to rebuild from updated program options."); }
            });
        }

        private void BuildRegexes()
        {
            if (_programOptions.CurrentValue.AcceptSDSAsCalloutsWithPattern)
            {
                _regexCache = new RegexCache(
                    Keyword: new Regex(_patternOptions.CurrentValue.KeywordPattern, RegexOptions.Compiled),
                    Facts: new Regex(_patternOptions.CurrentValue.FactsPattern, RegexOptions.Compiled),
                    Street: new Regex(_patternOptions.CurrentValue.StreetPattern, RegexOptions.Compiled),
                    HouseNumber: new Regex(_patternOptions.CurrentValue.HouseNumberPattern, RegexOptions.Compiled),
                    City: new Regex(_patternOptions.CurrentValue.CityPattern, RegexOptions.Compiled),
                    District: new Regex(_patternOptions.CurrentValue.DistrictPattern, RegexOptions.Compiled),
                    ZipCode: new Regex(_patternOptions.CurrentValue.ZipCodePattern, RegexOptions.Compiled),
                    Longitude: new Regex(_patternOptions.CurrentValue.LongitudePattern, RegexOptions.Compiled),
                    Latitude: new Regex(_patternOptions.CurrentValue.LatitudePattern, RegexOptions.Compiled),
                    ReporterName: new Regex(_patternOptions.CurrentValue.ReporterNamePattern, RegexOptions.Compiled),
                    ReporterPhoneNumber: new Regex(_patternOptions.CurrentValue.ReporterPhoneNumberPattern, RegexOptions.Compiled),
                    Ric: new Regex(_patternOptions.CurrentValue.RicPattern, RegexOptions.Compiled),
                    Number: new Regex(_patternOptions.CurrentValue.NumberPattern, RegexOptions.Compiled));
            }
        }

        private void BuildRetryPolicy()
        {
            _updateOperationRetryPolicy = Policy
                .Handle<InvalidOperationException>()
                .WaitAndRetryAsync(
                    _programOptions.CurrentValue.PollForActiveOperationBeforeFallbackMaxRetryCount,
                    retryCount => _programOptions.CurrentValue.PollForActiveOperationBeforeFallbackDelay,
                    (exception, sleepDuration, retryCount, context) => _log.LogInformation($"Operation is not present. Exception: '{exception.Message}' Retrying (Attempt '{retryCount}' after '{sleepDuration}')..."));
        }

        public void Dispose()
        {
            _patternOptionsChangeSubscription?.Dispose();
            _programOptionsChangeSubscription?.Dispose();
        }

        public async Task HandleSds(TetraControlDto sds)
        {
            var sdsType = sds.GetSdsType();

            _log.LogDebug("SDS is of type {SdsType}", sdsType);

            switch (sdsType)
            {
                case SdsType.Callout:
                    if (!_programOptions.CurrentValue.SendAlarms)
                    {
                        _log.LogDebug($"Ignoring SDS because {nameof(_programOptions.CurrentValue.SendAlarms)} is disabled in program options.");
                        return;
                    }

                    await HandleCallout(sds);
                    break;
                case SdsType.CalloutFeedback:
                    if (!_programOptions.CurrentValue.SendUserOperationStatus)
                    {
                        _log.LogDebug($"Ignoring SDS because {nameof(_programOptions.CurrentValue.SendUserOperationStatus)} is disabled in program options.");
                        return;
                    }

                    await HandleCalloutFeedback(sds);
                    break;
                case SdsType.TacticalAvailability:
                    if (!_programOptions.CurrentValue.SendUserAvailability)
                    {
                        _log.LogDebug($"Ignoring SDS because {nameof(_programOptions.CurrentValue.SendUserAvailability)} is disabled in program options.");
                        return;
                    }

                    await HandleTacticalAvailability(sds);
                    break;
                case SdsType.Unknown:
                default:
                    if (!_programOptions.CurrentValue.AcceptSDSAsCalloutsWithPattern)
                    {
                        _log.LogWarning($"Because {nameof(_programOptions.CurrentValue.AcceptSDSAsCalloutsWithPattern)} is false: Ignoring SDS...");
                        break;
                    }

                    await HandleInfo(sds);
                    break;
            }
        }

        private async Task HandleCallout(TetraControlDto sds)
        {
            var isSirenCallout = sds.IsCalloutForSirens();
            if (isSirenCallout && !_programOptions.CurrentValue.AcceptCalloutsForSirens)
            {
                _log.LogInformation("SDS is Alarm for Sirens and AcceptCalloutsForSirens is set to false. Ignoring...");

                return;
            }

            if (sds.IsCalloutSelfSent())
            {
                _log.LogInformation("Callout is self sent. Ignoring.");

                return;
            }

            var snasRaw = sds.ExtractSNAs();

            _log.LogInformation("Extracted SNAs from Callout are: {@Snas}", snasRaw);

            IEnumerable<Site> sitesForNormalAlarm;
            IEnumerable<Site> sitesForDirectAlarm;
            IEnumerable<SubnetAddress> snas;

            var isAlarmForGssi = snasRaw.Count == 0;

            if (isAlarmForGssi)
            {
                _log.LogInformation("Could not extract any SNA from SDS. Must be alarm for GSSI without subnet addresses. Collecting all sites with this GSSI...");

                if (_programOptions.CurrentValue.IgnoreAlarmWithoutSubnetAddresses)
                {
                    _log.LogWarning("IgnoreAlarmWithoutSubnetAddresses is set to true. Ignoring callout for GSSI.");
                    return;
                }

                snas = _connectOptions.CurrentValue.Sites
                    .SelectMany(s => s.SubnetAddresses)
                    .Where(sna => sna.GSSI == sds.DestinationSSI)
                    .ToList();

                _log.LogDebug("Collected SNAs from Configuration are with GSSI '{Gssi}: {snas}'", sds.DestinationSSI, snas);

                sitesForNormalAlarm = _connectOptions.CurrentValue.Sites
                    .Where(s => s.SubnetAddresses.Any(sna => sna.GSSI == sds.DestinationSSI && sna.AlarmDirectly == false));
                sitesForDirectAlarm = _connectOptions.CurrentValue.Sites
                    .Where(s => s.SubnetAddresses.Any(sna => sna.GSSI == sds.DestinationSSI && sna.AlarmDirectly == true));
            }
            else
            {
                snas = _connectOptions.CurrentValue.Sites
                    .SelectMany(s => s.SubnetAddresses)
                    .Where(sna => snasRaw.Contains(sna.SNA) && sna.GSSI == sds.DestinationSSI)
                    .ToList();
                _log.LogInformation("Collected SNAs from Configuration are '{@snas}' and GSSI is '{@destinationIssi}'.", snas, sds.DestinationSSI);

                sitesForNormalAlarm = _connectOptions.CurrentValue.Sites
                    .Where(s => s.SubnetAddresses.Any(sna => snasRaw.Contains(sna.SNA) && sna.GSSI == sds.DestinationSSI && sna.AlarmDirectly == false));
                sitesForDirectAlarm = _connectOptions.CurrentValue.Sites
                    .Where(s => s.SubnetAddresses.Any(sna => snasRaw.Contains(sna.SNA) && sna.GSSI == sds.DestinationSSI && sna.AlarmDirectly == true));
            }

            var anySitesForNormalAlarm = sitesForNormalAlarm.Any();
            var anySitesForDirectAlarm = sitesForDirectAlarm.Any();

            if (!anySitesForNormalAlarm)
            {
                _log.LogInformation("No sites for normal Alarm for SNAs '{@snas}' and GSSI '{DestinationSSI}' found. Skipping.",
                    snasRaw,
                    sds.DestinationSSI);
            }

            if (!anySitesForDirectAlarm)
            {
                _log.LogInformation("No sites for direct Alarm for SNAs '{@snas}' and GSSI '{DestinationSSI}' found.",
                    snasRaw,
                    sds.DestinationSSI);
            }

            if (!anySitesForDirectAlarm && !anySitesForNormalAlarm)
            {
                _log.LogWarning("Dont have any sites for direct or normal alarm. Skipping.");
                return;
            }

            var calloutRef = sds.ExtractCalloutReference();
            _log.LogDebug("Extracted Callout-Reference is {Reference}", calloutRef);

            if (calloutRef is not null)
            {
                var calloutAlreadySeen = _seenCalloutsByReference.TryGetValue(calloutRef.Value, out var timeStamp);

                if (calloutAlreadySeen && (DateTime.Now - timeStamp).Duration() <= TimeSpan.FromMinutes(3))
                {
                    _log.LogInformation("Callout already seen at {Time}. Ignoring...", timeStamp);

                    return;
                }

                _seenCalloutsByReference[calloutRef.Value] = DateTime.Now;
                _log.LogDebug("CalloutRef {CalloutRef} stored with lastSeen {LastSeen}", calloutRef, DateTime.Now);
            }


            var normalAlarmTasks = sitesForNormalAlarm.Select(async s =>
            {
                var result = await _updateOperationRetryPolicy.ExecuteAndCaptureAsync(async () => await UpdateOperationWithSNA(s, snas, sds, isAlarmForGssi));

                if (result.Outcome == OutcomeType.Failure)
                {
                    await CreateOperation(s, snas, sds, isAlarmForGssi, isSirenCallout);
                }
            });

            var directAlarmTasks = sitesForDirectAlarm.Select(async s =>
            {
                await CreateOperation(s, snas, sds, isAlarmForGssi, isSirenCallout);
            });

            await Task.WhenAll(normalAlarmTasks.Concat(directAlarmTasks));
        }

        private async Task HandleCalloutFeedback(TetraControlDto sds)
        {
            var statuscode = sds.ExtractStatuscode();
            _log.LogDebug("Extracted Statuscode '{statuscode}'.", statuscode);

            if (!statuscode.HasValue || statuscode is null)
            {
                _log.LogWarning("Could not extract statuscode from remark. Maybe wrong format.");
                return;
            }

            async Task PostUserOperationStatus(UserOperationStatus status)
            {
                var accessTokens = _userService.GetAccessTokensForUser(sds.SourceSSI);

                if (accessTokens.Count == 0)
                {
                    _log.LogDebug("Could not find users with ISSI '{SourceSSI}' in our user list. Ignoring...", sds.SourceSSI);
                    return;
                }

                var users = _userService.GetUsers(sds.SourceSSI);

                if (accessTokens is not null)
                {
                    foreach (var token in accessTokens)
                    {
                        // TODO: Send OperationStatus only once per Organization
                        var siteInfo = _sitesService.GetSiteInfo(token);
                        var user = users.Single(u => u.OrganizationId == siteInfo.OrganizationId);

                        var userStatus = new UserStatusModel()
                        {
                            Status = status,
                            PagerIssi = sds.SourceSSI,
                        };

                        await _connectApiService.PostUserStatus(token, userStatus);
                        _log.LogInformation(
                            "Sent user status '{status}' for user '{FirstName} {LastName}' ISSI '{PagerIssi}' to Connect. [{SiteName}]",
                            status,
                            user.FirstName,
                            user.LastName,
                            sds.SourceSSI,
                            siteInfo.Name);
                    }
                }
            };

            if (_statusOptions.CurrentValue.ComingStatus.SplitToIntArray(';').Contains(statuscode.Value))
            {
                await PostUserOperationStatus(UserOperationStatus.Coming);
                return;
            }

            if (_statusOptions.CurrentValue.NotComingStatus.SplitToIntArray(';').Contains(statuscode.Value))
            {
                await PostUserOperationStatus(UserOperationStatus.NotComing);
                return;
            }

            if (_statusOptions.CurrentValue.ComingLaterStatus.SplitToIntArray(';').Contains(statuscode.Value))
            {
                await PostUserOperationStatus(UserOperationStatus.ComingLater);
                return;
            }
        }

        private async Task HandleTacticalAvailability(TetraControlDto sds)
        {
            var statuscode = sds.ExtractStatuscode();
            _log.LogDebug("Extracted Statuscode {statuscode}.", statuscode);

            if (!statuscode.HasValue || statuscode is null)
            {
                _log.LogWarning("Could not extract statuscode from remark. Maybe wrong format.");
                return;
            }

            async Task PutAvailabilityStatus(AvailabilityStatus status)
            {
                // TODO: Send AvailabilityStatus only once per Organization
                var accessTokens = _userService.GetAccessTokensForUser(sds.SourceSSI);

                if (accessTokens.Count == 0)
                {
                    _log.LogDebug("Could not find users with ISSI {SourceSSI} in our user list. Ignoring...", sds.SourceSSI);
                    return;
                }

                var users = _userService.GetUsers(sds.SourceSSI);
                foreach (var token in accessTokens)
                {
                    var siteInfo = _sitesService.GetSiteInfo(token);
                    var user = users.Single(u => u.OrganizationId == siteInfo.OrganizationId);

                    var availabiltyModel = new UserAvailabilityModel()
                    {
                        Status = status,
                        Until = DateTime.Now.AddDays(_programOptions.CurrentValue.UserAvailabilityLifetimeDays),
                    };

                    await _connectApiService.PutUserAvailability(token, user.Id, availabiltyModel);
                    _log.LogInformation("Sent availability update {Status} for user {FirstName} {LastName} ISSI {PagerIssi} to Connect. [{SiteName}]",
                        status,
                        user.FirstName,
                        user.LastName,
                        sds.SourceSSI,
                        siteInfo.Name);
                }
            }
            ;

            if (_statusOptions.CurrentValue.AvailableStatus.SplitToIntArray(';').Contains(statuscode.Value))
            {
                await PutAvailabilityStatus(AvailabilityStatus.Available);
                return;
            }

            if (_statusOptions.CurrentValue.NotAvailableStatus.SplitToIntArray(';').Contains(statuscode.Value))
            {
                await PutAvailabilityStatus(AvailabilityStatus.NotAvailable);
                return;
            }

            if (_statusOptions.CurrentValue.LimitedAvailableStatus.SplitToIntArray(';').Contains(statuscode.Value))
            {
                await PutAvailabilityStatus(AvailabilityStatus.LimitedAvailable);
                return;
            }
        }

        private async Task UpdateOperationWithSNA(Site site, IEnumerable<SubnetAddress> snas, TetraControlDto sds, bool isGSSIAlarm)
        {
            var activeOperations = await _connectApiService.GetLatestOperations(site.Key);

            if (!activeOperations.Any())
            {
                throw new InvalidOperationException($"No active operation for site '{site.Name}'.");
            }

            if (activeOperations.All(o => !o.IsRecentlyAlertedOrUpdated()))
            {
                throw new InvalidOperationException($"All active operations for site '{site.Name}' are outdated.");
            }

            if (!_programOptions.CurrentValue.UpdateExistingOperations)
            {
                _log.LogInformation($"Found recently alerted or updated operation. Will not update this operation because {nameof(_programOptions.CurrentValue.UpdateExistingOperations)} is false.");

                return;
            }

            var operationToUpdate = activeOperations
                .Where(o => o.IsRecentlyAlertedOrUpdated())
                .OrderByDescending(o => o.CreatedAt)
                .ThenByDescending(o => o.LastUpdateAt)
                .First();

            var snasToAdd = snas.Select(sna => sna.ToStringForConnect(_programOptions.CurrentValue.UseFullyQualifiedSubnetAddressForConnect));

            if (!string.IsNullOrEmpty(operationToUpdate.Ric) && snasToAdd.All(s => operationToUpdate.Ric.Contains(s)))
            {
                _log.LogInformation($"Operation already contains all SNAs. Not updating this operation.");
                return;
            }

            var ricToAppend = $" | {string.Join(" | ", snasToAdd)}";

            if (isGSSIAlarm)
            {
                ricToAppend += $" | T2C(VOLLALARM GSSI {sds.DestinationSSI})";
            }

            operationToUpdate.Ric += ricToAppend;
            _log.LogDebug("Updating Operation with SNAs in RIC: {@Operation}", operationToUpdate);

            if (!operationToUpdate.Properties.Any(p => p.Key == "Alarmtext TC") && _programOptions.CurrentValue.AddPropertyForAlarmTexts)
            {
                var property = new OperationPropertyModel() { Key = "Alarmtext TC", Value = sds.Text.RemoveSubnetAddresses() };
                operationToUpdate.Properties.Add(property);

                _log.LogDebug("Added Property {Property}", property);
            }

            // Map incoming coordinates to CreateOperationModel from Connect
            // Otherwise the coordinates will be resettet and re-evaluated from OSM in Connect
            if ((operationToUpdate.Address?.Lat.HasValue ?? false) && (operationToUpdate.Address?.Lng.HasValue ?? false))
            {
                operationToUpdate.Position = new PositionModel()
                {
                    Latitude = operationToUpdate.Address.Lat.Value,
                    Longitude = operationToUpdate.Address.Lng.Value,
                };
            }

            if (!operationToUpdate.Source?.Contains("TetraControl2Connect") ?? false)
            {
                operationToUpdate.Source = $"{operationToUpdate.Source} (+ TetraControl2Connect)";
            }

            await _connectApiService.PostOperation(site.Key, operationToUpdate, UpdateStrategy.ByNumber);

            _log.LogInformation("Updated active Operation with number {Number} with SNAs.", operationToUpdate.Number);
        }

        private async Task HandleInfo(TetraControlDto sds)
        {
            var operation = ResolveOperationWithPattern(sds.Text);

            _log.LogDebug("Resolved operation from pattern with normal SDS: {@Operation}", operation);

            if (string.IsNullOrWhiteSpace(operation.Ric))
            {
                _log.LogWarning("RIC was null or whitespace. Cant evaluate sites to alarm. Abort.");
                return;
            }

            _log.LogDebug($"Extracted RIC is: {operation.Ric}");

            var sitesForAlarm = _connectOptions.CurrentValue.Sites
                .Where(s => s.SubnetAddresses
                    .Any(sa => operation.Ric.Contains(sa.Name)));

            _log.LogInformation($"Evaluated Sites for Alarm are: {string.Join(", ", sitesForAlarm.Select(s => s.Name))}");

            var alarmTasks = sitesForAlarm.Select(async s =>
            {
                _log.LogDebug("Send operation for site {SiteName} to Connect...", s.Name);
                await _connectApiService.PostOperation(s.Key, operation, UpdateStrategy.ByNumber);
                _log.LogInformation("Successfully sent operation for site {SiteName} to Connect...", s.Name);
            });

            await Task.WhenAll(alarmTasks).ConfigureAwait(false);
        }

        private async Task CreateOperation(Site site, IEnumerable<SubnetAddress> snas, TetraControlDto sds, bool isGSSIAlarm, bool isSirenCallout)
        {
            OperationModel operation;

            if (_patternOptions.CurrentValue.IsEnabled)
            {
                try
                {
                    operation = ResolveOperationWithPattern(sds.Text);
                    var ric = string.Join(" | ", snas.Select(sna => sna
                        .ToStringForConnect(_programOptions.CurrentValue.UseFullyQualifiedSubnetAddressForConnect))
                        .Distinct());

                    if (isGSSIAlarm)
                    {
                        ric += $" | T2C(VOLLALARM GSSI {sds.DestinationSSI})";
                    }
                    operation.Ric += " | " + ric;

                    if (string.IsNullOrWhiteSpace(operation.Number))
                    {
                        operation.Number = sds.GetFallbackOperationNumberForConnect();
                    }
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "Could not extract values with pattern correctly. Falling back to Fallback-Operation.");
                    operation = ResolveFallbackOperation(site, snas, sds, isGSSIAlarm, isSirenCallout);
                }
            }
            else
            {
                operation = ResolveFallbackOperation(site, snas, sds, isGSSIAlarm, isSirenCallout);
            }

            // If there was a fallback within the three 3 minutes, dont create a new fallback and update the existing one
            if (_fallbackPublishes.TryGetValue(site.Key, out var lastFallback) &&
                DateTime.Now - lastFallback <= TimeSpan.FromMinutes(3))
            {
                _log.LogInformation("There was already a fallback alarm for site {Name} within the last 60s. Try to update the last fallback instead of creating another one...", site.Name);

                if (DateTime.Now - lastFallback <= TimeSpan.FromSeconds(10))
                {
                    _log.LogDebug("The last fallback was within the last 5s. Waiting for 5 seconds to proceed...");
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }

                try
                {
                    _log.LogDebug("Trying to update last fallback for site {Name}", site.Name);
                    await UpdateOperationWithSNA(site, snas, sds, isGSSIAlarm);

                    return;
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "There was a fallback alarm for site {Name} at {Timestamp} but we cant update the last fallback due to an error. Creating another fallback...", site.Name, lastFallback);
                }

                _log.LogDebug("Successfully updated last fallback for site {Name}", site.Name);
            }

            _log.LogDebug("Created operation: {@Operation}", operation);

            _fallbackPublishes[site.Key] = DateTime.Now;

            await _connectApiService.PostOperation(site.Key, operation);

            _log.LogInformation("Created fallback operation for site {SiteName}.", site.Name);
        }

        private OperationModel ResolveFallbackOperation(Site site, IEnumerable<SubnetAddress> snas, TetraControlDto sds, bool isGSSIAlarm, bool isSirenCallout)
        {
            var siteInfo = _sitesService.GetSiteInfo(site.Key);

            var hasValidPosition = siteInfo is not null
                && siteInfo.Address.Lat.HasValue
                && siteInfo.Address.Lng.HasValue
                && siteInfo.Address.Lat > 0
                && siteInfo.Address.Lng > 0;

            var ric = string.Join(" | ", snas.Select(sna => sna
                .ToStringForConnect(_programOptions.CurrentValue.UseFullyQualifiedSubnetAddressForConnect))
                .Distinct());

            if (isGSSIAlarm)
            {
                ric += $" | T2C(VOLLALARM GSSI {sds.DestinationSSI})";
            }

            var keyword = Constants.DefaultKeyword;

            if (_severityOptions.CurrentValue.UseServerityTranslationAsKeyword)
            {
                keyword = _severityOptions.CurrentValue.SeverityTranslations.TryGetValue(sds.ExtractCalloutSeverity(), out var severity) ?
                    severity.ToUpper() :
                    Constants.DefaultKeyword;
            }

            if (_sirenCalloutOptions.CurrentValue.UseSirenCodeTranslationAsKeyword && isSirenCallout)
            {
                keyword = _sirenCalloutOptions.CurrentValue.SirenCodeTranslations.TryGetValue(sds.ExtractSirenCode(), out var sirenCodeTranslation) ?
                    sirenCodeTranslation.ToUpper() :
                    Constants.DefaultKeyword;
            }

            var reporterName = sds.SourceName.Equals(sds.SourceSSI, StringComparison.OrdinalIgnoreCase) ? "Digitalfunk" : sds.SourceName;

            return new OperationModel()
            {
                Keyword = keyword,
                Number = sds.GetFallbackOperationNumberForConnect(),
                Properties =
                [
                    new OperationPropertyModel()
                    {
                        Key = "Hinweis",
                        Value = "Alarm aus Rückfallebene TetraControl (Digitalfunk)! Daten unvollständig!",
                    },
                ],
                Start = DateTime.Now,
                Facts = sds.Text.RemoveSubnetAddresses(),
                Ric = ric,
                Source = $"TetraControl2Connect {Constants.Version}",
                Reporter = new ReporterModel()
                {
                    Name = reporterName,
                    PhoneNumber = sds.SourceSSI
                },
                Address = new AddressModel()
                {
                    City = "Unbekannt",
                },
                Position = hasValidPosition ? new PositionModel()
                {
                    Latitude = siteInfo!.Address.Lat!.Value,
                    Longitude = siteInfo!.Address.Lng!.Value,
                } : null
            };
        }

        private OperationModel ResolveOperationWithPattern(string text)
        {
            var cache = _regexCache; // snapshot for thread safety

            var longitude = cache.Longitude.Match(text).Groups[1].Value.Trim();
            var latitude = cache.Latitude.Match(text).Groups[1].Value.Trim();

            // Get the number separator for this culture and replace any others with it
            var separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            // Replace any periods or commas with the current culture separator and parse
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
            longitude = Regex.Replace(longitude, "[.,]", separator);
            latitude = Regex.Replace(latitude, "[.,]", separator);
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.

#pragma warning disable IDE0018 // Inline variable declaration
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            double longitudeValue = 0;
            double latitudeValue = 0;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
#pragma warning restore IDE0018 // Inline variable declaration
            var hasValidPosition = double.TryParse(longitude, out longitudeValue) && double.TryParse(latitude, out latitudeValue);

            var reporterName = cache.ReporterName.Match(text).Groups[1].Value.Trim();
            var reporterPhoneNumber = cache.ReporterPhoneNumber.Match(text).Groups[1].Value.Trim();
            var hasValidReporter = !string.IsNullOrWhiteSpace(reporterName) || !string.IsNullOrWhiteSpace(reporterPhoneNumber);

            var additionalProperties = new List<OperationPropertyModel>();

            foreach (var additionalProperty in _patternOptions.CurrentValue.AdditionalProperties)
            {
                var regex = new Regex(additionalProperty.Pattern);

                var value = regex.Match(text).Groups[1].Value;

                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                additionalProperties.Add(new OperationPropertyModel()
                {
                    Key = additionalProperty.Name,
                    Value = value.Trim(),
                });
            }

            var operation = new OperationModel()
            {
                Number = cache.Number.Match(text).Groups[1].Value.Trim(),
                Keyword = cache.Keyword.Match(text).Groups[1].Value.Trim(),
                Start = DateTime.Now,
                Facts = cache.Facts.Match(text).Groups[1].Value.Trim(),
                Ric = cache.Ric.Match(text).Groups[1].Value.Trim(),
                Source = $"TetraControl2Connect {Constants.Version}",
                Reporter = hasValidReporter ? new ReporterModel()
                {
                    Name = reporterName,
                    PhoneNumber = reporterPhoneNumber,
                } : null,
                Address = new AddressModel()
                {
                    City = cache.City.Match(text).Groups[1].Value.Trim(),
                    Street = cache.Street.Match(text).Groups[1].Value.Trim(),
                    HouseNumber = cache.HouseNumber.Match(text).Groups[1].Value.Trim(),
                    District = cache.District.Match(text).Groups[1].Value.Trim(),
                    ZipCode = cache.ZipCode.Match(text).Groups[1].Value.Trim(),
                },
                Position = hasValidPosition ? new PositionModel()
                {
                    Latitude = latitudeValue,
                    Longitude = longitudeValue,
                } : null,
                Properties = additionalProperties,
            };

            if (string.IsNullOrWhiteSpace(operation.Keyword) || string.IsNullOrWhiteSpace(operation.Address.City))
            {
                throw new InvalidDataException("Keyword or City is empty. Fallback...");
            }

            return operation;
        }
    }
}
