namespace FeuerSoftware.TetraControl2Connect.Shared.Options
{
    public record ProgramOptions
    {
        public const string SectionName = nameof(ProgramOptions);

        private bool _sendUserOperationStatus = true;
        private bool _sendUserAvailability = true;
        private bool _sendAlarms = true;

        [Obsolete($"Use {nameof(SendVehicleStatus)}, {nameof(SendVehiclePositions)}, {nameof(SendUserOperationStatus)} and {nameof(SendUserAvailability)} instead.")]
        public bool OnlyVehicleData { get; set; } = false;

        public bool SendVehicleStatus { get; set; } = true;

        public bool SendVehiclePositions { get; set; } = true;

#pragma warning disable CS0618 // Type or member is obsolete
        public bool SendUserOperationStatus
        {
            get => _sendUserOperationStatus && !OnlyVehicleData;
            set => _sendUserOperationStatus = value;
        }

        public bool SendUserAvailability
        {
            get => _sendUserAvailability && !OnlyVehicleData;
            set => _sendUserAvailability = value;
        }

        public bool SendAlarms
        {
            get => _sendAlarms && !OnlyVehicleData;
            set => _sendAlarms = value;
        }
#pragma warning restore CS0618 // Type or member is obsolete

        public bool UpdateExistingOperations { get; set; } = true;

        public int UserAvailabilityLifetimeDays { get; set; }

        public int WebSocketReconnectTimeoutMinutes { get; set; }

        public int PollForActiveOperationBeforeFallbackMaxRetryCount { get; set; } = 4;

        public TimeSpan PollForActiveOperationBeforeFallbackDelay { get; set; } = TimeSpan.FromSeconds(10);

        public string HeartbeatEndpointUrl { get; set; } = string.Empty;

        public TimeSpan? HeartbeatInterval { get; set; } = null;

        public bool IgnoreStatus5 { get; set; } = false;

        public bool IgnoreStatus0 { get; set; } = false;

        public bool IgnoreStatus9 { get; set; } = false;

        public bool AddPropertyForAlarmTexts { get; set; } = false;

        public bool UseFullyQualifiedSubnetAddressForConnect { get; set; } = false;

        public bool IgnoreAlarmWithoutSubnetAddresses { get; set; } = false;

        public bool AcceptCalloutsForSirens { get; set; } = false;

        public bool AcceptSDSAsCalloutsWithPattern { get; set; } = false;
    }
}
