namespace FeuerSoftware.TetraControl2Connect.Models.Connect
{
    public record AddressModel
    {
        public string? Street { get; set; }

        public string? HouseNumber { get; set; }

        public string? ZipCode { get; set; }

        public string? City { get; set; }

        public string? District { get; set; }

        /// <summary>
        /// Not in use for creating operations -> Use Posisition there!
        /// </summary>
        public double? Lng { get; set; }

        /// <summary>
        /// Not in use for creating operations -> Use Posisition there!
        /// </summary>
        public double? Lat { get; set; }
    }
}