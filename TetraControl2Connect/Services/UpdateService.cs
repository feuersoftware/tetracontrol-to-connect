using FeuerSoftware.TetraControl2Connect.Models;
using System.Text.Json;

namespace FeuerSoftware.TetraControl2Connect.Services
{
    public class UpdateService(
        ILogger<UpdateService> log,
        IHttpClientFactory httpClientFactory) : IUpdateService
    {
        private readonly ILogger<UpdateService> _log = log ?? throw new ArgumentNullException(nameof(log));
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

        public UpdateInfo? LatestUpdate { get; private set; }

        public async Task<UpdateInfo?> CheckForUpdateAsync(CancellationToken cancellationToken = default)
        {
#if DEBUG
            return null;
#else
            try
            {
                using var httpClient = _httpClientFactory.CreateClient(nameof(IUpdateService));
                using var response = await httpClient
                    .GetAsync("repos/feuersoftware/tetracontrol-to-connect/releases/latest", cancellationToken)
                    .ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    _log.LogDebug("Update check failed. Status code: {StatusCode}", response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                var release = JsonSerializer.Deserialize<GitHubReleaseModel>(content);

                if (release is null || string.IsNullOrEmpty(release.TagName) || release.Prerelease || release.Draft)
                    return null;

                var tagVersion = release.TagName.TrimStart('v');
                if (!Version.TryParse(tagVersion, out var latestVersion))
                    return null;

                var currentVersion = typeof(Agent).Assembly.GetName().Version;
                if (currentVersion is null)
                    return null;

                // Normalise to Major.Minor.Build for comparison (strip assembly revision)
                var latestNormalized = new Version(latestVersion.Major, latestVersion.Minor, Math.Max(latestVersion.Build, 0));
                var currentNormalized = new Version(currentVersion.Major, currentVersion.Minor, Math.Max(currentVersion.Build, 0));

                if (latestNormalized <= currentNormalized)
                    return null;

                LatestUpdate = new UpdateInfo(release.TagName, release.HtmlUrl);
                return LatestUpdate;
            }
            catch (Exception ex)
            {
                _log.LogDebug(ex, "Update check failed.");
                return null;
            }
#endif
        }
    }
}
