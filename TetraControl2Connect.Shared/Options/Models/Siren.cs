namespace FeuerSoftware.TetraControl2Connect.Shared.Options.Models
{
    public record Siren
    {
        public required string Name { get; set; } = string.Empty;

        public required string Issi { get; set; } = string.Empty;

        public TimeSpan? ExpectedHeartbeatInterval { get; set; }
    }
}
