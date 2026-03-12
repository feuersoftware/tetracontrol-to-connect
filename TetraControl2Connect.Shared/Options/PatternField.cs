namespace FeuerSoftware.TetraControl2Connect.Shared.Options
{
    public record PatternField
    {
        public required string Name { get; set; } = string.Empty;

        public required string Pattern { get; set; } = string.Empty;
    }
}