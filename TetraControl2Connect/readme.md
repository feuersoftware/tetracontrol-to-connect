# TetraControl2Connect

## Description
Connects multiple Connect sites (even across different organizations) with a single TetraControl instance.

## Beschreibung
Verbindet mehrere Connect-Standorte (auch aus unterschiedlichen Organisationen) mit einer TetraControl-Instanz.

---

## Prerequisites / Voraussetzungen
* TetraControl must be running continuously (preferably on localhost — WebSocket is unencrypted!)
* The web server must be enabled in TetraControl / Der Webserver muss in TetraControl aktiviert sein
* A user with sufficient web server permissions must be configured / Ein Benutzer mit ausreichenden Berechtigungen muss angelegt werden

## Important Notes / Hinweise
* When vehicles or users are added or modified, the application must be restarted.
* Data can only be processed when ISSIs are correctly stored in Connect.

## Configuration / Konfiguration

All configuration is managed through the Admin UI at `http://localhost:5050`. The setup wizard guides you through the essential settings on first start.

Die gesamte Konfiguration erfolgt über die Admin-Oberfläche unter `http://localhost:5050`. Der Einrichtungsassistent führt beim ersten Start durch die wichtigsten Einstellungen.

### TetraControl Connection / TetraControl-Verbindung
* `TetraControlHost` → Network address / Netzwerkadresse (hostname or IP)
* `TetraControlPort` → Port
* `TetraControlUsername` → Username / Benutzername
* `TetraControlPassword` → Password / Passwort

### Program Options / Programmoptionen
* `SendVehicleStatus` — Transmit vehicle status / Fahrzeugstatus übertragen (default: true)
* `SendVehiclePositions` — Transmit vehicle positions / Fahrzeugpositionen übertragen (default: true)
* `SendUserOperationStatus` — Transmit operation responses / Einsatzrückmeldungen übertragen (default: true)
* `SendUserAvailability` — Transmit user availability / Verfügbarkeiten übertragen (default: true)
* `SendAlarms` — Evaluate and transmit callouts / Alarmierungen auswerten (default: true)
* `UserAvailabilityLifetimeDays` — Days before availability reset / Tage bis Verfügbarkeit zurückgesetzt wird
* `WebSocketReconnectTimeoutMinutes` — Reconnect timeout in minutes / Reconnect-Timeout in Minuten (default: 5)
* `PollForActiveOperationBeforeFallbackMaxRetryCount` — Max retries to find active operation / Max. Versuche aktiven Einsatz zu finden (default: 4)
* `PollForActiveOperationBeforeFallbackDelay` — Delay between retries / Verzögerung zwischen Versuchen (format HH:mm:ss, default: 00:00:10)
* `HeartbeatEndpointUrl` — URL for health checks (e.g. UptimeRobot) / URL für Monitoring
* `HeartbeatInterval` — Heartbeat interval / Heartbeat-Intervall (format HH:mm:ss)
* `IgnoreStatus5` — Ignore status 5 (request to speak / Sprechwunsch) (default: false)
* `IgnoreStatus0` — Ignore status 0 (priority request / Priorisierter Sprechwunsch) (default: false)
* `IgnoreStatus9` — Ignore status 9 (remote ack / Fremdquittung) (default: false)
* `AddPropertyForAlarmTexts` — Add pager alarm text as additional field / Alarmtext als Zusatzfeld hinzufügen
* `UseFullyQualifiedSubnetAddressForConnect` — Use full subnet address format / Volle Subnetzadress-Darstellung (default: false)
* `IgnoreAlarmWithoutSubnetAddresses` — Ignore callouts without subnet addresses / Alarme ohne Subnetzadressen ignorieren (default: false)
* `AcceptCalloutsForSirens` — Process siren callouts / Sirenenalarme verarbeiten (default: false)
* `AcceptSDSAsCalloutsWithPattern` — Evaluate SDS as callouts using patterns / SDS als Callouts auswerten (default: false)

### Status Options / Status-Zuordnungen
Configure pager status values (numeric). Multiple values separated by semicolons (e.g. "123;456").

Statuswerte vom Pager (Zahlenwert). Mehrere Werte mit Semikolon trennen.

* `AvailableStatus` → Available / Verfügbar
* `LimitedAvailableStatus` → Limited availability / Bedingt verfügbar
* `NotAvailableStatus` → Not available / Nicht verfügbar
* `ComingStatus` → Coming / Komme
* `NotComingStatus` → Not coming / Komme nicht
* `ComingLaterStatus` → Coming later / Komme später

Set to -1 if unused / Nicht verwendete Status auf -1 setzen.

### Severity Options / Schweregrade
* `UseServerityTranslationAsKeyword` — Use severity translation as operation keyword / Als Stichwort verwenden (default: true)
* `SeverityTranslations` — Map severity numbers to keywords / Schweregrade zu Stichwörtern zuordnen

### Siren Callout Options / Sirenen-Alarmierung
* `UseSirenCodeTranslationAsKeyword` — Use control code as keyword / Steuercode als Stichwort verwenden (default: false)
* `SirenCodeTranslations` — Map control codes to keywords / Steuercodes zu Stichwörtern zuordnen

### Siren Status Options / Sirenen-Status
* `FailureTranslations` — Map error codes to descriptions / Fehlercodes zu Beschreibungen zuordnen

### Connect Options / Connect-Standorte
Configure API keys from Connect. Each site can have subnet addresses and sirens.

API-Keys aus Connect eintragen. Jeder Standort kann Subnetzadressen und Sirenen haben.

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

## How It Works / Funktionsweise

The application connects to TetraControl via WebSocket and processes position data, status data, and SDS messages. Voice calls are ignored.

Das Programm verbindet sich über WebSocket mit TetraControl und verarbeitet Positionsdaten, Statusdaten und SDS. Funkgespräche werden ignoriert.

## License / Lizenz

[GNU Affero General Public License v3.0](../LICENSE)

## Copyright
Copyright Feuer Software GmbH

Website: https://feuersoftware.com
