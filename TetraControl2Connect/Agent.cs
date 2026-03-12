using FeuerSoftware.TetraControl2Connect.Extensions;
using FeuerSoftware.TetraControl2Connect.Hubs;
using FeuerSoftware.TetraControl2Connect.Models.TetraControl;
using FeuerSoftware.TetraControl2Connect.Services;
using FeuerSoftware.TetraControl2Connect.Shared;
using FeuerSoftware.TetraControl2Connect.Shared.Options;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reactive.Linq;

namespace FeuerSoftware.TetraControl2Connect
{
    public sealed class Agent(
        ILogger<Agent> log,
        ITetraControlClient tcClient,
        IOptions<ConnectOptions> connectOptions,
        IOptions<ProgramOptions> programOptions,
        IOptions<StatusOptions> statusOptions,
        IOptions<SirenStatusOptions> sirenStatusOptions,
        IUserService userService,
        IVehicleService vehicleService,
        ISDSService sdsService,
        IOptions<SeverityOptions> severityOptions,
        IOptions<SirenCalloutOptions> sirenCalloutOptions,
        IHttpClientFactory httpClientFactory,
        ISirenService sirenService,
        ISitesService sitesService,
        IHubContext<MessageHub> messageHub) : IHostedService, IDisposable
    {
        private readonly ILogger<Agent> _log = log ?? throw new ArgumentNullException(nameof(log));
        private readonly ITetraControlClient _tcClient = tcClient ?? throw new ArgumentNullException(nameof(tcClient));
        private readonly SirenStatusOptions _sirenStatusOptions = sirenStatusOptions?.Value ?? throw new ArgumentNullException(nameof(sirenStatusOptions));
        private readonly ConnectOptions _connectOptions = connectOptions?.Value ?? throw new ArgumentNullException(nameof(connectOptions));
        private readonly ProgramOptions _programOptions = programOptions?.Value ?? throw new ArgumentNullException(nameof(programOptions));
        private readonly StatusOptions _statusOptions = statusOptions?.Value ?? throw new ArgumentNullException(nameof(statusOptions));
        private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        private readonly IVehicleService _vehicleService = vehicleService ?? throw new ArgumentNullException(nameof(vehicleService));
        private readonly ISDSService _sdsService = sdsService ?? throw new ArgumentNullException(nameof(sdsService));
        private readonly SeverityOptions _severityOptions = severityOptions?.Value ?? throw new ArgumentNullException(nameof(severityOptions));
        private readonly SirenCalloutOptions _sirenCalloutOptions = sirenCalloutOptions?.Value ?? throw new ArgumentNullException(nameof(sirenCalloutOptions));
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        private readonly ISirenService _sirenService = sirenService ?? throw new ArgumentNullException(nameof(sirenService));
        private readonly ISitesService _sitesService = sitesService ?? throw new ArgumentNullException(nameof(sitesService));
        private readonly IHubContext<MessageHub> _messageHub = messageHub ?? throw new ArgumentNullException(nameof(messageHub));
        private IDisposable? _statusSubscription;
        private IDisposable? _positionSubscription;
        private IDisposable? _sDSSubscription;
        private IDisposable? _heartbeatSubscription;
        private IDisposable? _connectionSubscription;

        public void Dispose()
        {
            _sDSSubscription?.Dispose();
            _statusSubscription?.Dispose();
            _positionSubscription?.Dispose();
            _vehicleService?.Dispose();
            _userService?.Dispose();
            _heartbeatSubscription?.Dispose();
            _connectionSubscription?.Dispose();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _log.LogInformation("Starting agent...");
            _log.LogInformation($"Version: {typeof(Program).Assembly.GetName().Version}");
            _log.LogInformation("Using following ConnectOptions from appsettings.json: {@Options}", _connectOptions);
            _log.LogInformation("Using following ProgramOptions from appsettings.json: {@Options}", _programOptions);
            _log.LogInformation("Using following StatusOptions from appsettings.json: {@Options}", _statusOptions);
            _log.LogInformation("Using following SeverityOptions from appsettings.json: {@Options}", _severityOptions);
            _log.LogInformation("Using following SirenCalloutOptions from appsettings.json: {@Options}", _sirenCalloutOptions);
            _log.LogInformation("Using following SirenStatusOptions from appsettings.json: {@Options}", _sirenStatusOptions);

            // Connect to TetraControl first — this is independent of site configuration
            try
            {
                InitializeStatus();
                InitializePosition();
                InitializeSDS();
                InitializeConnectionState();

                _tcClient.Init();
                await _tcClient.Start();
            }
            catch (Exception ex)
            {
                _log.LogCritical(ex, "Failed to start TetraControl connection.");
            }

            // Initialize Connect services — failures here should not prevent TetraControl connection
            try
            {
                await Prepare();
                await InitializeHeartbeat();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to initialize Connect services. Site-dependent features will be unavailable until configuration is corrected.");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _tcClient.Stop();
            _log.LogInformation("Agent stopped.");
        }

        private async Task Prepare()
        {
            await _sitesService.Initialize();
            await _sirenService.Initialize();

            if (_programOptions.SendVehiclePositions || _programOptions.SendVehicleStatus)
            {
                await _vehicleService.Initialize();
            }

            if (_programOptions.SendUserAvailability || _programOptions.SendUserOperationStatus || _programOptions.SendAlarms)
            {
                await _userService.Initialize();
            }
        }

        private async Task InitializeHeartbeat()
        {
            if (!_programOptions.IsHeartbeatConfigured())
            {
                _log.LogWarning("Heartbeat is not configured.");

                return;
            }

            async Task SendHeartbeat()
            {
                _log.LogDebug("Sending heartbeat...");
                using var httpClient = _httpClientFactory.CreateClient(nameof(Agent));

                await httpClient.GetAsync(string.Empty);
            }

            _heartbeatSubscription = Observable.Interval(_programOptions.HeartbeatInterval!.Value)
                .CombineLatest(_tcClient.IsConnected.DistinctUntilChanged())
                .Where(x => x.Second)
                .SubscribeAsyncSafe(async _ =>
                {
                    await SendHeartbeat();
                },
                ex => _log.LogWarning(ex, "Failed to send heartbeat request."),
                () => _log.LogDebug("HeartbeatSubscription completed."));

            await SendHeartbeat();
        }

        private void InitializeSDS()
        {
            _sDSSubscription = _tcClient.SDSReceived
                .Subscribe(dto =>
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _messageHub.Clients.All.SendAsync("MessageReceived", new
                            {
                                type = dto.Type,
                                source = ResolveSource(dto),
                                destination = dto.DestinationName ?? dto.DestinationSSI,
                                status = dto.Status,
                                statusCode = dto.StatusCode,
                                statusText = dto.StatusText,
                                text = dto.Text,
                                radioId = dto.RadioId,
                                radioName = dto.RadioName,
                                latitude = dto.Latitude,
                                longitude = dto.Longitude,
                                timestamp = dto.TimestampUTC
                            });
                        }
                        catch (Exception ex)
                        {
                            _log.LogWarning(ex, "Failed to broadcast SDS via SignalR.");
                        }
                    });

                    if (!_programOptions.SendAlarms && !_programOptions.SendUserAvailability && !_programOptions.SendUserOperationStatus)
                    {
                        _log.LogDebug($"Ignoring SDS because {nameof(_programOptions.SendAlarms)}, {nameof(_programOptions.SendUserAvailability)} and {nameof(_programOptions.SendUserOperationStatus)} are disabled in program options.");

                        return;
                    }

                    _log.LogInformation($"Processing SDS with Text '{dto.Text}' from ISSI '{dto.SourceSSI}'.");

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            // SDS comes from registered siren (Sirene24 sends SDS, not Status)
                            if (_connectOptions.Sites.SelectMany(s => s.Sirens.Select(si => si.Issi)).Contains(dto.SourceSSI))
                            {
                                _log.LogDebug("SDS comes from siren, so its probably a siren status...");
                                await _sirenService.HandleSirenStatuscode(dto);

                                return;
                            }

                            var type = dto.GetSdsType();

                            await _sdsService.HandleSds(dto);
                        }
                        catch (Exception ex)
                        {
                            _log.LogError(ex, "Failed to process SDS.");
                        }
                    });
                },
                e => _log.LogError(e, "Error while processing SDS."),
                () => _log.LogDebug("SDS subscription completed."));
        }

        private void InitializePosition()
        {
            _positionSubscription = _tcClient.PositionReceived
                .Subscribe(dto =>
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _messageHub.Clients.All.SendAsync("MessageReceived", new
                            {
                                type = dto.Type,
                                source = ResolveSource(dto),
                                destination = dto.DestinationName ?? dto.DestinationSSI,
                                status = dto.Status,
                                statusCode = dto.StatusCode,
                                statusText = dto.StatusText,
                                text = dto.Text,
                                radioId = dto.RadioId,
                                radioName = dto.RadioName,
                                latitude = dto.Latitude,
                                longitude = dto.Longitude,
                                timestamp = dto.TimestampUTC
                            });
                        }
                        catch (Exception ex)
                        {
                            _log.LogWarning(ex, "Failed to broadcast position via SignalR.");
                        }
                    });

                    if (!_programOptions.SendVehiclePositions)
                    {
                        _log.LogDebug($"Ignoring vehicle positions because {nameof(_programOptions.SendVehiclePositions)} is disabled in program options.");

                        return;
                    }

                    _log.LogInformation("Processing Position LAT {Latitude} LNG {Longitude} from ISSI {SourceSSI}.", dto.Latitude, dto.Longitude, dto.SourceSSI);

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _vehicleService.HandleVehiclePosition(dto);
                        }
                        catch (Exception ex)
                        {
                            _log.LogError(ex, "Failed to process Vehicle Position.");
                        }
                    });
                },
            e => _log.LogError(e, "Error while processing position."),
            () => _log.LogDebug("Position subscription completed."));
        }

        private void InitializeStatus()
        {
            _statusSubscription = _tcClient.StatusReceived
                .Subscribe(dto =>
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _messageHub.Clients.All.SendAsync("MessageReceived", new
                            {
                                type = dto.Type,
                                source = ResolveSource(dto),
                                destination = dto.DestinationName ?? dto.DestinationSSI,
                                status = dto.Status,
                                statusCode = dto.StatusCode,
                                statusText = dto.StatusText,
                                text = dto.Text,
                                radioId = dto.RadioId,
                                radioName = dto.RadioName,
                                latitude = dto.Latitude,
                                longitude = dto.Longitude,
                                timestamp = dto.TimestampUTC
                            });
                        }
                        catch (Exception ex)
                        {
                            _log.LogWarning(ex, "Failed to broadcast status via SignalR.");
                        }
                    });

                    var type = dto.GetStatusType();

                    _log.LogInformation("Status is of type {Type}", type);

                    if (type == StatusType.Unknown)
                    {
                        return;
                    }

                    if (type == StatusType.Vehicle)
                    {
                        if (!_programOptions.SendVehicleStatus)
                        {
                            _log.LogDebug($"Ignoring vehicle status because {nameof(_programOptions.SendVehicleStatus)} is disabled in program options.");

                            return;
                        }

                        _log.LogInformation("Processing vehicle status {Status} from ISSI {ISSI}.", dto.Status, dto.SourceSSI);

                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await _vehicleService.HandleVehicleStatus(dto);
                            }
                            catch (Exception ex)
                            {
                                _log.LogError(ex, "Failed to process vehicle status.");
                            }
                        });
                    }
                    else if (type == StatusType.Siren)
                    {
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await _sirenService.HandleSirenStatuscode(dto);
                            }
                            catch (Exception ex)
                            {
                                _log.LogError(ex, "Failed to process siren status.");
                            }
                        });
                    }


                },
            e => _log.LogError(e, "Error while processing status."),
            () => _log.LogDebug("Status subscription completed."));
        }

        private void InitializeConnectionState()
        {
            _connectionSubscription = _tcClient.IsConnected
                .DistinctUntilChanged()
                .Subscribe(isConnected =>
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            _log.LogInformation("Broadcasting connection state: {IsConnected}", isConnected);
                            MessageHub.SetConnectionState(isConnected);
                            await _messageHub.Clients.All.SendAsync("ConnectionStateChanged", new { isConnected });
                        }
                        catch (Exception ex)
                        {
                            _log.LogWarning(ex, "Failed to broadcast connection state via SignalR.");
                        }
                    });
                },
                e => _log.LogError(e, "Error while processing connection state."),
                () => _log.LogDebug("Connection state subscription completed."));
        }

        private static string ResolveSource(TetraControlDto dto) =>
            new[] { dto.SourceName, dto.SourceSSI, dto.RadioName, dto.RadioId.ToString() }
                .FirstOrDefault(s => !string.IsNullOrEmpty(s)) ?? string.Empty;
    }
}
