namespace FeuerSoftware.TetraControl2Connect.Data
{
    // ── Singleton settings entities (one row per table, Id always = 1) ──

    public class ProgramSettingsEntity
    {
        public int Id { get; set; } = 1;
        public bool SendVehicleStatus { get; set; }
        public bool SendVehiclePositions { get; set; }
        public bool SendUserOperationStatus { get; set; }
        public bool SendUserAvailability { get; set; }
        public bool SendAlarms { get; set; }
        public bool UpdateExistingOperations { get; set; }
        public int UserAvailabilityLifetimeDays { get; set; }
        public int WebSocketReconnectTimeoutMinutes { get; set; }
        public int PollForActiveOperationBeforeFallbackMaxRetryCount { get; set; }
        public string PollForActiveOperationBeforeFallbackDelay { get; set; } = "00:00:05";
        public string HeartbeatEndpointUrl { get; set; } = string.Empty;
        public string? HeartbeatInterval { get; set; }
        public bool IgnoreStatus5 { get; set; }
        public bool IgnoreStatus0 { get; set; }
        public bool IgnoreStatus9 { get; set; }
        public bool AddPropertyForAlarmTexts { get; set; }
        public bool UseFullyQualifiedSubnetAddressForConnect { get; set; }
        public bool IgnoreAlarmWithoutSubnetAddresses { get; set; }
        public bool AcceptCalloutsForSirens { get; set; }
        public bool AcceptSDSAsCalloutsWithPattern { get; set; }
    }

    public class TetraControlSettingsEntity
    {
        public int Id { get; set; } = 1;
        public string TetraControlHost { get; set; } = "localhost";
        public int TetraControlPort { get; set; } = 80;
        public string TetraControlUsername { get; set; } = "Connect";
        public string TetraControlPassword { get; set; } = "Connect";
    }

    public class StatusSettingsEntity
    {
        public int Id { get; set; } = 1;
        public string AvailableStatus { get; set; } = string.Empty;
        public string LimitedAvailableStatus { get; set; } = string.Empty;
        public string NotAvailableStatus { get; set; } = string.Empty;
        public string ComingStatus { get; set; } = string.Empty;
        public string NotComingStatus { get; set; } = string.Empty;
        public string ComingLaterStatus { get; set; } = string.Empty;
    }

    public class PatternSettingsEntity
    {
        public int Id { get; set; } = 1;
        public string NumberPattern { get; set; } = string.Empty;
        public string KeywordPattern { get; set; } = string.Empty;
        public string FactsPattern { get; set; } = string.Empty;
        public string ReporterNamePattern { get; set; } = string.Empty;
        public string ReporterPhoneNumberPattern { get; set; } = string.Empty;
        public string CityPattern { get; set; } = string.Empty;
        public string StreetPattern { get; set; } = string.Empty;
        public string HouseNumberPattern { get; set; } = string.Empty;
        public string ZipCodePattern { get; set; } = string.Empty;
        public string DistrictPattern { get; set; } = string.Empty;
        public string LatitudePattern { get; set; } = string.Empty;
        public string LongitudePattern { get; set; } = string.Empty;
        public string RicPattern { get; set; } = string.Empty;
        public List<AdditionalPatternEntity> AdditionalProperties { get; set; } = [];
    }

    public class AdditionalPatternEntity
    {
        public int Id { get; set; }
        public int PatternSettingsId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Pattern { get; set; } = string.Empty;
    }

    public class SeveritySettingsEntity
    {
        public int Id { get; set; } = 1;
        public bool UseServerityTranslationAsKeyword { get; set; }
        public List<SeverityTranslationEntity> SeverityTranslations { get; set; } = [];
    }

    public class SeverityTranslationEntity
    {
        public int Id { get; set; }
        public int SeveritySettingsId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Translation { get; set; } = string.Empty;
    }

    public class SirenCalloutSettingsEntity
    {
        public int Id { get; set; } = 1;
        public bool UseSirenCodeTranslationAsKeyword { get; set; }
        public List<SirenCodeTranslationEntity> SirenCodeTranslations { get; set; } = [];
    }

    public class SirenCodeTranslationEntity
    {
        public int Id { get; set; }
        public int SirenCalloutSettingsId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Translation { get; set; } = string.Empty;
    }

    public class SirenStatusSettingsEntity
    {
        public int Id { get; set; } = 1;
        public List<FailureTranslationEntity> FailureTranslations { get; set; } = [];
    }

    public class FailureTranslationEntity
    {
        public int Id { get; set; }
        public int SirenStatusSettingsId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Translation { get; set; } = string.Empty;
    }

    // ── Collection entities for ConnectOptions ──

    public class SiteEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public List<SubnetAddressEntity> SubnetAddresses { get; set; } = [];
        public List<SirenEntity> Sirens { get; set; } = [];
    }

    public class SubnetAddressEntity
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SNA { get; set; } = string.Empty;
        public bool AlarmDirectly { get; set; }
        public string GSSI { get; set; } = string.Empty;
    }

    public class SirenEntity
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Issi { get; set; } = string.Empty;
        public string? ExpectedHeartbeatInterval { get; set; }
    }

    public class SettingsBackupEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Description { get; set; } = string.Empty;
        public string SnapshotJson { get; set; } = string.Empty;
    }
}
