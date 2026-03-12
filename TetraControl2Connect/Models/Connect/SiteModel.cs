namespace FeuerSoftware.TetraControl2Connect.Models.Connect
{
    public class SiteModel
    {
        public int Id { get; set; }

        // Not offered by API
        public int OrganizationId { get; set; }

        public string Name { get; set; } = string.Empty;

        public AddressModel Address { get; set; } = new();
    }
}