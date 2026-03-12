# TetraControl2Connect

## Beschreibung
Dient der Verbindung von mehreren Connect-Standorten (auch aus unterschiedlichen Organisationen) mit einer TetraControl-Instanz. 

## Voraussetzungen
* TetraControl muss auf durchgehend laufen (am besten auf localhost, Websocket-Verbindung ist nicht verschl�sselt!)
* In TetraControl muss der Webserver aktiviert sein
* In TetraControl muss ein enstprechender Benutzer mit ausreichenden Berechtigungen f�r den Webserver angelegt werden.

## Hinweise
* Wenn Fahrzeuge oder Benutzer neu angelegt oder ge�ndert werden, muss das Programm neu gestartet werden.
* Wir k�nnen nur Daten verarbeiten, wenn die ISSIs der Fahrzeuge und Benutzer in Connect korrekt hinterlegt sind.

## Konfiguration
Die Konfiguration wird in der Datei `appsettings.json` vorgenommen. Diese muss im gleichen Verzeichnis wie die Anwendung selbst liegen. 

### Abschnitt `TetraControlOptions`
Dies ist die Konfiguration der Verbindung zu TetraControl. 
* `TetraControlHost` -> Netzwerkadresse der TetraControl-Instanz (Hostname oder IP-Adresse)
* `TetraControlPort` -> Port von TetraControl
* `TetraControlUsername` -> Benutzername des in TetraControl angelegten Benutzers
* `TetraControlPassword` -> Passwort des in TetraControl angelegten Benutzers

### Abschnitt `ProgramOptions`
* Unter `SendVehicleStatus` kann eingestellt werden, ob Fahrzeugstatus �bertragen werden (Standard: true)
* Unter `SendVehiclePositions` kann eingestellt werden, ob Fahrzeugpositionen �bertragen werden (Standard: true)
* Unter `SendUserOperationStatus` kann eingestellt werden, ob Einsatzr�ckmeldungen von Benutzern �bertragen werden (Standard: true)
* Unter `SendUserAvailability` kann eingestellt werden, ob Benutzer-Verf�gbarkeiten (Taktische Verf�gbarkeit) �bertragen werden (Standard: true)
* Unter `SendAlarms` kann eingestellt werden, ob Callouts (Alarmierungen) ausgewertet und �bertragen werden (Standard: true)
* Unter `UserAvailabilityLifetimeDays` kann die Anzahl der Tage eingestellt werden, wie lange eine Verf�gbarkeit f�r einen Benutzer gelten soll, bis sie zur�ckgesetzt wird.
* Unter `WebSocketReconnectTimeoutMinutes` kann die Zeitspanne in Minuten eingestellt werden, nachdem die WebSocket-Verbindung zu TetraControl neu hergestellt werden soll, nachdem in dieser keine Nachrichten von TetraControl empfangen worden sind. (Standard: 5 Minuten)
* Unter `PollForActiveOperationBeforeFallbackMaxRetryCount` kann die Anzahl der maximalen Versuche eingestellt werden, um nach einem aktiven Einsatz in Connect zu schauen, bevor ein Fallback-Alarm gegeben wird. (Standard: 4)
* Unter `PollForActiveOperationBeforeFallbackDelay` kann die Zeitverz�gerung zwischen den Versuchen nach einem aktiven Einsatz in Connect zu schauen eingestellt werden. (Format HH:mm:ss, Standard 00:00:10)
* Unter `HeartbeatEndpointUrl` kann die Url angegeben werden, zu der im eingestellen Intervall ein HTTP-GET Request gesendet wird. (z.B. f�r UptimeRobot, falls leer gelassen wird kein Heartbeat gesendet)
* Unter `HeartbeatInterval` kann das Intervall eingestellt werden, wie oft der Heartbeat-Aufruf erfolgen soll. (Format HH:mm:ss, wenn kein Wert wird kein Heartbeat gesendet)
* Unter `IgnoreStatus5` kann eingestellt werden, ob Status 5 (Sprechwunsch) ignoriert werden soll. (True/False, Standard: False)
* Unter `IgnoreStatus0` kann eingestellt werden, ob Status 0 (Priorisierter Sprechwunsch) ignoriert werden soll. (True/False, Standard: False)
* Unter `IgnoreStatus9` kann eingestellt werden, ob Status 9 (Fremdquittung / Sondersteuerungen) ignoriert werden soll. (True/False, Standard: False)
* Unter `AddPropertyForAlarmTexts` kann eingestellt werden, ob bei einer Einsatz-Aktualisierung nach Alarmeingang auch ein Zusatzfeld mit dem Alarmtext vom Pager erstellt werden soll
* Unter `UseFullyQualifiedSubnetAddressForConnect` kann eingestellt werden, ob die Darstellung der alarmierten Subnetzadressen ausf�hrlich oder gek�rzt erfolgen soll. (Bei True: z.B. "T2C(71234567_&01 - SBI"), bei False (standard): "SNA(&01)")
* Unter `IgnoreAlarmWithoutSubnetAddresses` kann eingestellt werden, ob Callouts (Alarme) ohne Subnetzadressen (also an die gesamte GSSI) ignoriert werden sollen (Standard: false)
* Unter `AcceptCalloutsForSirens` kann eingestellt werden, ob Callouts (Alarme) f�r Sirenen (beginnend mit Steuerzeichen z.B. $2002) verarbeitet werden sollen (Standard: false)

### Abschnitt `StatusOptions`
Hier m�ssen die Statuswerte (Zahlenwert, nicht der Text) eingestellt werden, die vom Pager f�r die einzelnen Status gesendet werden. (Bitte Melderkonfiguration beachten!)
* `AvailableStatus` -> Status "Verf�gbar"
* `LimitedAvailableStatus` -> Status "Bedingt verf�gbar"
* `NotAvailableStatus` -> Status "nicht verf�gbar"
* `ComingStatus` -> Einsatzr�ckmeldung "komme"
* `NotComingStatus` -> Einsatzr�ckmeldung "komme nicht"
* `ComingLaterStatus` -> Einsatzr�ckmeldung "komme sp�ter"

Wenn ein Status nicht verwendet wird, kann dort -1 eingetragen werden. Wichtig ist, dass dieser Status nicht an anderen Stellen in der Melderkonfiguration verwendet wird, um Missverst�ndnisse zu vermeiden.
Ab Version 2.1.9 ist es m�glich, hier mehrere Statuswerte in folgendem Format einzutragen "123;456;789". (N�tzlich, um z.B. verschiedene Pager-Hersteller gemischt auszuwerten (z.B. Motorola und Airbus)).

### Abschnitt `SeverityOptions`
Hier wird der Umgang mit den Schweregraden von Alarmierungen gesteuert. (optional)
* `UseServerityTranslationAsKeyword` -> Schweregrad-�bersetzung als Stichwort f�r Fallback-Eins�tze verwenden (anstelle von "ALARM") (Standard: True mit den Standardwerten von Hessen)
* `SeverityTranslations` -> Da die Schweregrad-Texte von der Melderprogrammierung abh�ngig sind und nur die Zahl von TetraControl �bermittelt wird, k�nnen hier die �bersetzungen der Schweregrade eingetragen werden. (Standard: Hessen) Falls keine �bersetzung gefunden wird, wird "ALARM" genommen

### Abschnitt `SirenCalloutOptions`
Hier wird der Umgang mit den Steuerzeichen von Sirenenalarmierungen gesteuert. (optional)
* `UseSirenCodeTranslationAsKeyword` -> Steuercode-�bersetzung als Stichwort f�r Fallback-Eins�tze verwenden (anstelle von "ALARM" bzw. der Schweregrad-�bersetzung) (Standard: False)
* `SirenCodeTranslations` -> Da die Steuerzeichen-Texte vom Bundesland abh�ngig sind, k�nnen hier die �bersetzungen der Steuerzeichen eingetragen werden. (Standard: Hessen) Falls keine �bersetzung gefunden wird, wird "ALARM" bzw. wenn konfiguriert die Schweregrad-�bersetzung genommen

### Abschnitt `SirenStatusOptions`
Hier wird der Umgang mit den Statusmeldungen von Sirenen geregelt. (Sirenen�berwachung)
* `FailureTranslations` -> �bersetzung aller Fehlercodes, die von der Sirene kommen k�nnen. Dabei wird sowohl auf den Meldungstext der SDS geschaut (Sirene24) als auch auf den hexadezimalen Statuswert

### Abschnitt `ConnectOptions`
Hier m�ssen die API-Keys aus Connect eingetragen werden. Unter `Name` kann ein frei w�hlbarer Bezeichner hinterlegt werden, um die Schl�ssel besser zuordnen zu k�nnen.
Eintragen mehrerer Schl�ssel erfolgt nach der JSON-Syntax. (Siehe https://developer.mozilla.org/de/docs/Learn/JavaScript/Objects/JSON#arrays_als_json)
Die SubnetAddresses sind aktuell nur f�r bestimmte Feuerwehren relevant und m�ssen nicht ausgef�llt werden. Unter `AlarmDirectly` kann pro Schleife eingestellt werden, ob ein Alarm direkt nach Pagerausl�sung nach Connect hochgeladen werden soll. 
Dann wird f�r diese Schleife nicht versucht, einen vorhandenen Einsatz zu aktualisieren.
F�r die Sirenen�berwachung muss unter jedem Standort eine Liste der zugeordneten Sirenen erstellt werden. In dem konfigurierten Standort werden dann auch die M�ngelmeldungen erfasst.
```json
"ConnectOptions": {
    "Sites": [
        {
            "Name": "Standort1",
            "Key": "<<STANDORTKEY1>>",
            "Sirens": [
              {
                "Issi": "1234567",
                "Name": "Sirene Musterstadt 1"
              },
              {
                "Issi": "1234568",
                "Name": "Sirene Musterstadt 2"
              }
            ],
            "SubnetAddresses": [
                {
                    "Name": "Schleife1",
                    "GSSI": "12345678",
                    "SNA": "&01",
                    "AlarmDirectly": false
                },
                {
                    "Name": "Schleife2",
                    "GSSI": "12345678",
                    "SNA": "&01",
                    "AlarmDirectly": false
                }
            ]
        },
        {
            "Name": "Standort2",
            "Key": "<<STANDORTKEY2>>",
            "SubnetAddresses": [
                {
                    "Name": "Schleife1",
                    "GSSI": "12345678",
                    "SNA": "&01",
                    "AlarmDirectly": false
                },
                {
                    "Name": "Schleife2",
                    "GSSI": "12345678",
                    "SNA": "&01",
                    "AlarmDirectly": false
                }
            ]
        }
    ]
}
```

## Funktionsweise
Das Programm verbindet sich �ber eine Websocket-Verbindung mit dem Webserver von TetraControl und verarbeitet Positionsdaten, Statusdaten und SDS. Funkgespr�che werden ignoriert.

## Vollst�ndies Konfigurationsbeispiel:
```json
{
  // Verbindungseinstellungen zu TetraControl
  "TetraControlOptions": {
    "TetraControlHost": "localhost",
    "TetraControlPort": 80,
    "TetraControlUsername": "Connect",
    "TetraControlPassword": "Connect"
  },
  // Einstellungen zum Programm allgemein
  "ProgramOptions": {
    // Aktualisiere vorhandene Eins�tze in Connect bei Alarmausl�sung (F�ge ausgel. SNA in RIC hinzu)
    "UpdateExistingOperations": true,
    // Sende Fahrzeugstatus
    "SendVehicleStatus": true,
    // Sende Fahrzeugpositionen (GPS)
    "SendVehiclePositions": true,
    // Sende Pager-Einsatzr�ckmeldungen (Komme/Komme nicht)
    "SendUserOperationStatus": true,
    // Sende taktische Verf�gbarkeitsmeldungen (Verf�gbar / nicht verf�gbar)
    "SendUserAvailability": true,
    // Erstelle Eins�tze (Fallback)
    "SendAlarms": true,
    // Anzahl in Tagen, bis die taktische Verf�gbarkeit wieder auf den Standard zur�ckgesetzt werden soll
    "UserAvailabilityLifetimeDays": 7,
    // Nachrichten-Timeout f�r TetraControl
    "WebSocketReconnectTimeoutMinutes": 5,
    // Anzahl der Versuche einen aktiven Einsatz zu finden, bevor ein Fallback-Einsatz erstellt wird
    "PollForActiveOperationBeforeFallbackMaxRetryCount": 4,
    // Verz�gerung zwischen den Versuchen einen aktiven Einsatz zu finden
    "PollForActiveOperationBeforeFallbackDelay": "00:00:10",
    // GET-Endpunkt f�r Monitoring-Systeme
    "HeartbeatEndpointUrl": "",
    // Invervall, in dem ein Heartbeat gesendet werden soll
    "HeartbeatInterval": "00:10:00",
    // Ignoriere Status 5
    "IgnoreStatus5": false,
    // Ignoriere Status 0
    "IgnoreStatus0": false,
    // Ignoriere Status 9
    "IgnoreStatus9": false,
    // F�ge den Alarmtext des Pagers bei der Einsatz-Aktualisierung als Zusatzfeld hinzu
    "AddPropertyForAlarmTexts": false,
    // Verwende f�r die RIC die volle Subadresse inkl. GSSI und Name
    "UseFullyQualifiedSubnetAddressForConnect": false,
    // Ignoriere Alarme ohne Subadressen (Vollalarm GSSI)
    "IgnoreAlarmWithoutSubnetAddresses": false,
    // Verarbeite Sirenenalarme (Steuercodes z.B. $2000)
    "AcceptCalloutsForSirens": false,
    // Verarbeite SDS als Callouts und werte sie mit den Pattern aus
    "AcceptSDSAsCalloutsWithPattern": false
  },
  // Einstellungen f�r Statuswerte von den Pagern (Siehe Pagerkonfiguration, mehrere Statuswerte k�nnen mit Semikolon getrennt werden)
  "StatusOptions": {
    // Statuswert f�r "Verf�gbar"
    "AvailableStatus": "15",
    // Statuswert f�r "Bedingt verf�gbar" (i.d.R. nicht vorhanden)
    "LimitedAvailableStatus": "-1",
    // Statuswert f�r "Nicht verf�gbar"
    "NotAvailableStatus": "0",
    // Statuswert f�r "Komme"
    "ComingStatus": "32768;57345",
    // Statuswert f�r "Komme nicht"
    "NotComingStatus": "32769;57344",
    // Statuswert f�r "Komme sp�ter" (i.d.R. nicht vorhanden)
    "ComingLaterStatus": "-1"
  },
  // Einstellungen f�r die �bersetzung von Schweregraden f�r das Stichwort bei Alarmierung (siehe Pagerkonfiguration)
  "SeverityOptions": {
    "UseServerityTranslationAsKeyword": true,
    "SeverityTranslations": {
      "1": "Information",
      "2": "Einsatzabbruch",
      "3": "Bereitschaft",
      "4": "Krankentransport",
      "5": "Rettungsdienst R-0",
      "7": "Hilfeleistung normal",
      "8": "Feuer normal",
      "9": "Rettungsdienst R-1",
      "10": "Rettungsdienst R-2",
      "11": "Hilfeleistung dringend",
      "12": "Feuer dringend",
      "13": "Gro�alarm",
      "15": "KatS-Alarm"
    }
  },
  // Einstellungen f�r die SDS-Alarmauswertung mit Pattern
  "PatternOptions": {
    "KeywordPattern": "",
    "FactsPattern": "",
    "ReporterNamePattern": "",
    "ReporterPhoneNumberPattern": "",
    "CityPattern": "",
    "StreetPattern": "",
    "HouseNumberPattern": "",
    "ZipCodePattern": "",
    "DistrictPattern": "",
    "LatitudePattern": "",
    "LongitudePattern": "",
    "RicPattern": "",
    "AdditionalProperties": [
      {
        "Name": "Zusatzfeld 1",
        "Pattern": ""
      }
    ]
  },
  // Einstellungen f�r Sirenenalarme (Steuerzeichen)
  "SirenCalloutOptions": {
    "UseSirenCodeTranslationAsKeyword": false,
    "SirenCodeTranslations": {
      "$2000": "Warnung der Bev�lkerung",
      "$2001": "Entwarnung",
      "$2002": "Feueralarm",
      "$2003": "Probealarm"
    }
  },
  // Einstellungen f�r Statusmeldungen von Sirenensteuerungen
  "SirenStatusOptions": {
    "FailureTranslations": {
      "E001": "Nicht ausgel�st, Sirene hat auf eine Alarmierung nicht ausgel�st",
      "E003": "Alarmierung: Besetzt und abgelehnt, Sirene war zum Zeitpunkt der Alarmierung mit einem anderen Auftrag belegt.",
      "E005": "Technischer Status Fehler, Sirene nicht f�r Alarmierung verf�gbar",
      "E006": "Sirene tempor�r abgeschaltet, Sirene steht nicht f�r Alarmierungen zur Verf�gung",
      "E007": "Sabotagealarm, T�rkontakt ge�ffnet",
      "E008": "Fehler Netzstromversorgung",
      "E009": "Fehler Batteriestromversorgung",
      "E00A": "�bertemperatur (�berhitzung, Brand)",
      "Fehler bei der Alarmausl�sung": "Nicht ausgel�st, Sirene hat auf eine Alarmierung nicht ausgel�st",
      "STATUS=1": "Technischer Status Fehler (allgemein)",
      "SPRT Sabotage: ge�ffnet": "Sabotagealarm, T�rkontakt ge�ffnet",
      "SPRT Fehler Netz, Batteriebetrieb": "Fehler Netzstromversorgung",
      "SPRT Batteriespannung niedrig": "Fehler Batteriestromversorgung",
      "SPRT Temperatur zu hoch!": "�bertemperatur (�berhitzung, Brand)",
      "SPRT Sammelst�rung": "Sammelst�rung, nicht n�her bezeichnet"
    }
  },
  // Einstellungen f�r die Verbindung zu Feuer Software Connect
  "ConnectOptions": {
    // Liste der Standorte
    "Sites": [
      {
        "Name": "Standort1",
        "Key": "<<STANDORTKEY OEFFENTLICHE API>>",
        // Liste der Sirenen des Standors f�r Sirenen�berwachung
        "Sirens": [
          {
            "Issi": "1234567",
            "ExpectedHeartbeatInterval": null, // z.B. "24:00:00" f�r Heartbeat alle 24 Stunden erwartet, null f�r Sirene sendet keine Heartbeats
            "Name": "Sirene Musterstadt"
          }
        ],
        // Liste der Subnetzadressen des Standorts
        "SubnetAddresses": [
          {
            "Name": "Schleife1",
            "GSSI": "12345678",
            "SNA": "&01",
            "AlarmDirectly": false
          }
        ]
      }
    ]
  }
}
```

## Copyright
Copyright Feuer Software GmbH

Internet: https://feuersoftware.com

Alle Rechte vorbehalten.