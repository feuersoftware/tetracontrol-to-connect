# TetraControl2Connect

## Deutsch

### Überblick
TetraControl2Connect verbindet eine TetraControl-Instanz mit mehreren Connect-Standorten, auch standort- und organisationsübergreifend. Die Anwendung verarbeitet Fahrzeugpositionen, Fahrzeugstatus, Benutzerrückmeldungen, Verfügbarkeiten, Alarmierungen und sirenenbezogene Meldungen und überträgt die relevanten Informationen an Connect.

### Voraussetzungen
- TetraControl muss dauerhaft laufen, idealerweise auf demselben System oder im lokalen Netz, da die WebSocket-Verbindung unverschlüsselt ist.
- Der Webserver muss in TetraControl aktiviert sein.
- In TetraControl muss ein Benutzer mit ausreichenden Rechten für den Webserver eingerichtet sein.

### Wichtige Hinweise
- Wenn Fahrzeuge oder Benutzer in Connect hinzugefügt oder geändert werden, muss die Anwendung neu gestartet werden.
- Daten können nur korrekt verarbeitet werden, wenn die ISSIs in Connect vollständig und korrekt gepflegt sind.

### Konfiguration
Die Konfiguration erfolgt über die Admin-Oberfläche unter `http://localhost:5050`. Beim ersten Start führt der Einrichtungsassistent durch die wichtigsten Einstellungen.

#### TetraControlOptions
- `TetraControlHost` – Hostname oder IP-Adresse der TetraControl-Instanz.
- `TetraControlPort` – Port des TetraControl-Webservers.
- `TetraControlUsername` – Benutzername für den Zugriff auf den Webserver.
- `TetraControlPassword` – Passwort für den Zugriff auf den Webserver.

#### ProgramOptions
- `SendVehicleStatus` – Fahrzeugstatus an Connect übertragen (`true` standardmäßig).
- `SendVehiclePositions` – Fahrzeugpositionen an Connect übertragen (`true` standardmäßig).
- `SendUserOperationStatus` – Einsatzrückmeldungen von Benutzern an Connect übertragen (`true` standardmäßig).
- `SendUserAvailability` – Verfügbarkeiten von Benutzern an Connect übertragen (`true` standardmäßig).
- `SendAlarms` – Alarmierungen auswerten und an Connect übertragen (`true` standardmäßig).
- `UpdateExistingOperations` – Bereits vorhandene Einsätze in Connect aktualisieren (`true` standardmäßig).
- `UserAvailabilityLifetimeDays` – Anzahl der Tage, nach denen Verfügbarkeiten zurückgesetzt werden.
- `WebSocketReconnectTimeoutMinutes` – Zeitfenster für automatische WebSocket-Wiederverbindungen in Minuten.
- `PollForActiveOperationBeforeFallbackMaxRetryCount` – Maximale Anzahl von Abfragen, um vor einem Fallback einen aktiven Einsatz zu finden (`4` standardmäßig).
- `PollForActiveOperationBeforeFallbackDelay` – Wartezeit zwischen diesen Abfragen im Format `HH:mm:ss` (`00:00:10` standardmäßig).
- `HeartbeatEndpointUrl` – URL für externe Verfügbarkeits- oder Health-Checks, zum Beispiel durch Monitoring-Systeme.
- `HeartbeatInterval` – Intervall für Heartbeat-Aufrufe im Format `HH:mm:ss`.
- `IgnoreStatus5` – Status 5 (Sprechwunsch) ignorieren (`false` standardmäßig).
- `IgnoreStatus0` – Status 0 (priorisierter Sprechwunsch) ignorieren (`false` standardmäßig).
- `IgnoreStatus9` – Status 9 (Fremdquittung) ignorieren (`false` standardmäßig).
- `AddPropertyForAlarmTexts` – Alarmtexte zusätzlich als Eigenschaft an Connect übergeben.
- `UseFullyQualifiedSubnetAddressForConnect` – Subnetzadressen in Connect im vollständig qualifizierten Format verwenden (`false` standardmäßig).
- `IgnoreAlarmWithoutSubnetAddresses` – Alarmierungen ohne passende Subnetzadressen ignorieren (`false` standardmäßig).
- `AcceptCalloutsForSirens` – Sirenenalarmierungen verarbeiten (`false` standardmäßig).
- `AcceptSDSAsCalloutsWithPattern` – SDS-Nachrichten anhand definierter Muster als Alarmierungen auswerten (`false` standardmäßig).

#### StatusOptions
Diese Werte definieren Pager-Status und werden numerisch angegeben. Mehrere Werte können mit Semikolon getrennt werden, zum Beispiel `123;456`. Nicht verwendete Einträge sollten auf `-1` gesetzt werden.

- `AvailableStatus` – Verfügbar.
- `LimitedAvailableStatus` – Bedingt verfügbar.
- `NotAvailableStatus` – Nicht verfügbar.
- `ComingStatus` – Komme.
- `NotComingStatus` – Komme nicht.
- `ComingLaterStatus` – Komme später.

#### SeverityOptions
- `UseServerityTranslationAsKeyword` – Übersetzte Schweregrade als Stichwort für den Einsatz verwenden (`true` standardmäßig).
- `SeverityTranslations` – Zuordnung von numerischen Schweregraden zu Stichwörtern.

#### SirenCalloutOptions
- `UseSirenCodeTranslationAsKeyword` – Übersetzten Sirenencode statt der allgemeinen Schweregrad-Zuordnung als Stichwort verwenden (`false` standardmäßig).
- `SirenCodeTranslations` – Zuordnung von Sirenencodes zu Stichwörtern.

#### SirenStatusOptions
- `FailureTranslations` – Zuordnung von Fehlercodes zu lesbaren Beschreibungen.

#### PatternOptions
Dieser Abschnitt ist vor allem relevant, wenn `AcceptSDSAsCalloutsWithPattern` aktiviert ist. Hier werden reguläre Muster definiert, mit denen Inhalte aus SDS-Nachrichten extrahiert werden.

- `NumberPattern` – Muster für die Einsatznummer.
- `KeywordPattern` – Muster für das Einsatzstichwort.
- `FactsPattern` – Muster für zusätzliche Einsatzinformationen.
- `ReporterNamePattern` – Muster für den Namen des Meldenden.
- `ReporterPhoneNumberPattern` – Muster für die Telefonnummer des Meldenden.
- `CityPattern` – Muster für den Ort.
- `StreetPattern` – Muster für die Straße.
- `HouseNumberPattern` – Muster für die Hausnummer.
- `ZipCodePattern` – Muster für die Postleitzahl.
- `DistrictPattern` – Muster für den Ortsteil oder Bezirk.
- `LatitudePattern` – Muster für den Breitengrad.
- `LongitudePattern` – Muster für den Längengrad.
- `RicPattern` – Muster für die RIC.
- `AdditionalProperties` – Zusätzliche frei definierbare Felder, die aus der SDS extrahiert werden.

#### ConnectOptions
In diesem Abschnitt werden die API-Schlüssel und die standortbezogenen Zuordnungen aus Connect gepflegt. Jeder Standort kann eigene Subnetzadressen und Sirenen enthalten.

```json
{
  "Sites": [
    {
      "Name": "Site1",
      "Key": "<<API_KEY>>",
      "Sirens": [
        { "Issi": "1234567", "Name": "Siren 1" }
      ],
      "SubnetAddresses": [
        { "Name": "Loop1", "GSSI": "12345678", "SNA": "&01", "AlarmDirectly": false }
      ]
    }
  ]
}
```

### Funktionsweise
Die Anwendung verbindet sich per WebSocket mit TetraControl und verarbeitet Positionsdaten, Statusdaten sowie SDS-Nachrichten. Sprachverbindungen werden nicht ausgewertet.

### Lizenz
[GNU Affero General Public License v3.0](../LICENSE)

### Copyright
Copyright Feuer Software GmbH

Website: https://feuersoftware.com

## English

### Overview
TetraControl2Connect connects a TetraControl instance to multiple Connect sites, including sites from different organizations. The application processes vehicle positions, vehicle statuses, user operation responses, availability updates, callouts, and siren-related messages and forwards the relevant information to Connect.

### Prerequisites
- TetraControl must run continuously, ideally on the same machine or within the local network, because the WebSocket connection is not encrypted.
- The web server must be enabled in TetraControl.
- A user with sufficient web server permissions must be configured in TetraControl.

### Important Notes
- If vehicles or users are added or changed in Connect, the application must be restarted.
- Data can only be processed correctly when ISSIs are maintained completely and accurately in Connect.

### Configuration
Configuration is managed through the Admin UI at `http://localhost:5050`. On first start, the setup wizard guides you through the most important settings.

#### TetraControlOptions
- `TetraControlHost` – Host name or IP address of the TetraControl instance.
- `TetraControlPort` – Port of the TetraControl web server.
- `TetraControlUsername` – User name for web server access.
- `TetraControlPassword` – Password for web server access.

#### ProgramOptions
- `SendVehicleStatus` – Send vehicle status updates to Connect (`true` by default).
- `SendVehiclePositions` – Send vehicle position updates to Connect (`true` by default).
- `SendUserOperationStatus` – Send user operation responses to Connect (`true` by default).
- `SendUserAvailability` – Send user availability updates to Connect (`true` by default).
- `SendAlarms` – Process and forward callouts to Connect (`true` by default).
- `UpdateExistingOperations` – Update operations that already exist in Connect (`true` by default).
- `UserAvailabilityLifetimeDays` – Number of days before availability states are reset.
- `WebSocketReconnectTimeoutMinutes` – Timeout window for automatic WebSocket reconnects in minutes.
- `PollForActiveOperationBeforeFallbackMaxRetryCount` – Maximum number of retries to find an active operation before falling back (`4` by default).
- `PollForActiveOperationBeforeFallbackDelay` – Delay between those retries in `HH:mm:ss` format (`00:00:10` by default).
- `HeartbeatEndpointUrl` – URL for external heartbeat or health-check monitoring.
- `HeartbeatInterval` – Interval for heartbeat calls in `HH:mm:ss` format.
- `IgnoreStatus5` – Ignore status 5 (request to speak) (`false` by default).
- `IgnoreStatus0` – Ignore status 0 (priority request to speak) (`false` by default).
- `IgnoreStatus9` – Ignore status 9 (remote acknowledgment) (`false` by default).
- `AddPropertyForAlarmTexts` – Add alarm texts to Connect as an additional property.
- `UseFullyQualifiedSubnetAddressForConnect` – Use fully qualified subnet address formatting in Connect (`false` by default).
- `IgnoreAlarmWithoutSubnetAddresses` – Ignore callouts without matching subnet addresses (`false` by default).
- `AcceptCalloutsForSirens` – Process siren callouts (`false` by default).
- `AcceptSDSAsCalloutsWithPattern` – Evaluate SDS messages as callouts by using configured patterns (`false` by default).

#### StatusOptions
These values define pager statuses and are configured as numeric values. Multiple values can be separated with semicolons, for example `123;456`. Set unused entries to `-1`.

- `AvailableStatus` – Available.
- `LimitedAvailableStatus` – Limited availability.
- `NotAvailableStatus` – Not available.
- `ComingStatus` – Coming.
- `NotComingStatus` – Not coming.
- `ComingLaterStatus` – Coming later.

#### SeverityOptions
- `UseServerityTranslationAsKeyword` – Use translated severity values as the operation keyword (`true` by default).
- `SeverityTranslations` – Mapping from numeric severity levels to operation keywords.

#### SirenCalloutOptions
- `UseSirenCodeTranslationAsKeyword` – Use the translated siren code as the operation keyword instead of the general severity mapping (`false` by default).
- `SirenCodeTranslations` – Mapping from siren codes to operation keywords.

#### SirenStatusOptions
- `FailureTranslations` – Mapping from failure codes to readable descriptions.

#### PatternOptions
This section is mainly relevant when `AcceptSDSAsCalloutsWithPattern` is enabled. It contains the patterns used to extract structured information from SDS messages.

- `NumberPattern` – Pattern for the operation number.
- `KeywordPattern` – Pattern for the operation keyword.
- `FactsPattern` – Pattern for additional operation details.
- `ReporterNamePattern` – Pattern for the reporter name.
- `ReporterPhoneNumberPattern` – Pattern for the reporter phone number.
- `CityPattern` – Pattern for the city.
- `StreetPattern` – Pattern for the street.
- `HouseNumberPattern` – Pattern for the house number.
- `ZipCodePattern` – Pattern for the postal code.
- `DistrictPattern` – Pattern for the district or locality.
- `LatitudePattern` – Pattern for the latitude.
- `LongitudePattern` – Pattern for the longitude.
- `RicPattern` – Pattern for the RIC.
- `AdditionalProperties` – Additional custom fields extracted from SDS messages.

#### ConnectOptions
This section contains the Connect API keys and site-specific mappings. Each site can define its own subnet addresses and sirens.

```json
{
  "Sites": [
    {
      "Name": "Site1",
      "Key": "<<API_KEY>>",
      "Sirens": [
        { "Issi": "1234567", "Name": "Siren 1" }
      ],
      "SubnetAddresses": [
        { "Name": "Loop1", "GSSI": "12345678", "SNA": "&01", "AlarmDirectly": false }
      ]
    }
  ]
}
```

### How It Works
The application connects to TetraControl through WebSocket and processes position data, status data, and SDS messages. Voice calls are ignored.

### License
[GNU Affero General Public License v3.0](../LICENSE)

### Copyright
Copyright Feuer Software GmbH

Website: https://feuersoftware.com
