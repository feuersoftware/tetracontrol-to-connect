using FeuerSoftware.TetraControl2Connect.Models.Connect;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FeuerSoftware.TetraControl2Connect.Services
{
    public class ConnectApiService(
        ILogger<ConnectApiService> log,
        IHttpClientFactory httpClientFactory) : IConnectApiService
    {
        private readonly ILogger<ConnectApiService> _log = log ?? throw new ArgumentNullException(nameof(log));
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

        public async Task PostDefectReport(DefectReportModel model, string accessToken)
        {
            await Post<DefectReportModel>("/interfaces/public/defectReport", model, accessToken, default);
        }

        public async Task PutDefectReport(int id, DefectReportModel model, string accessToken)
        {
            await Put<DefectReportModel>($"/interfaces/public/defectReport/{id}", model, accessToken, default);
        }

        public async Task<OrganizationModel?> GetOrganizationInfo(string accessToken)
        {
            return await Get<OrganizationModel?>("/interfaces/public/organization", accessToken, default);
        }

        public async Task<List<DefectReportModel>> GetDefectReports(string accessToken)
        {
            return await Get<List<DefectReportModel>>("/interfaces/public/defectReport", accessToken, default) ?? [];
        }

        public async Task<List<DefectReportCategoryModel>> GetDefectReportCategories(string accessToken)
        {
            return await Get<List<DefectReportCategoryModel>>("/interfaces/public/defectReportCategory", accessToken, default) ?? [];
        }

        public async Task PostDefectReportCategory(DefectReportCategoryModel model, string accessToken)
        {
            await Post<DefectReportCategoryModel>("/interfaces/public/defectReportCategory", model, accessToken, default);
        }

        public async Task<IEnumerable<OperationModel>> GetLatestOperations(string accessToken)
        {
            var operations = await Get<List<OperationModel>>("/interfaces/public/operation", accessToken, default).ConfigureAwait(false);

            if (operations is null)
            {
                return [];
            }

            return operations
                .OrderByDescending(o => o.CreatedAt)
                .ThenBy(o => o.LastUpdateAt);
        }

        public async Task PostOperation(string accessToken, OperationModel operationModel, UpdateStrategy updateStrategy = UpdateStrategy.ByNumber)
        {
            await Post<OperationModel>($"/interfaces/public/operation?updateStrategy={updateStrategy}", operationModel, accessToken, default).ConfigureAwait(false);
        }

        public async Task PostUserStatus(string accessToken, UserStatusModel statusModel)
        {
            await Post<UserStatusModel>($"/interfaces/public/operation/userstatus", statusModel, accessToken, default).ConfigureAwait(false);
        }

        public async Task PostVehicleStatus(string accessToken, string issi, StatusModel statusModel)
        {
            await Post<StatusModel>($"/interfaces/public/vehicle/{issi}/status", statusModel, accessToken, default).ConfigureAwait(false);
        }

        public async Task PostVehiclePosition(string accessToken, string issi, StatusPositionModel positionModel)
        {
            await Post<StatusPositionModel>($"/interfaces/public/vehicle/{issi}/status", positionModel, accessToken, default).ConfigureAwait(false);
        }

        public async Task PutUserAvailability(string accessToken, string userIssi, UserAvailabilityModel availabilityModel)
        {
            await Put<UserAvailabilityModel>($"/interfaces/public/user/{userIssi}/availability/current", availabilityModel, accessToken, default).ConfigureAwait(false);
        }

        public async Task<IEnumerable<UserModel>> GetUsers(string accessToken)
        {
            return await Get<IEnumerable<UserModel>>("/interfaces/public/user", accessToken, default).ConfigureAwait(false) ?? [];
        }

        public async Task<IEnumerable<VehicleModel>> GetVehicles(string accessToken)
        {
            return await Get<IEnumerable<VehicleModel>>("/interfaces/public/vehicle", accessToken, default).ConfigureAwait(false) ?? [];
        }

        private Task<T?> Get<T>(string url, string accessToken, CancellationToken cancellationToken)
            => CallService<T>(client => client.GetAsync(url, cancellationToken), accessToken, cancellationToken);

        private Task<T?> Post<T>(string url, T data, string accessToken, CancellationToken cancellationToken)
            => CallService<T>(client => client.PostAsync(url, new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json"), cancellationToken), accessToken, cancellationToken);

        private Task<T?> Put<T>(string url, T data, string accessToken, CancellationToken cancellationToken)
            => CallService<T>(client => client.PutAsync(url, new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json"), cancellationToken), accessToken, cancellationToken);

        private async Task<T?> CallService<T>(Func<HttpClient, Task<HttpResponseMessage>> action, string accessToken, CancellationToken cancellationToken)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient(nameof(IConnectApiService));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                using var response = await action(httpClient);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                    if (!string.IsNullOrEmpty(responseContent))
                    {
                        return JsonSerializer.Deserialize<T>(responseContent) ?? default;
                    }
                }
                else
                {
                    _log.LogError($"Unsuccessful HTTP-Request to Connect. Statuscode '{response.StatusCode}' from '{response.RequestMessage?.Method}' '{response.RequestMessage?.RequestUri}'.");
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error happened on CallService.");
            }

            return default;
        }
    }
}
