namespace FeuerSoftware.TetraControl2Connect.Models.Connect
{
    public record UserAvailabilityModel
    {
        public AvailabilityStatus Status { get; set; }

        public AvailabilitySource Source { get; set; } = AvailabilitySource.Pager;

        public DateTime Until { get; set; }
    }
}
