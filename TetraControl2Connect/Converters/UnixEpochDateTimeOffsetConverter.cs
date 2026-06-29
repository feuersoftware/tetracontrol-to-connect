using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace FeuerSoftware.TetraControl2Connect.Converters
{
    /// <summary>
    /// Converts the Microsoft JSON date format (<c>/Date(unixMilliseconds)/</c>) used by TetraControl
    /// to and from <see cref="DateTimeOffset"/>. Unix epoch milliseconds are inherently UTC, so the
    /// resulting value always carries a zero offset and serializes with an explicit designator.
    /// </summary>
    sealed partial class UnixEpochDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {
        [GeneratedRegex("^/Date\\(([+-]*\\d+)\\)/$", RegexOptions.CultureInvariant)]
        private static partial Regex UnixEpochRegex();

        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var formatted = reader.GetString()!;
            var match = UnixEpochRegex().Match(formatted);

            if (!match.Success ||
                !long.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long unixTime))
            {
                throw new JsonException("Given Unix epoch value does not match our pattern.");
            }

            return DateTimeOffset.FromUnixTimeMilliseconds(unixTime);
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            var unixTime = value.ToUnixTimeMilliseconds();

            var formatted = FormattableString.Invariant($"/Date({unixTime})/");
            writer.WriteStringValue(formatted);
        }
    }
}
