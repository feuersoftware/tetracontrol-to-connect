namespace FeuerSoftware.TetraControl2Connect.Models.Connect
{
    public class OrganizationModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public List<SiteModel> Sites { get; set; } = [];
    }
}
