using System.Text.Json.Serialization;

namespace FeuerSoftware.TetraControl2Connect.Shared.Options
{
    public record PatternOptions
    {
        public const string SectionName = nameof(PatternOptions);

        [JsonIgnore]
        public bool IsEnabled => !string.IsNullOrWhiteSpace(KeywordPattern) && !string.IsNullOrWhiteSpace(CityPattern);

        public string NumberPattern { get; set; } = string.Empty;

        public string KeywordPattern { get; set; } = string.Empty;

        public string FactsPattern { get; set; } = string.Empty;

        public string ReporterNamePattern { get; set; } = string.Empty;

        public string ReporterPhoneNumberPattern { get; set; } = string.Empty;

        public string CityPattern { get; set; } = string.Empty;

        public string StreetPattern { get; set; } = string.Empty;

        public string HouseNumberPattern { get; set; } = string.Empty;

        public string ZipCodePattern { get; set; } = string.Empty;

        public string DistrictPattern { get; set; } = string.Empty;

        public string LatitudePattern { get; set; } = string.Empty;

        public string LongitudePattern { get; set; } = string.Empty;

        public string RicPattern { get; set; } = string.Empty;

        public List<PatternField> AdditionalProperties { get; set; } = [];
    }
}
