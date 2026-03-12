using FeuerSoftware.TetraControl2Connect.Shared.Options;

namespace FeuerSoftware.TetraControl2Connect.Shared
{
    public static class ProgramOptionsExtensions
    {
        public static bool IsHeartbeatConfigured(this ProgramOptions options)
        {
            return !string.IsNullOrWhiteSpace(options.HeartbeatEndpointUrl) &&
                options.HeartbeatInterval is not null &&
                options.HeartbeatInterval > TimeSpan.Zero;
        }
    }
}
