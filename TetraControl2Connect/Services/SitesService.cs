using FeuerSoftware.TetraControl2Connect.Models.Connect;
using FeuerSoftware.TetraControl2Connect.Shared.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace FeuerSoftware.TetraControl2Connect.Services
{
    public class SitesService(
        ILogger<SitesService> log,
        IConnectApiService connectApiService,
        IOptionsMonitor<ConnectOptions> connectOptions) : ISitesService
    {
        private readonly ILogger<SitesService> _log = log ?? throw new ArgumentNullException(nameof(log));
        private readonly IConnectApiService _connectApiService = connectApiService ?? throw new ArgumentNullException(nameof(connectApiService));
        private readonly IOptionsMonitor<ConnectOptions> _connectOptions = connectOptions ?? throw new ArgumentNullException(nameof(connectOptions));
        private readonly ConcurrentDictionary<string, SiteModel> _accessTokenSites = new();
        private bool _initialized = false;

        public SiteModel GetSiteInfo(string accessToken)
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("Service not initialized.");
            }

            return _accessTokenSites[accessToken];
        }

        public async Task Initialize()
        {
            _log.LogDebug($"Initializing {nameof(SitesService)}.");

            foreach (var siteFromConfiguration in _connectOptions.CurrentValue.Sites)
            {
                try
                {
                    var organizationInfo = await _connectApiService.GetOrganizationInfo(siteFromConfiguration.Key);

                    if (organizationInfo is null)
                    {
                        _log.LogCritical("Could not get information about site '{Name}'.", siteFromConfiguration.Name);

                        continue;
                    }

                    if (organizationInfo.Sites.Count > 1)
                    {
                        _log.LogCritical("Site '{Name}' is organization. Only site keys are allowed since version 2.8.0.", siteFromConfiguration.Name);

                        throw new InvalidDataException("Ab Version 2.8.0 keine Organisations-Schlüssel mehr verwenden, nur Standort-Schlüssel!");
                    }

                    var site = organizationInfo.Sites.Single();
                    site.OrganizationId = organizationInfo.Id;

                    var success = _accessTokenSites.TryAdd(siteFromConfiguration.Key, site);

                    if (!success)
                    {
                        _log.LogWarning("Duplicated key for site '{Name}'.", siteFromConfiguration.Name);
                    }

                    _log.LogDebug("Added site information {@siteInformation}.", site);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Error while getting information about site '{Name}'", siteFromConfiguration.Name);
                }
            }

            _initialized = true;

            _log.LogDebug($"{nameof(SitesService)} initializing completed.");
        }
    }
}
