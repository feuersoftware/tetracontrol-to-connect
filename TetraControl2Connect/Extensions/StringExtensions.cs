namespace FeuerSoftware.TetraControl2Connect.Extensions
{
    public static partial class StringExtensions
    {
        public static int[] SplitToIntArray(this string value, char separator)
        {
            var splitted = value.Split(separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            return splitted.Select(x => Convert.ToInt32(x)).ToArray();
        }

        public static string RemoveSubnetAddresses(this string value)
        {
            if (value is null)
            {
                return string.Empty;
            }

            return TetraControlDtoExtensions.SnaRegex().Replace(value, "");
        }
    }
}
