using FeuerSoftware.TetraControl2Connect.Converters;
using System.Text.Json.Serialization;

namespace FeuerSoftware.TetraControl2Connect.Models.TetraControl
{
    public class TetraControlDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("statusCode")]
        public string StatusCode { get; set; } = string.Empty;

        [JsonPropertyName("statusText")]
        public string StatusText { get; set; } = string.Empty;

        /// <summary>
        /// In some situations also GSSI
        /// </summary>
        [JsonPropertyName("destSSI")]
        public string DestinationSSI { get; set; } = string.Empty;

        [JsonPropertyName("destName")]
        public string DestinationName { get; set; } = string.Empty;

        [JsonPropertyName("srcSSI")]
        public string SourceSSI { get; set; } = string.Empty;

        [JsonPropertyName("srcName")]
        public string SourceName { get; set; } = string.Empty;

        [JsonPropertyName("radioID")]
        public int RadioId { get; set; }

        [JsonPropertyName("radioName")]
        public string RadioName { get; set; } = string.Empty;

        [JsonPropertyName("remark")]
        public string Remark { get; set; } = string.Empty;

        [JsonPropertyName("Alt")]
        public int Alt { get; set; }

        [JsonPropertyName("FixQual")]
        public int FixQual { get; set; }

        [JsonPropertyName("Lat")]
        public double Latitude { get; set; }

        [JsonPropertyName("Lon")]
        public double Longitude { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("ts")]
        [JsonConverter(typeof(UnixEpochDateTimeConverter))]
        public DateTime TimestampUTC { get; set; } = DateTime.Now;
    }
}
