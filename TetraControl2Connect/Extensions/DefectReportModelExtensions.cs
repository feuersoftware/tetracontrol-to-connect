using FeuerSoftware.TetraControl2Connect.Models.Connect;

namespace FeuerSoftware.TetraControl2Connect.Extensions
{
    public static class DefectReportModelExtensions
    {
        public static bool IsClosed(this DefectReportModel defectReport)
        {
            return defectReport.Status == DefectReportStatus.Resolved || defectReport.Status == DefectReportStatus.Rejected || defectReport.Status == DefectReportStatus.Paused;
        }
    }
}
