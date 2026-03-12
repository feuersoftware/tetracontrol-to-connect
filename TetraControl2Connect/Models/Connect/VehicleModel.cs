namespace FeuerSoftware.TetraControl2Connect.Models.Connect
{
    public sealed record VehicleModel
    {
        public int Id { get; set; }

        public string RadioId { get; set; } = string.Empty;

        public string PlaceName { get; set; } = string.Empty;

        public string OrganizationCallSign { get; set; } = string.Empty;

        public int? LocationIdentificationNumber { get; set; }

        public string VehicleIdentifier { get; set; } = string.Empty;

        public int? Subdivision { get; set; }

        public string Description { get; set; } = string.Empty;

        public int Crew { get; set; }

        public string Phone { get; set; } = string.Empty;

        public string CallSign { get; set; } = string.Empty;

        public bool Equals(VehicleModel? other)
        {
            return ReferenceEquals(this, other) || string.Equals(RadioId, other?.RadioId);
        }

        public override int GetHashCode()
        {
            return RadioId.GetHashCode();
        }
    }
}
