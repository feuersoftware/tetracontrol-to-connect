# Changelog TetraControl2Connect

## Version 2.1.19
* Heartbeat-Einstellungen ergänzt. Siehe Readme

## Version 2.1.20
* Heartbeat-Fehler behoben.

## Version 2.1.21
* Verarbeitung von verketteten Callouts (Also mehrere Subnetzadressen in einer Alarmierung)
* Verarbeitung von Sirenenalarmen
* Umstellung der Intervalle auf TimeSpans (siehe mitgelieferte AppSettings.json)

## Versionen 2.1.22 - 2.1.23
* Fehlerbehebungen im Zusammenhang mit verketteten Callouts

## Version 2.1.24
* Fehlerbehebungen im Zusammenhang mit verketteten Callouts
* Beachtung von Einsätzen, zu denen nachalarmiert wurde (Startzeitpunkt länger in der Vergangenheit)

## Version 2.1.25
* Unterdrückung von Status 5 und 0 in den AppSettings konfigurierbar. (BOOL "IgnoreStatus5" und "IgnoreStatus0")

## Version 2.1.26
* Unterstützung für Subnetzadressen für direkten Alarm (Neue Einstellung für jede Schleife BOOL "AlarmDirectly")
* Neue Einstellung: "AddPropertyForAlarmTexts" in den ProgramOptions: Wenn aktiv wird für alle Alarme ein Zusatzfeld mit dem Alarmtext zum Einsatz hinzugefügt

## Version 2.2.3
* Unterstützung für Callouts ohne Subnetzadressen (Vollalarm der GSSI)
  * Die Konfiguration muss nicht angepasst werden. Es werden alle Standorte alarmiert, die irgendeine Subnetzadresse mit der alarmierten GSSI in der Konfiguration haben.
* AddPropertyForAlarmTexts so angepasst, dass nur noch ein Zusatzfeld "Alarmtext TC" hinzugefügt wird und nicht mehr für jede Subnetzadresse
* Aktualisierung von Drittanbieterpaketen
* Bei Alarm aus Rückfallebene wird nun die Position des Standorts aus Connect als Position für den Einsatz übernommen (Keine Navigation mehr zur Nordsee)

## Version 2.2.4
* Neue Option unter "ProgramOptions" gibt es eine neue Einstellung "IgnoreAlarmWithoutSubnetAddresses". Dort kann man einstellen, ob Alarme, die an eine gesamte GSSI (ohne Sunetzadressen)
geschickt werden, ignoriert werden sollen. (true / false, Standard: false)

## Version 2.2.5
* Neue Option unter "ProgramOptions" gibt es eine neue Einstellung "AcceptCalloutsForSirens". Dort kann eingestellt werden, ob auch Alarmierungen für Sirenen 
verarbeitet werden sollen. (Callouts für Sirenen beginnen mit einem Steuerzeichen z.B. $2002). (true / false, Standard: false)

## Version 2.2.6
* Die Subnetzadressen werden nun bei Fallback-Alarmen nicht mehr in den Sachverhalt geschrieben, sondern vorher rausgefiltert
* Als Meldender (Reporter) wurde jetzt Name und ISSI des Absenders des Callouts hinzugefügt

## Version 2.2.7
* Kleinere Fehlerbehebungen

## Version 2.3.0
* Schweregrad (Severity) von Callouts wird nun als Stichwort für Fallback-Einsätze verwendet (Abschaltbar via SeverityOptions-> UseServerityTranslationAsKeyword auf false setzen)
* Neue Sektion "SeverityOptions" in den AppSettings (optional)
  * Standardmäßig eingetragen sind dort die Schweregrad-Übersetzungen von Hessen

## Version 2.3.1
* Falls am TetraControl ein Pager und ein HRT oder MRT angeschlossen sind, werden die "Gesendet:"-SDS vom Funkgerät hier ignoriert, um nicht zwei Einsätze in Connect zu erzeugen.

## Version 2.3.2
* Fehlerbehebungen, dass alarmierte Subnetzadressen bei Fallback-Alarmen mehrfach im RIC-Feld standen
* Statuscode 429 (Too many Requests) von Connect wird nun mit Retry behandelt

## Version 2.3.3
* Tippfehler in (Standard) Schweregrad "Hilfeleistung" behoben
* Fehler mit dem fehlerhaften Update von Koordinatne bei Einsatz-Updates behoben (Nach Update von T2C wurden die Koordinaten nicht mit übertragen und damit von OSM neu geholt)

## Version 2.3.4
* Karenzzeit für Einsatz-Aktualisierung auf Erstellt / Geupdated in den letzten 10 Minuten erhöht

## Version 2.3.5
* Einstellung "OnlyVehicleData" in den ProgramOptions durch granularere Einstellungen ersetzt. Neu sind:
  * SendVehicleStatus -> Fahrzeugstatus übertragen (Standard: true)
  * SendVehiclePositions -> Fahrzeugpositionen übertragen (Standard: true)
  * SendUserOperationStatus -> Einsatzrückmeldungen von Benutzern übertragen (Standard: true)
  * SendUserAvailability -> Benutzer-Verfügbarkeit (Taktische Verfügbarkeit) übertragen (Standard: true)
  * SendAlarms -> Callouts (Alarmierungen) auswerten und übertragen (Standard: true)
* "OnlyVehicleData" ist nicht mehr zu verwenden.

## Version 2.3.6
* Fahrzeugpositionen werden zwischengespeichert und Updates nur an Connect übertragen, wenn die Werte um mehr als 0.00008 verändern.
* Bei einem erstellten Einsatz wird die Quelle nun in das dafür vorgesehene Feld geschrieben

## Version 2.3.7
* Fehler (Vertauschter Längen- und Breitengrad) bei den Fahrzeugpositionen behoben

## Version 2.3.8
* Fehlerhafte Fallback-Alarmierung bei ungünstiger Anordnung/Abfolge mehrerer aktiver Einsätze in Connect behoben
* Mehr Intelligenz eingebaut, um mehrfache Fallback-Alarme für den gleichen Einsatz bei mehreren ausgelösten Schleifen zu verhindern.

## Version 2.3.9
* Bessere Parallelverarbeitung

## Version 2.3.10
* Fehlerbehebungen

## Version 2.3.11
* Status 9 ignorieren hinzugefügt. Neue Einstellung "IgnoreStatus9" unter ProgramOptions hinzugefügt. Details siehe Readme.

## Version 2.3.12
* Weiter verbesserte Parallelverarbeitung

## Version 2.3.13
* Verbessertes Logging
* Verbesserte Kompatibilität zu älteren TetraControl-Versionen

## Version 2.4.0
* Übersetzungen für Sirenen-Steuerzeichen hinzugefügt (optional, standardmäßig nicht aktiv). 
	* Neuer (optionaler) Abschnitt in den AppSettings: "SirenCalloutOptions"
	* "UseSirenCodeTranslationAsKeyword" -> Das Steuerzeichen der Sirene soll als Stichwort für den Einsatz verwendet werden (Bsp.: $2000 -> Warnung der Bevölkerung, Standard: false)
	* "SirenCodeTranslations" -> Hier können die Übersetzungen (analog zu den Schweregrad-Übersetzungen eingetragen werden). Standard ist Hessen.

## Version 2.5.0
* Neue Einstellung unter ProgramOptions "UpdateExistingOperations" hinzugefügt (optional, standardmäßig true)
	* Wenn auf true, dann wird ein vorhandener Einsatz in Connect mit den ausgelösten Subadressen aktualisiert (so wie es vorher immer war)
	* Wenn auf false, dann wird der Einsatz nicht aktualisiert, jedoch wird weiterhin ein Fallback-Einsatz erstellt, falls kein Einsatz aktiv gefunden wurde.

## Version 2.5.2
* Neue Konfigurationsmöglichkeit "PatternOptions" ergänzt.
	* Informationen über den Einsatz können hier mittels Regex aus dem SDS Text extrahiert werden
	* Die dabei im Feld "RIC" ausgelesenen Werte werden mit den Einträgen in den Subadressen mit einem Contains verglichen, um Einsätze auf Standorte aufzuteilen

## Version 2.5.4
* Kleinere Fehlerbehebungen

## Version 2.5.5
* Kleinere Fehlerbehebungen

## Version 2.5.6
* Fehlerbehebung SDS-Auswerung Meldender
* Aktualisierung von Drittanbieterpaketen

## Version 2.6.0
* Aktualisierung von Drittanbieterpaketen

## Version 2.6.1
* Bei Verwendung der SDS-Patternauswertung: Fallback wenn Stichwort oder Stadt leer ist

## Version 2.6.2
* Automatischer Retry, wenn HTTP-POST in Konflikt mit einem anderen gleichzeitigen Request steht (Statuscode 409)

## Version 2.6.3
* Heartbeat verbessert, sendet nun nur noch Heartbeats, wenn Verbindung zu TC in Ordnung ist

## Version 2.7.0
* Sirenenüberwachung als Feature hinzugefügt. Bei Störungen von Sirenen werden Mängelmeldungen in Connect erstellt.
	* Dazu muss in den ConnectOptions für jeden Standort ein neuer Abschnitt "Sirens" gepflegt werden.
	* Eine Siren besteht aus der ISSI des verbauten Digitalfunkgeräts und einem frei wählbaren Namen (dieser wird dann auch in der Mängelmeldung stehen)
	* Ein Konfigurationsbeispiel kann der Muster-Konfig in der Readme entnommen werden

## Version 2.7.1
* Sirenenüberwachung erweitert: Support für Sirene24 erweitert. Fehlermeldungen im neuen, optionalen Abschnitt "SirenStatusOptions" konfigurierbar.
	* Standardmäßig wird das genommen, was in der Beispielkonfiguration steht
	* Die "Liste" ist ein Wörterbuch. Auf der linken Seite muss das stehen, was von der Sirene entweder als Statuscode (z.B. E001) kommt oder im SDS-Text steht. Beim Text wird mit einem Contains gearbeitet, Groß- und Kleinschreibung ist egal
	* Auf der rechten Seite steht das, was dann in der Mängelmeldung stehen soll (als Übersetzung)

## Version 2.7.2
* Wenn es für eine Sirenenstörung eines bestimmten Typs in einem Standort bereits eine noch offene Mängelmeldung gibt, wird in diesem Standort keine weitere Mängelmeldung angelegt.

## Version 2.7.3
* Text der Mängelmeldung korrigiert, keine Logikänderungen oder neue Features

## Version 2.7.4
* Aktualisierung von Drittanbieterpaketen
* Sirenenüberwachung erstellt nun eine eigene Kategorie "TETRA-Sirenen" für die Mängelmeldungen in Connect. Keine Konfigurationsänderungen notwendig.

## Version 2.7.5
* "Alarmtext-TC" Zusatzfeld bei aktualisierten Einsätzen beinhaltet nun keine Subadressen mehr

## Version 2.7.6
* Fehlerbehebung bei Taktischer Verfügbarkeit und Einsatzrückmeldung von Personen in mehreren Organisationen mit der gleichen Pager-ISSI.
* Aktualisierung von Drittanbieterpaketen
* Aktualisierung auf .NET 9

## Version 2.7.7
* Fehlerbehebung Sirenenüberwachung: Wenn existierende Mängelmeldung für Sirene Status Abgelehnt oder Pausiert hat, wird eine neue Mängelmeldung angelegt
* Wenn die Mängelmeldung für eine Sirene älter ist als 14 Tage ist und immer noch nicht auf "gelöst" ist, wird eine neue angelegt.

## Version 2.8.0
* BREAKING: In der Konfiguration sind nur noch Standort-API-Schlüssel zulässig. Beim Start des Programms wird ein Fehler geworfen, wenn noch Organisationsschlüssel hinterlegt sind.

## Version 2.8.1
* Sirenen-Störungsmeldungen (Mängelmeldungen) lösen sich nun automatisch auf, wenn Sirene eine Klarmeldung (Heartbeat) schickt. 
* Sirenen-Heartbeat-Überwachung nun möglich. Unter Punkt Site->Siren nun neuer Parameter "ExpectedHeartbeatInterval". Dort kann im Format "HH:mm:ss" das Intervall eingetragen werden, wo ein Heartbeat erwartet wird. Falls dieser ausbleibt wird eine Störungmeldung erstellt.
* Heartbeats werden in einer Datei "heartbeats.json" im Arbeitsverzeichnis gespeichert.

## Version 2.8.2
* Sirenen-Heartbeat und Störungsmeldungen von Sonnenburg-Sirenen werden nun korrekt verarbeitet.
* Fehlerbehebung bei der Verarbeitung von Konfigurationswerten, die Dictionaries sind (z.B. Schweregradübersetzungen, Sirenen-Störungsübersetzungen). Diese werden wieder korrekt aus der Konfigurationsdatei gelesen.
  * Das bedeutet aber auch, dass die Konfiguration jetzt diese Werte auch enthalten muss, vorher wurden Standardwerte angenommen. Falls Werte fehlen, diese aus der mitglieferten AppSettings-datei reinkopieren als Standardwerte. 

## Version 2.8.3
* Wenn AcceptSDSAsCalloutsWithPattern auf true gesetzt ist, wird an die über Pattern evtl. ausgewertete RIC noch die Standard-Callout Informationen angehängt

## Version 2.8.4
* Bei der Detektion von Sirenen-Callouts wird nun auf "$2\d\d\d" geprüft, anstelle von "$\d\d\d\d"
* Separates Logging für Sirenen-Events hinzugefügt. Neues Logfile "siren_events_yyyymmdd.txt"

## Version 2.9.0
* Fallback-Einsätze bekommen eine generische Einsatznummer, die wie folgt aufgebaut ist: "T2C-FB-{DateTime.Now:yyyy-MM-dd}-{sds.ExtractCalloutReference() ?? -1}"

## Version 2.9.1
* Fehlerbehebung bzgl. Sirenen-Statuscode-Übersetzung nach TetraControl-Update