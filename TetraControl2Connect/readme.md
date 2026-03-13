# TetraControl2Connect

[🇩🇪 Deutsche Version](readme.de.md)

## Description
Connects multiple Connect sites (even across different organizations) with a single TetraControl instance.

## Prerequisites
* TetraControl must be running continuously (preferably on localhost — the WebSocket connection is unencrypted!)
* The web server must be enabled in TetraControl
* A user with sufficient web server permissions must be configured in TetraControl

## Important Notes
* When vehicles or users are added or modified, the application must be restarted.
* Data can only be processed when the ISSIs of vehicles and users are correctly stored in Connect.

## Configuration
All configuration is managed through the built-in Admin UI at `http://localhost:5050`. On first start, the setup wizard guides you through the essential settings.

### TetraControl Connection
Connection settings for TetraControl:
* `TetraControlHost` → Network address of the TetraControl instance (hostname or IP address)
* `TetraControlPort` → TetraControl port
* `TetraControlUsername` → Username of the user configured in TetraControl
* `TetraControlPassword` → Password of the user configured in TetraControl

### Program Options
* `SendVehicleStatus` — Whether to transmit vehicle status updates (default: true)
* `SendVehiclePositions` — Whether to transmit vehicle positions (default: true)
* `SendUserOperationStatus` — Whether to transmit user operation responses (default: true)
* `SendUserAvailability` — Whether to transmit user availability (tactical availability) (default: true)
* `SendAlarms` — Whether to evaluate and transmit callouts (alarms) (default: true)
* `UserAvailabilityLifetimeDays` — Number of days before a user's availability is reset
* `WebSocketReconnectTimeoutMinutes` — Time in minutes after which the WebSocket connection is re-established if no messages are received (default: 5 minutes)
* `PollForActiveOperationBeforeFallbackMaxRetryCount` — Maximum attempts to find an active operation in Connect before creating a fallback alarm (default: 4)
* `PollForActiveOperationBeforeFallbackDelay` — Delay between attempts to find an active operation (format HH:mm:ss, default: 00:00:10)
* `HeartbeatEndpointUrl` — URL for periodic HTTP GET health checks (e.g., for UptimeRobot; leave empty to disable)
* `HeartbeatInterval` — Interval for heartbeat calls (format HH:mm:ss; leave empty to disable)
* `IgnoreStatus5` — Whether to ignore status 5 (request to speak) (default: false)
* `IgnoreStatus0` — Whether to ignore status 0 (priority request to speak) (default: false)
* `IgnoreStatus9` — Whether to ignore status 9 (remote acknowledge / special control) (default: false)
* `AddPropertyForAlarmTexts` — Whether to add the pager alarm text as an additional field when updating operations
* `UseFullyQualifiedSubnetAddressForConnect` — Whether to use fully qualified subnet addresses (e.g., "T2C(71234567_&01 - SBI)") or short form ("SNA(&01)") (default: false)
* `IgnoreAlarmWithoutSubnetAddresses` — Whether to ignore callouts without subnet addresses (full GSSI alert) (default: false)
* `AcceptCalloutsForSirens` — Whether to process siren callouts (control codes, e.g., $2002) (default: false)
* `AcceptSDSAsCalloutsWithPattern` — Whether to evaluate SDS messages as callouts using patterns (default: false)

### Status Options
Configure the status values (numeric value, not text) sent by pagers for each status type. Refer to your pager configuration!
* `AvailableStatus` → "Available"
* `LimitedAvailableStatus` → "Limited availability"
* `NotAvailableStatus` → "Not available"
* `ComingStatus` → Operation response "Coming"
* `NotComingStatus` → Operation response "Not coming"
* `ComingLaterStatus` → Operation response "Coming later"

If a status is not used, set it to -1. Make sure this value is not used elsewhere in the pager configuration.
Multiple status values can be specified separated by semicolons: "123;456;789" (useful for mixed pager manufacturers, e.g., Motorola and Airbus).

### Severity Options
Controls the handling of alarm severity levels (optional):
* `UseServerityTranslationAsKeyword` — Use severity translation as keyword for fallback operations (instead of "ALARM") (default: true)
* `SeverityTranslations` — Translations for severity levels (depends on pager programming, only the number is transmitted by TetraControl). Falls back to "ALARM" if no translation is found.

### Siren Callout Options
Controls the handling of siren alarm control codes (optional):
* `UseSirenCodeTranslationAsKeyword` — Use control code translation as keyword for fallback operations (default: false)
* `SirenCodeTranslations` — Translations for siren control codes (state/region dependent). Falls back to "ALARM" or severity translation if no match is found.

### Siren Status Options
Controls the handling of siren status messages (siren monitoring):
* `FailureTranslations` — Translations for all error codes that sirens can report. Checks both the SDS message text (Sirene24) and the hexadecimal status value.

### Connect Options
Configure the API keys from Connect. The `Name` field is a freely chosen label for easier identification.
Multiple sites can be configured. Subnet addresses (`SubnetAddresses`) are only relevant for certain fire departments and can be left empty. The `AlarmDirectly` flag per subnet address controls whether an alarm is uploaded to Connect immediately after pager activation (bypassing the active operation search).

For siren monitoring, each site can have a list of assigned sirens. Defect reports will be created in the configured site.

```json
"ConnectOptions": {
    "Sites": [
        {
            "Name": "Site1",
            "Key": "<<SITE_API_KEY>>",
            "Sirens": [
              {
                "Issi": "1234567",
                "Name": "Siren Site1"
              }
            ],
            "SubnetAddresses": [
                {
                    "Name": "Loop1",
                    "GSSI": "12345678",
                    "SNA": "&01",
                    "AlarmDirectly": false
                }
            ]
        }
    ]
}
```

## How It Works
The application connects to the TetraControl web server via WebSocket and processes position data, status data, and SDS messages. Voice calls are ignored.

## License

This project is licensed under the [GNU Affero General Public License v3.0](../LICENSE).

## Copyright
Copyright Feuer Software GmbH

Website: https://feuersoftware.com

All rights reserved.