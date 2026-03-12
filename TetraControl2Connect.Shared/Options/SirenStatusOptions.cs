namespace FeuerSoftware.TetraControl2Connect.Shared.Options
{
    public class SirenStatusOptions
    {
        public const string SectionName = nameof(SirenStatusOptions);

        public Dictionary<string, string> FailureTranslations { get; set; } = new();
    }
}
