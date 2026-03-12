namespace FeuerSoftware.TetraControl2Connect.Shared.Options
{
    public class StatusOptions
    {
        public const string SectionName = nameof(StatusOptions);

        public string AvailableStatus { get; set; } = "15";

        public string LimitedAvailableStatus { get; set; } = "-1";

        public string NotAvailableStatus { get; set; } = "0";

        public string ComingStatus { get; set; } = "57345";

        public string NotComingStatus { get; set; } = "57344";

        public string ComingLaterStatus { get; set; } = "-1";
    }
}
