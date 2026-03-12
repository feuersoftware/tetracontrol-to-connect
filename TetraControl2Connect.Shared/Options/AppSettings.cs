namespace FeuerSoftware.TetraControl2Connect.Shared.Options
{
    public record AppSettings
    {
        public ConnectOptions ConnectOptions { get; set; } = new();

        public PatternOptions PatternOptions { get; set; } = new();

        public ProgramOptions ProgramOptions { get; set; } = new();

        public StatusOptions StatusOptions { get; set; } = new();

        public TetraControlOptions TetraControlOptions { get; set; } = new();

        public SeverityOptions SeverityOptions { get; set; } = new();
    }
}
