using FeuerSoftware.TetraControl2Connect.Extensions;
using FeuerSoftware.TetraControl2Connect.Models.Connect;
using FeuerSoftware.TetraControl2Connect.Shared.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Reactive.Linq;

namespace FeuerSoftware.TetraControl2Connect.Services
{
    public class UserService(
        ILogger<UserService> log,
        IConnectApiService connectApiService,
        IOptions<ConnectOptions> connectOptions,
        ISitesService sitesService) : IUserService
    {
        private readonly ILogger<UserService> _log = log ?? throw new ArgumentNullException(nameof(log));
        private readonly IConnectApiService _connectApiService = connectApiService ?? throw new ArgumentNullException(nameof(connectApiService));
        private readonly ISitesService _siteService = sitesService ?? throw new ArgumentNullException(nameof(sitesService));
        private readonly ConnectOptions _connectOptions = connectOptions?.Value ?? throw new ArgumentNullException(nameof(connectOptions));
        private readonly HashSet<UserModel> _users = [];
        private readonly ConcurrentDictionary<string, List<string>> _userAccessTokens = new();
        private IDisposable? _refreshSubscription;
        private bool _isInitialized = false;

        public void Dispose()
        {
            _refreshSubscription?.Dispose();
        }

        public List<string> GetAccessTokensForUser(string pagerIssi)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Service not initialized.");
            }

            var sucess = _userAccessTokens.TryGetValue(pagerIssi, out var accessTokens);

            if (!sucess || accessTokens is null)
            {
                _log.LogWarning($"No user with pager ISSI '{pagerIssi}' found.");
                return [];
            }

            return accessTokens;
        }

        public IEnumerable<UserModel> GetUsers(string pagerIssi)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Service not initialized.");
            }

            var users = _users.Where(u => u.PagerIssi == pagerIssi);

            return users;
        }

        public async Task Initialize()
        {
            _log.LogDebug($"Initializing {nameof(UserService)}.");

            _refreshSubscription = Observable
                .Interval(TimeSpan.FromHours(12))
                .SubscribeAsyncSafe(async _ =>
                {
                    _log.LogInformation("Refreshing users after 12 hours...");

                    await LoadOrRefreshUsers();
                },
                e => _log.LogError(e, "Failed to refresh users."),
                () => _log.LogDebug("User refresh subscription completed."));

            await LoadOrRefreshUsers();

            _isInitialized = true;
            _log.LogDebug($"{nameof(UserService)} initialization completed.");
        }

        private async Task LoadOrRefreshUsers()
        {
            _users.Clear();
            _userAccessTokens.Clear();

            foreach (var site in _connectOptions.Sites)
            {
                _log.LogInformation("Prepare users for Site '{SiteName}'.", site.Name);

                var users = await _connectApiService.GetUsers(site.Key);
                var siteInfo = _siteService.GetSiteInfo(site.Key);

                users = users.Where(u => !string.IsNullOrWhiteSpace(u.PagerIssi));


                if (siteInfo == null)
                {
                    _log.LogWarning("Cannot retrieve site info.");
                    continue;
                }

                foreach (var user in users)
                {
                    user.OrganizationId = siteInfo.OrganizationId;
                    _log.LogDebug("Found user {FirstName} {LastName} with Pager-ISSI {PagerIssi} in site {SiteName} in organization {OrganizationId}.", user.FirstName, user.LastName, user.PagerIssi, site.Name, siteInfo.OrganizationId);

                    if (_userAccessTokens.TryGetValue(user.PagerIssi, out var keys))
                    {
                        _log.LogDebug("User ist already present. Adding site {SiteName}' to list.", site.Name);
                        keys.Add(site.Key);
                    }
                    else
                    {
                        _userAccessTokens[user.PagerIssi] = [site.Key];
                    }
                }

                _users.UnionWith(users
                    .Where(u => !string.IsNullOrWhiteSpace(u.PagerIssi)));
            }
        }
    }
}
