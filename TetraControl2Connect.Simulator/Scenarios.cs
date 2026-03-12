namespace FeuerSoftware.TetraControl2Connect.Simulator;

public static class Scenarios
{
    private static readonly Random Rng = Random.Shared;

    public static async Task FireAlarm(WebSocketServer server)
    {
        Console.WriteLine("\n--- Szenario: Brand (Feueralarm) ---\n");

        var calloutRef = Rng.Next(100000, 999999);
        var text = "&01&02F BMA - Brandmeldeanlage";

        Console.WriteLine("  → Alarmierung SDS...");
        await server.BroadcastAsync(MessageGenerator.SdsCallout(
            text, TestData.LeitstelleGssi, TestData.LeitstelleName,
            TestData.Vehicles[0].Issi, TestData.Vehicles[0].RadioName, severity: 8));
        await Task.Delay(500);

        await server.BroadcastAsync(MessageGenerator.Callout(
            text, TestData.LeitstelleGssi, TestData.Vehicles[0].Issi));
        await Task.Delay(2000);

        // Vehicle status "1" (Einsatzbereit) for multiple vehicles
        foreach (var v in TestData.Vehicles[..4])
        {
            Console.WriteLine($"  → Status 1 (Einsatzbereit): {v.RadioName}");
            await server.BroadcastAsync(MessageGenerator.VehicleStatus(v.RadioId, v.RadioName, "1"));
            await Task.Delay(1000);
        }

        // Callout feedback
        foreach (var v in TestData.Vehicles[..3])
        {
            Console.WriteLine($"  → Rückmeldung 'komme': {v.RadioName}");
            await server.BroadcastAsync(MessageGenerator.CalloutFeedback(
                v.Issi, v.RadioName, "komme", calloutRef));
            await Task.Delay(800);
        }

        // Status 3 (Einsatz übernommen)
        foreach (var v in TestData.Vehicles[..3])
        {
            Console.WriteLine($"  → Status 3 (Einsatz übernommen): {v.RadioName}");
            await server.BroadcastAsync(MessageGenerator.VehicleStatus(v.RadioId, v.RadioName, "3"));
            await Task.Delay(500);
        }

        // Position updates moving toward fire
        var targetLat = TestData.BaseLat + 0.005;
        var targetLon = TestData.BaseLon + 0.003;
        for (var step = 0; step < 3; step++)
        {
            foreach (var v in TestData.Vehicles[..3])
            {
                var lat = TestData.BaseLat + (targetLat - TestData.BaseLat) * (step + 1) / 3.0
                          + (Rng.NextDouble() - 0.5) * 0.001;
                var lon = TestData.BaseLon + (targetLon - TestData.BaseLon) * (step + 1) / 3.0
                          + (Rng.NextDouble() - 0.5) * 0.001;
                Console.WriteLine($"  → Position {v.RadioName}: {lat:F5}, {lon:F5}");
                await server.BroadcastAsync(MessageGenerator.VehiclePosition(v.RadioId, v.RadioName, lat, lon));
                await Task.Delay(300);
            }

            await Task.Delay(1000);
        }

        // Status 4 (Am Einsatzort)
        foreach (var v in TestData.Vehicles[..3])
        {
            Console.WriteLine($"  → Status 4 (Am Einsatzort): {v.RadioName}");
            await server.BroadcastAsync(MessageGenerator.VehicleStatus(v.RadioId, v.RadioName, "4"));
            await Task.Delay(500);
        }

        Console.WriteLine("\n--- Szenario beendet ---\n");
    }

    public static async Task TrafficAccident(WebSocketServer server)
    {
        Console.WriteLine("\n--- Szenario: Verkehrsunfall ---\n");

        var calloutRef = Rng.Next(100000, 999999);
        var text = "&01&03VU eingeklemmte Person";

        Console.WriteLine("  → Alarmierung SDS...");
        await server.BroadcastAsync(MessageGenerator.SdsCallout(
            text, TestData.LeitstelleGssi, TestData.LeitstelleName,
            TestData.Vehicles[0].Issi, TestData.Vehicles[0].RadioName, severity: 8));
        await Task.Delay(500);

        await server.BroadcastAsync(MessageGenerator.Callout(
            text, TestData.LeitstelleGssi, TestData.Vehicles[0].Issi));
        await Task.Delay(2000);

        // Status 3 (Einsatz übernommen)
        foreach (var v in TestData.Vehicles[..4])
        {
            Console.WriteLine($"  → Status 3 (Einsatz übernommen): {v.RadioName}");
            await server.BroadcastAsync(MessageGenerator.VehicleStatus(v.RadioId, v.RadioName, "3"));
            await Task.Delay(800);
        }

        // Callout feedback
        foreach (var v in TestData.Vehicles[..3])
        {
            Console.WriteLine($"  → Rückmeldung 'komme': {v.RadioName}");
            await server.BroadcastAsync(MessageGenerator.CalloutFeedback(
                v.Issi, v.RadioName, "komme", calloutRef));
            await Task.Delay(600);
        }

        // Position updates
        var accidentLat = TestData.BaseLat + 0.01;
        var accidentLon = TestData.BaseLon - 0.005;
        for (var step = 0; step < 3; step++)
        {
            foreach (var v in TestData.Vehicles[..3])
            {
                var lat = TestData.BaseLat + (accidentLat - TestData.BaseLat) * (step + 1) / 3.0
                          + (Rng.NextDouble() - 0.5) * 0.001;
                var lon = TestData.BaseLon + (accidentLon - TestData.BaseLon) * (step + 1) / 3.0
                          + (Rng.NextDouble() - 0.5) * 0.001;
                Console.WriteLine($"  → Position {v.RadioName}: {lat:F5}, {lon:F5}");
                await server.BroadcastAsync(MessageGenerator.VehiclePosition(v.RadioId, v.RadioName, lat, lon));
                await Task.Delay(300);
            }

            await Task.Delay(1000);
        }

        // Status 4 (Am Einsatzort)
        foreach (var v in TestData.Vehicles[..3])
        {
            Console.WriteLine($"  → Status 4 (Am Einsatzort): {v.RadioName}");
            await server.BroadcastAsync(MessageGenerator.VehicleStatus(v.RadioId, v.RadioName, "4"));
            await Task.Delay(500);
        }

        Console.WriteLine("\n--- Szenario beendet ---\n");
    }

    public static async Task SirenTest(WebSocketServer server)
    {
        Console.WriteLine("\n--- Szenario: Sirenentest ---\n");

        var calloutRef = Rng.Next(100000, 999999);

        foreach (var siren in TestData.Sirens)
        {
            Console.WriteLine($"  → Sirenenalarm: {siren.Name} ($2002)");
            await server.BroadcastAsync(MessageGenerator.SirenAlarm(
                TestData.LeitstelleGssi, siren.Issi, siren.Name, "$2002"));
            await Task.Delay(1000);
        }

        await Task.Delay(2000);

        // Siren status updates (E-codes)
        var eCodes = new[] { "E001", "E002", "E010" };
        foreach (var siren in TestData.Sirens)
        {
            foreach (var code in eCodes)
            {
                Console.WriteLine($"  → Sirenenstatus {siren.Name}: {code}");
                await server.BroadcastAsync(MessageGenerator.SirenStatus(siren.Issi, siren.Name, code));
                await Task.Delay(500);
            }
        }

        // Siren callout feedback
        foreach (var siren in TestData.Sirens)
        {
            Console.WriteLine($"  → Sirenen-Rückmeldung: {siren.Name}");
            await server.BroadcastAsync(MessageGenerator.CalloutFeedback(
                siren.Issi, siren.Name, "Sirenentest OK", calloutRef));
            await Task.Delay(500);
        }

        Console.WriteLine("\n--- Szenario beendet ---\n");
    }

    public static async Task UserAvailabilityChanges(WebSocketServer server)
    {
        Console.WriteLine("\n--- Szenario: Verfügbarkeitsänderungen ---\n");

        var users = new (string Issi, string Name)[]
        {
            ("73821001", "Flamm, Maximilian"),
            ("73821002", "Brand, Sabrina"),
            ("73821003", "Lösch, Florian"),
            ("73821004", "Schlauch, Petra"),
            ("73821005", "Rettung, Kai"),
            ("73821006", "Leiter, Jana"),
        };

        // Set some available
        foreach (var user in users[..4])
        {
            Console.WriteLine($"  → Verfügbar (15): {user.Name}");
            await server.BroadcastAsync(MessageGenerator.UserAvailability(user.Issi, user.Name, 15));
            await Task.Delay(500);
        }

        await Task.Delay(1000);

        // Set some unavailable
        foreach (var user in users[4..])
        {
            Console.WriteLine($"  → Nicht verfügbar (0): {user.Name}");
            await server.BroadcastAsync(MessageGenerator.UserAvailability(user.Issi, user.Name, 0));
            await Task.Delay(500);
        }

        await Task.Delay(1000);

        // Toggle a few
        Console.WriteLine($"  → Nicht verfügbar (0): {users[0].Name}");
        await server.BroadcastAsync(MessageGenerator.UserAvailability(users[0].Issi, users[0].Name, 0));
        await Task.Delay(500);

        Console.WriteLine($"  → Verfügbar (15): {users[5].Name}");
        await server.BroadcastAsync(MessageGenerator.UserAvailability(users[5].Issi, users[5].Name, 15));

        Console.WriteLine("\n--- Szenario beendet ---\n");
    }

    public static async Task ContinuousRandom(WebSocketServer server, CancellationToken ct)
    {
        Console.WriteLine("\n--- Szenario: Zufällige Nachrichten (Strg+C oder Enter zum Beenden) ---\n");

        while (!ct.IsCancellationRequested)
        {
            var v = TestData.Vehicles[Rng.Next(TestData.Vehicles.Length)];
            var action = Rng.Next(10);

            switch (action)
            {
                case < 4: // Status update (most common)
                {
                    var status = Rng.Next(1, 10).ToString();
                    Console.WriteLine($"  → Status {status}: {v.RadioName}");
                    await server.BroadcastAsync(MessageGenerator.VehicleStatus(v.RadioId, v.RadioName, status));
                    break;
                }
                case < 7: // Position update
                {
                    var lat = TestData.BaseLat + (Rng.NextDouble() - 0.5) * 0.02;
                    var lon = TestData.BaseLon + (Rng.NextDouble() - 0.5) * 0.02;
                    Console.WriteLine($"  → Position {v.RadioName}: {lat:F5}, {lon:F5}");
                    await server.BroadcastAsync(MessageGenerator.VehiclePosition(v.RadioId, v.RadioName, lat, lon));
                    break;
                }
                case < 9: // Availability
                {
                    var code = Rng.Next(2) == 0 ? 15 : 0;
                    Console.WriteLine($"  → Verfügbarkeit ({code}): {v.RadioName}");
                    await server.BroadcastAsync(MessageGenerator.UserAvailability(v.Issi, v.RadioName, code));
                    break;
                }
                default: // Alarm
                {
                    var text = "&01&02F BMA - Automatischer Alarm";
                    Console.WriteLine($"  → Alarm: {text}");
                    await server.BroadcastAsync(MessageGenerator.SdsCallout(
                        text, TestData.LeitstelleGssi, TestData.LeitstelleName,
                        v.Issi, v.RadioName));
                    break;
                }
            }

            var delay = Rng.Next(1000, 5001);
            try
            {
                await Task.Delay(delay, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        Console.WriteLine("\n--- Szenario beendet ---\n");
    }
}
