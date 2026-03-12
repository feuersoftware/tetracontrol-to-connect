using FeuerSoftware.TetraControl2Connect.Extensions;
using FeuerSoftware.TetraControl2Connect.Models.Connect;
using FeuerSoftware.TetraControl2Connect.Models.TetraControl;
using FeuerSoftware.TetraControl2Connect.Shared.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Reactive.Linq;

namespace FeuerSoftware.TetraControl2Connect.Services
{
    public class VehicleService(
        ILogger<VehicleService> log,
        IConnectApiService connectApiService,
        IOptions<ConnectOptions> connectOptions,
        IOptions<ProgramOptions> programOptions) : IVehicleService
    {
        public const double PositionTolerance = 0.00009d;

        private readonly ILogger<VehicleService> _log = log ?? throw new ArgumentNullException(nameof(log));
        private readonly IConnectApiService _connectApiService = connectApiService ?? throw new ArgumentNullException(nameof(connectApiService));
        private readonly ConnectOptions _connectOptions = connectOptions?.Value ?? throw new ArgumentNullException(nameof(connectOptions));
        private readonly ProgramOptions _programOptions = programOptions?.Value ?? throw new ArgumentNullException(nameof(programOptions));
        private readonly HashSet<VehicleModel> _vehicles = [];
        private readonly ConcurrentDictionary<string, List<string>> _vehicleAccessTokens = new();
        private readonly ConcurrentDictionary<string, int> _vehicleStatusCache = new();
        private readonly ConcurrentDictionary<string, (double lat, double lng)> _vehiclePositionsCache = new();
        private bool _isInitialized = false;
        private IDisposable? _refreshSubscription;

        public async Task Initialize()
        {
            _log.LogDebug($"Initializing {nameof(VehicleService)}.");

            _refreshSubscription = Observable
                .Interval(TimeSpan.FromHours(6))
                .SubscribeAsyncSafe(async _ =>
                {
                    _log.LogInformation("Refreshing vehicles after 6 hours...");

                    await LoadOrRefreshVehiclesAsync();
                },
                e => _log.LogError(e, "Failed to refresh vehicles."),
                () => _log.LogDebug("Vehicle refresh subscription completed."));

            await LoadOrRefreshVehiclesAsync();

            _isInitialized = true;

            _log.LogDebug($"{nameof(VehicleService)} initialization completed.");
        }

        public void Dispose()
        {
            _refreshSubscription?.Dispose();
        }

        public async Task HandleVehiclePosition(TetraControlDto dto)
        {
            var vehicle = GetVehicle(dto.SourceSSI);
            var latitude = Math.Round(dto.Latitude, 7);
            var longitude = Math.Round(dto.Longitude, 7);

            if (vehicle is null)
            {
                _log.LogDebug("Could not find vehicle with ISSI {SourceIssi} in our vehicle list. Ignoring...", dto.SourceSSI);
                return;
            }

            if (_vehiclePositionsCache.TryGetValue(dto.SourceSSI, out var cachedPosition))
            {
                var cachedLat = cachedPosition.lat;
                var cachedLng = cachedPosition.lng;

                var latDifference = Math.Abs(cachedLat - latitude);
                var longDifference = Math.Abs(cachedLng - longitude);

                if (latDifference <= PositionTolerance &&
                    longDifference <= PositionTolerance)
                {
                    _log.LogInformation("Position update is within tolerance. Ignoring...");

                    return;
                }

                _vehiclePositionsCache[dto.SourceSSI] = (latitude, longitude);
            }
            else
            {
                _ = _vehiclePositionsCache.TryAdd(dto.SourceSSI, (latitude, longitude));
            }

            var position = new StatusPositionModel()
            {
                Position = new PositionModel()
                {
                    Latitude = latitude,
                    Longitude = longitude,
                },
                PositionTimestamp = DateTime.Now,
            };

            var accessTokens = GetAccessTokensForVehicle(vehicle.RadioId);

            foreach (var token in accessTokens)
            {
                await _connectApiService.PostVehiclePosition(token, vehicle.RadioId, position);
                _log.LogInformation($"Sent position update for vehicle '{vehicle.Description}' ISSI '{vehicle.RadioId}' to Connect.");
            }
        }

        public async Task HandleVehicleStatus(TetraControlDto dto)
        {
            var vehicle = GetVehicle(dto.SourceSSI);

            if (vehicle is null)
            {
                _log.LogDebug("Could not find vehicle with ISSI {SourceSSI} in our vehicle list. Ignoring...", dto.SourceSSI);
                return;
            }

            var status = new StatusModel()
            {
                Status = Convert.ToByte(dto.Status),
                StatusTimestamp = DateTime.Now,
            };

            if ((status.Status == 5 && _programOptions.IgnoreStatus5) ||
                status.Status == 0 && _programOptions.IgnoreStatus0 ||
                status.Status == 9 && _programOptions.IgnoreStatus9)
            {
                _log.LogInformation("Ignoring status because of enabled suppression.");
                return;
            }

            var accessTokens = GetAccessTokensForVehicle(vehicle.RadioId);

            foreach (var token in accessTokens)
            {
                await _connectApiService.PostVehicleStatus(token, vehicle.RadioId, status);
                _log.LogInformation($"Sent status update status '{status.Status}' for vehicle '{vehicle.Description}' ISSI '{vehicle.RadioId}' to Connect.");
            }
        }

        public List<string> GetAccessTokensForVehicle(string issi)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Service not initialized.");
            }

            var sucess = _vehicleAccessTokens.TryGetValue(issi, out var accessTokens);

            if (!sucess || accessTokens is null)
            {
                _log.LogWarning($"No vehicle with ISSI '{issi}' found.");

                return [];
            }

            return accessTokens;
        }

        public VehicleModel? GetVehicle(string issi)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Service not initialized.");
            }

            var vehicle = _vehicles.SingleOrDefault(v => v.RadioId == issi);

            return vehicle;
        }

        private async Task LoadOrRefreshVehiclesAsync()
        {
            _vehicles.Clear();
            _vehicleAccessTokens.Clear();

            foreach (var site in _connectOptions.Sites)
            {
                var vehicles = await _connectApiService.GetVehicles(site.Key).ConfigureAwait(false);

                _log.LogInformation($"Prepare vehicles for Site '{site.Name}'.");

                _vehicles.UnionWith(vehicles
                    .Where(v => !string.IsNullOrWhiteSpace(v.RadioId))
                    .Select(v =>
                    {
                        _log.LogDebug($"Found vehicle '{v.Description}' with ISSI '{v.RadioId}'.");

                        if (_vehicleAccessTokens.TryGetValue(v.RadioId, out var existingKeys))
                        {
                            _log.LogDebug($"Vehicle is already present. Adding site '{site.Name}' to list.");
                            existingKeys.Add(site.Key);
                        }
                        else
                        {
                            _vehicleAccessTokens[v.RadioId] = [site.Key];
                        }

                        return v;
                    }));
            }
        }
    }
}
