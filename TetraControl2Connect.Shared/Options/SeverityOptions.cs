namespace FeuerSoftware.TetraControl2Connect.Shared.Options
{
    public record SeverityOptions
    {
        public const string SectionName = nameof(SeverityOptions);

        public Dictionary<string, string> SeverityTranslations { get; set; } = [];

        public bool UseServerityTranslationAsKeyword { get; set; } = true;
    }
}
