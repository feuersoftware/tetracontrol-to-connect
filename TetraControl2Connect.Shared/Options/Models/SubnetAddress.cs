namespace FeuerSoftware.TetraControl2Connect.Shared.Options.Models
{
    public record SubnetAddress
    {
        public required string Name { get; set; } = string.Empty;

        public required string SNA { get; set; } = string.Empty;

        public bool AlarmDirectly { get; set; } = false;

        public required string GSSI { get; set; } = string.Empty;
    }
}
