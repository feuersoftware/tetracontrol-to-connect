namespace FeuerSoftware.TetraControl2Connect
{
    public static class Constants
    {
        public static string Version => typeof(Agent).Assembly.GetName().Version?.ToString() ?? "DEBUG";

        public const string DefaultKeyword = "ALARM";

        public const string ConnectBaseUrl = "https://connectapi.feuersoftware.com";
    }
}
