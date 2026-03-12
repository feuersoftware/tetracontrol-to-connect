using FeuerSoftware.TetraControl2Connect.Models.TetraControl;
using System.Text.RegularExpressions;

namespace FeuerSoftware.TetraControl2Connect.Extensions
{
    public static partial class TetraControlDtoExtensions
    {
        [GeneratedRegex("(&[\\d]{2})+", RegexOptions.IgnoreCase | RegexOptions.Compiled, "de-DE")]
        public static partial Regex SnaRegex();
        [GeneratedRegex("(&[\\d]{2})+\\$\\d{4}", RegexOptions.IgnoreCase | RegexOptions.Compiled, "de-DE")]
        private static partial Regex SirenAlarmRegex();

        [GeneratedRegex("\\$2\\d{3}", RegexOptions.IgnoreCase | RegexOptions.Compiled, "de-DE")]
        private static partial Regex SirenAlarmCodeRegex();

        [GeneratedRegex("E[\\dA-F]{3}", RegexOptions.IgnoreCase | RegexOptions.Compiled, "de-DE")]
        private static partial Regex SirenStatusCodeRegex();

        [GeneratedRegex("^\\d{1}$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "de-DE")]
        private static partial Regex VehicleStatusCodeRegex();

        public static string GetFallbackOperationNumberForConnect(this TetraControlDto sds)
        {
            return $"T2C-FB-{DateTime.Now:yyyy-MM-dd}-{sds.ExtractCalloutReference() ?? -1}";
        }

        public static SdsType GetSdsType(this TetraControlDto sds)
        {
            var typeFromRemark = sds.Remark.Split(';', StringSplitOptions.TrimEntries).FirstOrDefault();

            if (typeFromRemark is null)
            {
                return SdsType.Unknown;
            }

            return typeFromRemark switch
            {
                "-1" => SdsType.Callout,
                "3" => SdsType.CalloutFeedback,
                "5" => SdsType.TacticalAvailability,
                _ => SdsType.Unknown
            };
        }

        public static StatusType GetStatusType(this TetraControlDto sds)
        {
            if (SirenStatusCodeRegex().IsMatch(sds.StatusCode))
            {
                return StatusType.Siren;
            }
            else if (VehicleStatusCodeRegex().IsMatch(sds.Status))
            {
                return StatusType.Vehicle;
            }
            else
            {
                return StatusType.Unknown;
            }
        }

        public static int? ExtractStatuscode(this TetraControlDto sds)
        {
            var splitted = sds.Remark.SplitToIntArray(';');

            if (splitted.Length != 4)
            {
                return null;
            }

            return splitted[3];
        }

        public static bool IsCalloutForSirens(this TetraControlDto sds)
        {
            return SirenAlarmRegex().IsMatch(sds.Text);
        }

        public static List<string> ExtractSNAs(this TetraControlDto sds)
        {
            var match = SnaRegex().Match(sds.Text);

            if (!match.Success)
            {
                return sds.ExtractSNAsFromRemark();
            }

            return match.Groups[1].Captures.Select(c => c.Value.Trim()).ToList();
        }

        public static string ExtractCalloutSeverity(this TetraControlDto sds)
        {
            try
            {
                // Not with RemoveEmptyEntries because of GSSI-Alarm without SNAs
                var splitted = sds.Remark.Split(';', StringSplitOptions.TrimEntries);

                if (splitted.Length != 4)
                {
                    return string.Empty;
                }

                return splitted[1];
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Case with two connected devices to TetraControl (Pager + MRT/HRT) and sending Callout to Pager from MRT/HRT.
        /// </summary>
        /// <param name="sds">SDS</param>
        /// <returns>True if Callout is self sent from MRT/HRT</returns>
        public static bool IsCalloutSelfSent(this TetraControlDto sds)
        {
            return sds.ExtractSNAsFromRemark().Count == 0 && sds.Text.StartsWith("Gesendet:", StringComparison.Ordinal);
        }

        public static int? ExtractCalloutReference(this TetraControlDto sds)
        {
            try
            {
                var splitted = sds.Remark.Split(';', StringSplitOptions.TrimEntries);

                if (splitted.Length != 4)
                {
                    return null;
                }

                var cref = splitted[2];

                return int.TryParse(cref, out var parsedCref) ? parsedCref : null;
            }
            catch
            {
                return null;
            }
        }

        public static string ExtractSirenCode(this TetraControlDto sds)
        {
            var match = SirenAlarmCodeRegex().Match(sds.Text);

            if (!match.Success)
            {
                return string.Empty;
            }

            return match.Captures.First().Value;
        }

        private static List<string> ExtractSNAsFromRemark(this TetraControlDto sds)
        {
            var splitted = sds.Remark.Split(';', StringSplitOptions.None);

            // For older TetraControl Versions the SNAs are not written to the Remark Field
            // Mirko Jechimer Memorial
            if (splitted.Length != 4)
            {
                return [];
            }

            var snasFromRemark = splitted[3];

#pragma warning disable IDE0305 // Simplify collection initialization
            return snasFromRemark.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
#pragma warning restore IDE0305 // Simplify collection initialization
        }
    }
}
