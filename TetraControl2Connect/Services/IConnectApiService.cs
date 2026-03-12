using FeuerSoftware.TetraControl2Connect.Models.Connect;

namespace FeuerSoftware.TetraControl2Connect.Services
{
    public interface IConnectApiService
    {
        Task<IEnumerable<UserModel>> GetUsers(string accessToken);

        Task<IEnumerable<VehicleModel>> GetVehicles(string accessToken);

        Task<OrganizationModel?> GetOrganizationInfo(string accessToken);

        Task PutUserAvailability(string accessToken, string userIssi, UserAvailabilityModel availabilityModel);

        Task PostVehicleStatus(string accessToken, string issi, StatusModel statusModel);

        Task PostVehiclePosition(string accessToken, string issi, StatusPositionModel position);

        Task PostUserStatus(string accessToken, UserStatusModel statusModel);

        Task<IEnumerable<OperationModel>> GetLatestOperations(string accessToken);

        Task PostOperation(string accessToken, OperationModel operationModel, UpdateStrategy updateStrategy = UpdateStrategy.ByNumber);
        Task PostDefectReport(DefectReportModel model, string accessToken);
        Task<List<DefectReportModel>> GetDefectReports(string accessToken);
        Task<List<DefectReportCategoryModel>> GetDefectReportCategories(string accessToken);
        Task PostDefectReportCategory(DefectReportCategoryModel model, string accessToken);
        Task PutDefectReport(int id, DefectReportModel model, string accessToken);
    }
}