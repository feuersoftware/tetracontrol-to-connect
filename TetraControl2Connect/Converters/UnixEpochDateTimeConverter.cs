using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace FeuerSoftware.TetraControl2Connect.Converters
{
    sealed partial class UnixEpochDateTimeConverter : JsonConverter<DateTime>
    {
        private static readonly DateTime EpochStart = new(1970, 1, 1, 0, 0, 0);
        [GeneratedRegex("^/Date\\(([+-]*\\d+)\\)/$", RegexOptions.CultureInvariant)]
        private static partial Regex UnixEpochRegex();

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var formatted = reader.GetString()!;
            var match = UnixEpochRegex().Match(formatted);

            if (!match.Success ||
                !long.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long unixTime))
            {
                throw new JsonException("Given Unix epoch value does not match our pattern.");
            }

            return EpochStart.AddMilliseconds(unixTime);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            var unixTime = Convert.ToInt64((value - EpochStart).TotalMilliseconds);

            var formatted = FormattableString.Invariant($"/Date({unixTime})/");
            writer.WriteStringValue(formatted);
        }
    }
}
