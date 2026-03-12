using FeuerSoftware.TetraControl2Connect.Simulator;

var port = args.Length > 0 && int.TryParse(args[0], out var p) ? p : 8085;

using var server = new WebSocketServer(port);
using var cts = new CancellationTokenSource();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

try
{
    await server.StartAsync(cts.Token);
}
catch (IOException ex) when (ex.InnerException is Microsoft.AspNetCore.Connections.AddressInUseException)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine($"ERROR: Port {port} is already in use.");
    Console.Error.WriteLine($"Either stop the process using that port, or specify a different port:");
    Console.Error.WriteLine($"  dotnet run -- <port>");
    Console.ResetColor();
    return 1;
}

Console.WriteLine("TetraControl Simulator");
Console.WriteLine("======================");
Console.WriteLine($"Listening on port: {port}");
Console.WriteLine($"WebSocket endpoint: ws://localhost:{port}/live.json");
Console.WriteLine($"Auth: Connect / Connect\n");

while (!cts.Token.IsCancellationRequested)
{
    Console.WriteLine($"Connected clients: {server.ClientCount}\n");
    Console.WriteLine("Select scenario:");
    Console.WriteLine("  1. Brand (Feueralarm)");
    Console.WriteLine("  2. Verkehrsunfall");
    Console.WriteLine("  3. Sirenentest");
    Console.WriteLine("  4. Verfügbarkeitsänderungen");
    Console.WriteLine("  5. Zufällige Nachrichten (kontinuierlich)");
    Console.WriteLine("  0. Beenden");
    Console.Write("\n> ");

    var input = Console.ReadLine();
    if (input is null || cts.Token.IsCancellationRequested)
        break;

    switch (input.Trim())
    {
        case "1":
            await Scenarios.FireAlarm(server);
            break;
        case "2":
            await Scenarios.TrafficAccident(server);
            break;
        case "3":
            await Scenarios.SirenTest(server);
            break;
        case "4":
            await Scenarios.UserAvailabilityChanges(server);
            break;
        case "5":
            using (var randomCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token))
            {
                var task = Scenarios.ContinuousRandom(server, randomCts.Token);
                Console.WriteLine("  (Enter drücken zum Beenden des Szenarios)");
                _ = Task.Run(() =>
                {
                    Console.ReadLine();
                    randomCts.Cancel();
                });
                await task;
            }

            break;
        case "0":
            cts.Cancel();
            break;
        default:
            Console.WriteLine("Ungültige Auswahl.\n");
            break;
    }
}

Console.WriteLine("Simulator beendet.");
return 0;
