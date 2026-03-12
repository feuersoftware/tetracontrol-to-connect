using Destructurama.Attributed;

namespace FeuerSoftware.TetraControl2Connect.Shared.Options.Models
{
    public record Site
    {
        public required string Name { get; set; } = string.Empty;

        [LogMasked]
        public required string Key { get; set; } = string.Empty;

        public List<SubnetAddress> SubnetAddresses { get; set; } = [];

        public List<Siren> Sirens { get; set; } = [];
    }
}
