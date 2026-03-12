namespace FeuerSoftware.TetraControl2Connect.Models.Connect
{
    public class DefectReportCategoryModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int? OrganizationId { get; set; }

        public int? SiteId { get; set; }

        public bool IsBuiltin { get; set; }
    }
}
