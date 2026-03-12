using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace FeuerSoftware.TetraControl2Connect.Hubs
{
    public class MessageHub : Hub
    {
        private static volatile bool _tetraControlConnected;
        private readonly ILogger<MessageHub> _logger;

        public MessageHub(ILogger<MessageHub> logger)
        {
            _logger = logger;
        }

        public static void SetConnectionState(bool isConnected)
        {
            _tetraControlConnected = isConnected;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("SignalR client connected: {ConnectionId}. Sending current TetraControl state: {IsConnected}",
                Context.ConnectionId, _tetraControlConnected);
            await Clients.Caller.SendAsync("ConnectionStateChanged", new { isConnected = _tetraControlConnected });
            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("SignalR client disconnected: {ConnectionId}", Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
