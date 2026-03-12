<script setup lang="ts">
import type { StepperItem } from '@nuxt/ui'

definePageMeta({ layout: false })

const { updateSettings, saving } = useSettings()
const toast = useToast()

const stepper = ref()
const completed = ref(false)

const steps: StepperItem[] = [
  { slot: 'welcome', title: 'Willkommen', icon: 'i-lucide-rocket' },
  { slot: 'tetracontrol', title: 'TetraControl', icon: 'i-lucide-radio' },
  { slot: 'connect', title: 'Connect', icon: 'i-lucide-network' },
  { slot: 'options', title: 'Optionen', icon: 'i-lucide-settings' },
  { slot: 'summary', title: 'Fertig', icon: 'i-lucide-check-circle' }
]

// TetraControl settings
const tetraControl = ref({
  id: 1,
  webSocketHost: 'localhost',
  webSocketPort: 8085,
  tetraControlUsername: 'Connect',
  tetraControlPassword: 'Connect'
})

// First Connect site
const site = ref({
  id: 0,
  connectSettingsId: 1,
  name: '',
  key: '',
  subnetAddresses: [] as any[],
  sirens: [] as any[]
})

// Program options
const programOptions = ref({
  id: 1,
  sendVehiclePosition: true,
  sendVehicleStatus: true,
  sendUserAvailability: true,
  sendAlarms: true,
  openBrowserOnStartup: true,
  webSocketReconnectTimeoutMinutes: 5,
  heartbeatIntervalMinutes: 15,
  positionCacheEnabled: true,
  suppressedVehicleStatusCodes: ''
})

async function saveAll() {
  let success = true

  success = await updateSettings('tetracontrol', tetraControl.value) && success

  const connectSettings = {
    id: 1,
    sites: [site.value]
  }
  success = await updateSettings('connect', connectSettings) && success
  success = await updateSettings('program', programOptions.value) && success

  if (success) {
    completed.value = true
    toast.add({
      title: 'Einrichtung abgeschlossen',
      description: 'Alle Einstellungen wurden gespeichert. Du kannst die Konfiguration jederzeit über die Einstellungsseiten anpassen.',
      color: 'success'
    })
  }
}
</script>

<template>
  <div class="min-h-screen bg-default">
    <UApp>
      <div class="max-w-3xl mx-auto px-4 py-8">
        <!-- Header -->
        <div class="text-center mb-8">
          <h1 class="text-3xl font-bold">TetraControl2Connect</h1>
          <p class="text-muted mt-2">Einrichtungsassistent</p>
        </div>

        <!-- Stepper -->
        <UStepper
          ref="stepper"
          :items="steps"
          color="primary"
          size="md"
          class="mb-8"
        >
          <!-- Step: Welcome -->
          <template #welcome>
            <UCard>
              <div class="text-center py-8 space-y-6">
                <div class="inline-flex p-4 rounded-full bg-primary/10">
                  <UIcon name="i-lucide-rocket" class="w-12 h-12 text-primary" />
                </div>
                <div>
                  <h2 class="text-xl font-bold mb-2">Willkommen bei TetraControl2Connect!</h2>
                  <p class="text-muted max-w-lg mx-auto">
                    Dieser Assistent führt dich durch die Ersteinrichtung. In wenigen Schritten
                    verbindest du TetraControl mit Feuer Software Connect.
                  </p>
                </div>
                <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 max-w-lg mx-auto text-left">
                  <div class="flex items-start gap-2">
                    <UIcon name="i-lucide-radio" class="w-5 h-5 text-primary shrink-0 mt-0.5" />
                    <div>
                      <p class="text-sm font-medium">TetraControl</p>
                      <p class="text-xs text-muted">Verbindung einrichten</p>
                    </div>
                  </div>
                  <div class="flex items-start gap-2">
                    <UIcon name="i-lucide-network" class="w-5 h-5 text-primary shrink-0 mt-0.5" />
                    <div>
                      <p class="text-sm font-medium">Connect</p>
                      <p class="text-xs text-muted">Standort konfigurieren</p>
                    </div>
                  </div>
                  <div class="flex items-start gap-2">
                    <UIcon name="i-lucide-settings" class="w-5 h-5 text-primary shrink-0 mt-0.5" />
                    <div>
                      <p class="text-sm font-medium">Optionen</p>
                      <p class="text-xs text-muted">Datenübertragung wählen</p>
                    </div>
                  </div>
                </div>
              </div>
              <template #footer>
                <div class="flex justify-end">
                  <UButton label="Weiter" icon="i-lucide-arrow-right" trailing @click="stepper?.next()" />
                </div>
              </template>
            </UCard>
          </template>

          <!-- Step: TetraControl -->
          <template #tetracontrol>
            <UCard>
              <div class="space-y-6">
                <div>
                  <h2 class="text-xl font-bold mb-1">TetraControl-Verbindung</h2>
                  <p class="text-sm text-muted">Konfiguriere die WebSocket-Verbindung zum TetraControl-Server.</p>
                </div>

                <div class="space-y-4">
                  <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
                    <UFormField label="Host" description="IP-Adresse oder Hostname des TetraControl-Servers" class="sm:col-span-2">
                      <UInput v-model="tetraControl.webSocketHost" placeholder="z.B. 192.168.1.100" class="w-full" />
                    </UFormField>
                    <UFormField label="Port" description="Standard: 8085">
                      <UInput v-model.number="tetraControl.webSocketPort" type="number" class="w-full" />
                    </UFormField>
                  </div>
                  <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <UFormField label="Benutzername">
                      <UInput v-model="tetraControl.tetraControlUsername" class="w-full" />
                    </UFormField>
                    <UFormField label="Passwort">
                      <UInput v-model="tetraControl.tetraControlPassword" type="password" class="w-full" />
                    </UFormField>
                  </div>
                </div>
              </div>
              <template #footer>
                <div class="flex items-center justify-between">
                  <UButton label="Zurück" icon="i-lucide-arrow-left" variant="outline" @click="stepper?.prev()" />
                  <UButton
                    label="Weiter"
                    icon="i-lucide-arrow-right"
                    trailing
                    :disabled="!tetraControl.webSocketHost || tetraControl.webSocketPort <= 0"
                    @click="stepper?.next()"
                  />
                </div>
              </template>
            </UCard>
          </template>

          <!-- Step: Connect Site -->
          <template #connect>
            <UCard>
              <div class="space-y-6">
                <div>
                  <h2 class="text-xl font-bold mb-1">Erster Connect-Standort</h2>
                  <p class="text-sm text-muted">Füge deinen ersten Feuer Software Connect-Standort hinzu. Weitere Standorte kannst du später in den Einstellungen ergänzen.</p>
                </div>

                <div class="space-y-4">
                  <UFormField label="Standortname" required description="Ein beschreibender Name für den Standort">
                    <UInput v-model="site.name" placeholder="z.B. Feuerwehr Musterstadt" class="w-full" />
                  </UFormField>
                  <UFormField label="API-Schlüssel" required>
                    <UInput v-model="site.key" placeholder="API-Key aus Feuer Software Connect" type="password" class="w-full" />
                    <template #description>
                      Den API-Schlüssel findest du im <a href="https://connect.feuersoftware.com/v2/app/interfaces" target="_blank" class="text-primary hover:underline">Connect-Portal</a> unter „Öffentliche Connect-Schnittstelle".
                    </template>
                  </UFormField>
                </div>

                <UCard variant="subtle">
                  <div class="flex gap-3">
                    <UIcon name="i-lucide-info" class="w-5 h-5 text-primary shrink-0 mt-0.5" />
                    <div class="text-sm text-muted">
                      <p>Jeder Standort benötigt einen eigenen API-Schlüssel aus Feuer Software Connect. Subnetz-Adressen (SNA/GSSI) und Sirenen kannst du anschließend in den Detail-Einstellungen konfigurieren.</p>
                    </div>
                  </div>
                </UCard>
              </div>
              <template #footer>
                <div class="flex items-center justify-between">
                  <UButton label="Zurück" icon="i-lucide-arrow-left" variant="outline" @click="stepper?.prev()" />
                  <UButton
                    label="Weiter"
                    icon="i-lucide-arrow-right"
                    trailing
                    :disabled="!site.name || !site.key"
                    @click="stepper?.next()"
                  />
                </div>
              </template>
            </UCard>
          </template>

          <!-- Step: Options -->
          <template #options>
            <UCard>
              <div class="space-y-6">
                <div>
                  <h2 class="text-xl font-bold mb-1">Programmoptionen</h2>
                  <p class="text-sm text-muted">Wähle, welche Daten an Connect übertragen werden sollen.</p>
                </div>

                <div class="space-y-4">
                  <div class="space-y-3">
                    <div class="flex items-center justify-between py-2 border-b border-default">
                      <div>
                        <p class="text-sm font-medium">Fahrzeugpositionen</p>
                        <p class="text-xs text-muted">GPS-Positionen der Fahrzeuge</p>
                      </div>
                      <USwitch v-model="programOptions.sendVehiclePosition" />
                    </div>
                    <div class="flex items-center justify-between py-2 border-b border-default">
                      <div>
                        <p class="text-sm font-medium">Fahrzeugstatus</p>
                        <p class="text-xs text-muted">Status-Änderungen (z.B. Status 1-6)</p>
                      </div>
                      <USwitch v-model="programOptions.sendVehicleStatus" />
                    </div>
                    <div class="flex items-center justify-between py-2 border-b border-default">
                      <div>
                        <p class="text-sm font-medium">Verfügbarkeit</p>
                        <p class="text-xs text-muted">Verfügbarkeitsstatus der Einsatzkräfte</p>
                      </div>
                      <USwitch v-model="programOptions.sendUserAvailability" />
                    </div>
                    <div class="flex items-center justify-between py-2">
                      <div>
                        <p class="text-sm font-medium">Alarme (SDS)</p>
                        <p class="text-xs text-muted">Alarmierungen aus SDS-Nachrichten</p>
                      </div>
                      <USwitch v-model="programOptions.sendAlarms" />
                    </div>
                  </div>
                </div>

                <UCard v-if="programOptions.sendAlarms" variant="subtle">
                  <div class="flex gap-3">
                    <UIcon name="i-lucide-alert-triangle" class="w-5 h-5 text-warning shrink-0 mt-0.5" />
                    <div class="text-sm text-muted">
                      <p><strong>Wichtig für Alarme (SDS):</strong> Damit Einsätze aus SDS-Nachrichten korrekt erkannt und an Connect übertragen werden, musst du nach der Ersteinrichtung unter <strong>Muster (Pattern)</strong> die passenden Regex-Muster konfigurieren.</p>
                      <p class="mt-2">Diese Muster bestimmen, wie Einsatznummer, Stichwort, Adresse und weitere Informationen aus dem Alarmtext extrahiert werden. Ohne konfigurierte Muster können empfangene Alarme nicht ausgewertet werden.</p>
                    </div>
                  </div>
                </UCard>
              </div>
              <template #footer>
                <div class="flex items-center justify-between">
                  <UButton label="Zurück" icon="i-lucide-arrow-left" variant="outline" @click="stepper?.prev()" />
                  <UButton label="Weiter" icon="i-lucide-arrow-right" trailing @click="stepper?.next()" />
                </div>
              </template>
            </UCard>
          </template>

          <!-- Step: Summary -->
          <template #summary>
            <UCard v-if="!completed">
              <div class="space-y-6">
                <div>
                  <h2 class="text-xl font-bold mb-1">Zusammenfassung</h2>
                  <p class="text-sm text-muted">Überprüfe deine Einstellungen und speichere die Konfiguration.</p>
                </div>

                <div class="space-y-4">
                  <UCard>
                    <template #header>
                      <div class="flex items-center gap-2">
                        <UIcon name="i-lucide-radio" class="w-4 h-4 text-primary" />
                        <h3 class="font-semibold text-sm">TetraControl</h3>
                      </div>
                    </template>
                    <dl class="grid grid-cols-2 gap-2 text-sm">
                      <dt class="text-muted">Host</dt>
                      <dd>{{ tetraControl.webSocketHost }}:{{ tetraControl.webSocketPort }}</dd>
                      <dt class="text-muted">Benutzer</dt>
                      <dd>{{ tetraControl.tetraControlUsername }}</dd>
                    </dl>
                  </UCard>

                  <UCard>
                    <template #header>
                      <div class="flex items-center gap-2">
                        <UIcon name="i-lucide-network" class="w-4 h-4 text-primary" />
                        <h3 class="font-semibold text-sm">Connect-Standort</h3>
                      </div>
                    </template>
                    <dl class="grid grid-cols-2 gap-2 text-sm">
                      <dt class="text-muted">Standort</dt>
                      <dd>{{ site.name }}</dd>
                      <dt class="text-muted">API-Schlüssel</dt>
                      <dd>{{ site.key ? '••••••••' : '—' }}</dd>
                    </dl>
                  </UCard>

                  <UCard>
                    <template #header>
                      <div class="flex items-center gap-2">
                        <UIcon name="i-lucide-settings" class="w-4 h-4 text-primary" />
                        <h3 class="font-semibold text-sm">Datenübertragung</h3>
                      </div>
                    </template>
                    <div class="flex flex-wrap gap-2">
                      <UBadge v-if="programOptions.sendVehiclePosition" variant="subtle" color="success">Fahrzeugpositionen</UBadge>
                      <UBadge v-if="programOptions.sendVehicleStatus" variant="subtle" color="success">Fahrzeugstatus</UBadge>
                      <UBadge v-if="programOptions.sendUserAvailability" variant="subtle" color="success">Verfügbarkeit</UBadge>
                      <UBadge v-if="programOptions.sendAlarms" variant="subtle" color="success">Alarme</UBadge>
                      <UBadge
                        v-if="!programOptions.sendVehiclePosition && !programOptions.sendVehicleStatus && !programOptions.sendUserAvailability && !programOptions.sendAlarms"
                        variant="subtle"
                        color="warning"
                      >
                        Keine Daten aktiviert
                      </UBadge>
                    </div>
                  </UCard>
                </div>
              </div>
              <template #footer>
                <div class="flex items-center justify-between">
                  <UButton label="Zurück" icon="i-lucide-arrow-left" variant="outline" @click="stepper?.prev()" />
                  <UButton label="Konfiguration speichern" icon="i-lucide-save" :loading="saving" @click="saveAll" />
                </div>
              </template>
            </UCard>

            <!-- Completed -->
            <UCard v-else>
              <div class="text-center py-8 space-y-6">
                <div class="inline-flex p-4 rounded-full bg-green-500/10">
                  <UIcon name="i-lucide-check-circle" class="w-12 h-12 text-green-500" />
                </div>
                <div>
                  <h2 class="text-xl font-bold mb-2">Einrichtung abgeschlossen!</h2>
                  <p class="text-muted max-w-lg mx-auto">
                    Die Grundkonfiguration wurde gespeichert. Du kannst nun weitere
                    Einstellungen vornehmen oder zur Übersicht zurückkehren.
                  </p>
                </div>

                <UCard variant="subtle" class="max-w-lg mx-auto text-left">
                  <div class="flex gap-3">
                    <UIcon name="i-lucide-list-checks" class="w-5 h-5 text-primary shrink-0 mt-0.5" />
                    <div class="text-sm text-muted">
                      <p class="font-medium text-foreground">Nächste Schritte</p>
                      <ul class="mt-2 space-y-1 list-disc list-inside">
                        <li><strong>Muster (Pattern):</strong> Regex-Muster konfigurieren, damit Alarme aus SDS-Nachrichten ausgewertet werden können (Einsatznummer, Stichwort, Adresse, etc.)</li>
                        <li><strong>Subnetz-Adressen:</strong> SNA/GSSI-Zuordnungen für den Standort ergänzen</li>
                        <li><strong>Status-Zuordnungen:</strong> TETRA-Statuscodes den Connect-Status-Werten zuordnen</li>
                      </ul>
                    </div>
                  </div>
                </UCard>

                <div class="flex items-center justify-center gap-3">
                  <UButton
                    label="Muster konfigurieren"
                    icon="i-lucide-regex"
                    to="/settings/pattern"
                  />
                  <UButton
                    label="Zum Dashboard"
                    icon="i-lucide-layout-dashboard"
                    variant="outline"
                    to="/"
                  />
                </div>
              </div>
            </UCard>
          </template>
        </UStepper>

        <!-- Skip link -->
        <div v-if="!completed" class="text-center mt-4">
          <NuxtLink to="/" class="text-sm text-muted hover:underline">
            Einrichtung überspringen →
          </NuxtLink>
        </div>

        <!-- Footer -->
        <div class="text-center mt-8 text-xs text-muted">
          © {{ new Date().getFullYear() }} FeuerSoftware GmbH
        </div>
      </div>
    </UApp>
  </div>
</template>
