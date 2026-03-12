using Destructurama.Attributed;

namespace FeuerSoftware.TetraControl2Connect.Shared.Options
{
    public class TetraControlOptions
    {
        public const string SectionName = nameof(TetraControlOptions);

        public string TetraControlHost { get; set; } = "localhost";

        public int TetraControlPort { get; set; } = 8085;

        public string TetraControlUsername { get; set; } = "Connect";

        [LogMasked]
        public string TetraControlPassword { get; set; } = "Connect";

        public Uri WebSocketUri => new($"ws://{TetraControlHost}:{TetraControlPort}/live.json?MaxAlter=0");
    }
}
