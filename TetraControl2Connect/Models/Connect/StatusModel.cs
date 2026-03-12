namespace FeuerSoftware.TetraControl2Connect.Models.Connect
{
    public record StatusModel
    {
        public byte Status { get; set; }

        public DateTimeOffset StatusTimestamp { get; set; }

        public string Source { get; set; } = "TetraControl";
    }
}
