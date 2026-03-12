namespace FeuerSoftware.TetraControl2Connect.Models.Connect
{
    public class DefectReportModel
    {
        public int Id { get; set; }

        public int SiteId { get; set; }

        public DefectReportStatus Status { get; set; }

        public string ShortDescription { get; set; } = string.Empty;

        public string DetailedDescription { get; set; } = string.Empty;

        public Priority? Priority { get; set; }

        public string SequenceNumber { get; set; } = string.Empty;

        public int? CategoryId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
