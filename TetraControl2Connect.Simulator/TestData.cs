namespace FeuerSoftware.TetraControl2Connect.Simulator;

public static class TestData
{
    public record Vehicle(int RadioId, string RadioName, string Issi);
    public record Siren(string Issi, string Name);

    public static readonly Vehicle[] Vehicles =
    [
        new(1001, "FF-Musterstadt 1/10-1", "73820011"),
        new(1002, "FF-Musterstadt 1/44-1", "73820012"),
        new(1003, "FF-Musterstadt 1/23-1", "73820013"),
        new(2001, "FF-Flammenhausen 2/10-1", "73820021"),
        new(2002, "FF-Flammenhausen 2/44-1", "73820022"),
        new(3001, "FF-Musterhausen 3/10-1", "73820031"),
    ];

    public static readonly Siren[] Sirens =
    [
        new("73829001", "Sirene Rathaus Musterstadt"),
        new("73829002", "Sirene Feuerwehrhaus Flammenhausen"),
    ];

    public const string LeitstelleGssi = "73825001";
    public const string LeitstelleName = "Leitstelle Musterkreis";

    public const double BaseLat = 51.33;
    public const double BaseLon = 9.47;
}
