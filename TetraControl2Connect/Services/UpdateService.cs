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
                    .GetAsync("repos/feuersoftware/tetracontrol-to-connect/releases?per_page=100", cancellationToken)
                    .ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    _log.LogDebug("Update check failed. Status code: {StatusCode}", response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                var releases = JsonSerializer.Deserialize<List<GitHubReleaseModel>>(content);

                if (releases is null || releases.Count == 0)
                    return null;

                var currentVersion = typeof(Agent).Assembly.GetName().Version;
                if (currentVersion is null)
                    return null;

                var currentNormalized = new Version(currentVersion.Major, currentVersion.Minor, Math.Max(currentVersion.Build, 0));

                // Find the highest production (non-prerelease, non-draft) release that is newer than the current version.
                GitHubReleaseModel? bestRelease = null;
                Version? bestVersion = null;

                foreach (var release in releases)
                {
                    if (release.Prerelease || release.Draft || string.IsNullOrEmpty(release.TagName))
                        continue;

                    var tagVersion = release.TagName.TrimStart('v');
                    if (!Version.TryParse(tagVersion, out var parsedVersion))
                        continue;

                    var normalized = new Version(parsedVersion.Major, parsedVersion.Minor, Math.Max(parsedVersion.Build, 0));

                    if (normalized <= currentNormalized)
                        continue;

                    if (bestVersion is null || normalized > bestVersion)
                    {
                        bestVersion = normalized;
                        bestRelease = release;
                    }
                }

                if (bestRelease is null)
                    return null;

                LatestUpdate = new UpdateInfo(bestRelease.TagName, bestRelease.HtmlUrl);
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
