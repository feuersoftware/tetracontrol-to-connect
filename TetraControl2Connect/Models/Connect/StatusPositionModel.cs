namespace FeuerSoftware.TetraControl2Connect.Models.Connect
{
    public record StatusPositionModel
    {
        public PositionModel Position { get; set; } = new();

        public DateTimeOffset PositionTimestamp { get; set; }

        public string Source { get; set; } = "TetraControl";
    }
}
