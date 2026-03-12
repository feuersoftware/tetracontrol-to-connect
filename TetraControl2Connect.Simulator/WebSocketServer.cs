using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FeuerSoftware.TetraControl2Connect.Simulator;

public class WebSocketServer : IDisposable
{
    private readonly ConcurrentDictionary<Guid, WebSocket> _clients = new();
    private readonly WebApplication _app;
    private CancellationTokenSource? _cts;

    public int ClientCount => _clients.Count;

    public WebSocketServer(int port)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = [] });
        builder.WebHost.ConfigureKestrel(o => o.ListenLocalhost(port));
        builder.Logging.SetMinimumLevel(LogLevel.Warning);

        _app = builder.Build();
        _app.UseWebSockets();

        _app.Map("/live.json", async context =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                Console.WriteLine($"  [HTTP] Rejected: not a WebSocket upgrade request");
                context.Response.StatusCode = 400;
                return;
            }

            LogAuth(context);
            Console.WriteLine($"  [WS] Accepting WebSocket upgrade...");

            var ws = await context.WebSockets.AcceptWebSocketAsync();
            await HandleClientAsync(ws, _cts?.Token ?? CancellationToken.None);
        });

        _app.MapFallback(context =>
        {
            var path = context.Request.Path;
            Console.WriteLine($"  [HTTP] Rejected: path '{path}' != '/live.json'");
            context.Response.StatusCode = 404;
            return Task.CompletedTask;
        });
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Console.WriteLine($"  [Server] Kestrel started, waiting for connections...");
        await _app.StartAsync(_cts.Token);
    }

    private static void LogAuth(HttpContext context)
    {
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader))
        {
            Console.WriteLine("  [Auth] No credentials provided (accepted without auth)");
            return;
        }

        if (authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            var encoded = authHeader["Basic ".Length..].Trim();
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            Console.WriteLine($"  [Auth] Basic credentials: {decoded}");
        }
        else
        {
            Console.WriteLine($"  [Auth] Auth header present: {authHeader[..Math.Min(authHeader.Length, 20)]}...");
        }
    }

    private async Task HandleClientAsync(WebSocket ws, CancellationToken ct)
    {
        var id = Guid.NewGuid();
        _clients.TryAdd(id, ws);
        Console.WriteLine($"  [WS] Client connected ({ClientCount} total)");

        try
        {
            var buffer = new byte[1024];
            while (ws.State == WebSocketState.Open && !ct.IsCancellationRequested)
            {
                var result = await ws.ReceiveAsync(buffer, ct);
                if (result.MessageType == WebSocketMessageType.Close)
                    break;
            }
        }
        catch (OperationCanceledException) { }
        catch (WebSocketException) { }
        finally
        {
            _clients.TryRemove(id, out _);
            Console.WriteLine($"  [WS] Client disconnected ({ClientCount} total)");
            if (ws.State is WebSocketState.Open or WebSocketState.CloseReceived)
            {
                try
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down",
                        CancellationToken.None);
                }
                catch { /* best effort */ }
            }

            ws.Dispose();
        }
    }

    public async Task BroadcastAsync(string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        var segment = new ArraySegment<byte>(bytes);

        var deadClients = new List<Guid>();

        foreach (var (id, ws) in _clients)
        {
            if (ws.State != WebSocketState.Open)
            {
                deadClients.Add(id);
                continue;
            }

            try
            {
                await ws.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch
            {
                deadClients.Add(id);
            }
        }

        foreach (var id in deadClients)
        {
            if (_clients.TryRemove(id, out var ws))
                ws.Dispose();
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        foreach (var (_, ws) in _clients)
            ws.Dispose();
        _clients.Clear();
        _app.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _cts?.Dispose();
    }
}
