namespace FeuerSoftware.TetraControl2Connect.Shared.Options
{
    public class SirenCalloutOptions
    {
        public const string SectionName = nameof(SirenCalloutOptions);

        /// <summary>
        /// Overrides Severity-Level-Translation!
        /// </summary>
        public bool UseSirenCodeTranslationAsKeyword { get; set; } = false;

        public Dictionary<string, string> SirenCodeTranslations { get; set; } = new()
        {
            { "$2000", "Warnung der Bevölkerung" },
            { "$2001", "Entwarnung" },
            { "$2002", "Feueralarm" },
            { "$2003", "Probealarm" },
        };
    }
}
