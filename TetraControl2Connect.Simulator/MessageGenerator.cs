using System.Text.Json;

namespace FeuerSoftware.TetraControl2Connect.Simulator;

public static class MessageGenerator
{
    private static string Ts() => $"/Date({DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()})/";

    private static string Serialize(object msg) =>
        JsonSerializer.Serialize(msg, new JsonSerializerOptions { PropertyNamingPolicy = null });

    public static string VehicleStatus(int radioId, string radioName, string status) =>
        Serialize(new
        {
            type = "status",
            status,
            statusCode = "",
            statusText = StatusText(status),
            destSSI = "",
            destName = "",
            srcSSI = "",
            srcName = "",
            radioID = radioId,
            radioName,
            remark = "",
            Alt = 0,
            FixQual = 0,
            Lat = 0.0,
            Lon = 0.0,
            text = "",
            ts = Ts()
        });

    public static string VehiclePosition(int radioId, string radioName, double lat, double lon) =>
        Serialize(new
        {
            type = "pos",
            status = "",
            statusCode = "",
            statusText = "",
            destSSI = "",
            destName = "",
            srcSSI = "",
            srcName = "",
            radioID = radioId,
            radioName,
            remark = "",
            Alt = 0,
            FixQual = 1,
            Lat = lat,
            Lon = lon,
            text = "",
            ts = Ts()
        });

    public static string SdsCallout(string text, string srcSSI, string srcName,
        string destSSI, string destName, int severity = 8)
    {
        var calloutRef = Random.Shared.Next(100000, 999999);
        return Serialize(new
        {
            type = "sds",
            status = "",
            statusCode = "",
            statusText = "",
            destSSI,
            destName,
            srcSSI,
            srcName,
            radioID = 0,
            radioName = "",
            remark = $"-1;{severity};{calloutRef};",
            Alt = 0,
            FixQual = 0,
            Lat = 0.0,
            Lon = 0.0,
            text,
            ts = Ts()
        });
    }

    public static string Callout(string text, string srcSSI, string destSSI) =>
        Serialize(new
        {
            type = "callout",
            status = "",
            statusCode = "",
            statusText = "",
            destSSI,
            destName = "",
            srcSSI,
            srcName = "",
            radioID = 0,
            radioName = "",
            remark = "",
            Alt = 0,
            FixQual = 0,
            Lat = 0.0,
            Lon = 0.0,
            text,
            ts = Ts()
        });

    public static string CalloutFeedback(string srcSSI, string srcName, string text, int calloutRef) =>
        Serialize(new
        {
            type = "sds",
            status = "",
            statusCode = "",
            statusText = "",
            destSSI = "",
            destName = "",
            srcSSI,
            srcName,
            radioID = 0,
            radioName = "",
            remark = $"3;-1;{calloutRef};0",
            Alt = 0,
            FixQual = 0,
            Lat = 0.0,
            Lon = 0.0,
            text,
            ts = Ts()
        });

    public static string UserAvailability(string srcSSI, string srcName, int availabilityCode) =>
        Serialize(new
        {
            type = "sds",
            status = "",
            statusCode = "",
            statusText = "",
            destSSI = "",
            destName = "",
            srcSSI,
            srcName,
            radioID = 0,
            radioName = "",
            remark = $"5;-1;0;{availabilityCode}",
            Alt = 0,
            FixQual = 0,
            Lat = 0.0,
            Lon = 0.0,
            text = "",
            ts = Ts()
        });

    public static string SirenAlarm(string srcSSI, string sirenIssi, string sirenName, string code) =>
        Serialize(new
        {
            type = "sds",
            status = "",
            statusCode = "",
            statusText = "",
            destSSI = sirenIssi,
            destName = sirenName,
            srcSSI,
            srcName = "",
            radioID = 0,
            radioName = "",
            remark = "",
            Alt = 0,
            FixQual = 0,
            Lat = 0.0,
            Lon = 0.0,
            text = code,
            ts = Ts()
        });

    public static string SirenStatus(string sirenIssi, string sirenName, string statusCode) =>
        Serialize(new
        {
            type = "status",
            status = "",
            statusCode,
            statusText = "",
            destSSI = "",
            destName = "",
            srcSSI = sirenIssi,
            srcName = sirenName,
            radioID = 0,
            radioName = "",
            remark = "",
            Alt = 0,
            FixQual = 0,
            Lat = 0.0,
            Lon = 0.0,
            text = "",
            ts = Ts()
        });

    private static string StatusText(string status) => status switch
    {
        "1" => "Einsatzbereit über Funk",
        "2" => "Einsatzbereit auf Wache",
        "3" => "Einsatz übernommen",
        "4" => "Am Einsatzort",
        "5" => "Sprechwunsch",
        "6" => "Nicht einsatzbereit",
        "7" => "Patient aufgenommen",
        "8" => "Am Transportziel",
        "9" => "Handsender",
        _ => ""
    };
}
