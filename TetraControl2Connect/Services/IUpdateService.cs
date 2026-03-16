namespace FeuerSoftware.TetraControl2Connect.Services
{
    public record UpdateInfo(string LatestVersion, string ReleaseUrl);

    public interface IUpdateService
    {
        Task<UpdateInfo?> CheckForUpdateAsync(CancellationToken cancellationToken = default);

        UpdateInfo? LatestUpdate { get; }
    }
}
