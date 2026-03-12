using FeuerSoftware.TetraControl2Connect.Shared.Options.Models;

namespace FeuerSoftware.TetraControl2Connect.Shared.Options
{
    public record ConnectOptions
    {
        public const string SectionName = nameof(ConnectOptions);

        public virtual List<Site> Sites { get; set; } = [];
    }
}
