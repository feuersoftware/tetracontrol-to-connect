using FeuerSoftware.TetraControl2Connect.Models.TetraControl;
using FeuerSoftware.TetraControl2Connect.Shared.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using Websocket.Client;

namespace FeuerSoftware.TetraControl2Connect.Services
{
    public class TetraControlClient : IDisposable, ITetraControlClient
    {
        private readonly ILogger<TetraControlClient> _log;
        private readonly IOptionsMonitor<TetraControlOptions> _tetraControlOptions;
        private readonly IOptionsMonitor<ProgramOptions> _programOptions;
        private readonly WebsocketClient _wsClient;
        private readonly Subject<TetraControlDto> _statusSubject = new();
        private readonly Subject<TetraControlDto> _positionSubject = new();
        private readonly Subject<TetraControlDto> _sDSSubject = new();
        private readonly Subject<TetraControlDto> _sirenStatusSubject = new();
        private readonly Subject<bool> _connectionSubject = new();
        private IDisposable? _reconnectSubscription;
        private IDisposable? _disconnectionSubscription;
        private IDisposable? _dataReceivedSubscription;

        public TetraControlClient(
            ILogger<TetraControlClient> log,
            IOptionsMonitor<TetraControlOptions> tetraControlOptions,
            IOptionsMonitor<ProgramOptions> programOptions)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _tetraControlOptions = tetraControlOptions ?? throw new ArgumentNullException(nameof(tetraControlOptions));
            _programOptions = programOptions ?? throw new ArgumentNullException(nameof(programOptions));

            var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
            {
                Options =
                {
                    KeepAliveInterval = TimeSpan.FromSeconds(30),
                    Credentials = new NetworkCredential(
                        userName: _tetraControlOptions.CurrentValue.TetraControlUsername,
                        password: _tetraControlOptions.CurrentValue.TetraControlPassword),
                }
            });

            _wsClient = new(_tetraControlOptions.CurrentValue.WebSocketUri, factory)
            {
                ReconnectTimeout = TimeSpan.FromMinutes(_programOptions.CurrentValue.WebSocketReconnectTimeoutMinutes),
                IsReconnectionEnabled = true
            };
        }

        public IObservable<TetraControlDto> SDSReceived => _sDSSubject.AsObservable();

        public IObservable<TetraControlDto> PositionReceived => _positionSubject.AsObservable();

        public IObservable<TetraControlDto> StatusReceived => _statusSubject.AsObservable();

        public IObservable<TetraControlDto> SirenStatusReceived => _sirenStatusSubject.AsObservable();

        public IObservable<bool> IsConnected => _connectionSubject.AsObservable();

        public async Task Start()
        {
            _log.LogInformation("Connecting to TetraControl WebSocket at {Uri}...", _tetraControlOptions.CurrentValue.WebSocketUri);

            try
            {
                await _wsClient.Start();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "WebSocket connection to {Uri} failed.", _tetraControlOptions.CurrentValue.WebSocketUri);
                _connectionSubject.OnNext(false);
                return;
            }

            if (_wsClient.IsRunning)
            {
                _log.LogInformation("✓ Connected to TetraControl at {Uri}", _tetraControlOptions.CurrentValue.WebSocketUri);
            }
            else
            {
                _log.LogError("✗ Could not connect to TetraControl at {Uri}. Is the server running?", _tetraControlOptions.CurrentValue.WebSocketUri);
            }

            _connectionSubject.OnNext(_wsClient.IsRunning);
        }

        public async Task Stop()
        {
            _log.LogDebug("Stopping TetraControl client...");

            _positionSubject.OnCompleted();
            _sDSSubject.OnCompleted();
            _statusSubject.OnCompleted();

            await _wsClient.Stop(WebSocketCloseStatus.NormalClosure, "Disconnect");
            _log.LogInformation("TetraControl client stopped.");

            _connectionSubject.OnNext(false);
        }

        public void Dispose()
        {
            _reconnectSubscription?.Dispose();
            _disconnectionSubscription?.Dispose();
            _dataReceivedSubscription?.Dispose();
            _wsClient?.Dispose();
        }

        public void Init()
        {
            _disconnectionSubscription = _wsClient.DisconnectionHappened.Subscribe(info =>
            {
                _log.LogWarning("✗ Disconnected from TetraControl. Type: {Type}, CloseStatus: {Status}, Description: {Desc}",
                    info.Type, info.CloseStatus, info.CloseStatusDescription);
                if (info.Exception != null)
                    _log.LogWarning(info.Exception, "  Disconnect exception details:");
                _connectionSubject.OnNext(false);
            });

            _reconnectSubscription = _wsClient.ReconnectionHappened.Subscribe(info =>
            {
                _log.LogInformation("✓ Reconnected to TetraControl. Type: {Type}", info.Type);
                _connectionSubject.OnNext(true);
            });

            _dataReceivedSubscription = _wsClient.MessageReceived.Subscribe(message =>
            {
                if (string.IsNullOrEmpty(message.Text))
                {
                    _log.LogWarning("Message text was null or empty. Ignoring.");

                    return;
                }

                var dto = JsonSerializer.Deserialize<TetraControlDto>(message.Text);

                if (dto is null)
                {
                    _log.LogError("Could not deserialize message from TetraControl.");
                    _log.LogDebug("Message: '{@Message}'", message.Text);
                    return;
                }

                _connectionSubject.OnNext(true);

                if (dto.Type == MessageTypes.DEVUPDATE)
                {
                    _log.LogDebug("Dev update received. Ignoring.");
                    return;
                }

                if (dto.Type == MessageTypes.CALL)
                {
                    _log.LogDebug("Call received. Ignoring.");
                    return;
                }

                if (dto.Type == MessageTypes.STATUS)
                {
                    _log.LogDebug("Status {@Status}", dto);

                    _statusSubject.OnNext(dto);
                    return;
                }

                if (dto.Type == MessageTypes.POSITION)
                {
                    _log.LogDebug("Position {@Position}", dto);

                    _positionSubject.OnNext(dto);
                    return;
                }

                if (dto.Type == MessageTypes.SDS)
                {
                    _log.LogDebug("SDS {@SDS}", dto);

                    _sDSSubject.OnNext(dto);

                    return;
                }

                if (dto.Type == MessageTypes.CALLOUT)
                {
                    _log.LogDebug("Callout {@Callout}", dto);

                    _sDSSubject.OnNext(dto);

                    return;
                }

                _log.LogDebug("Received unsupported Message.");
                _log.LogDebug("Message {@Message}", dto);
            },
            e =>
            {
                _log.LogCritical(e, "Failed to get message from TetraControl.");
                _connectionSubject.OnNext(false);
            },
            () => _log.LogDebug("TetraControl Websocket subscription completed."));
        }
    }
}
