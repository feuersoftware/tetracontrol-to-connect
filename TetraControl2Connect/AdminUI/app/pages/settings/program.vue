<script setup lang="ts">
const { fetchSettings, updateSettings, loading, saving } = useSettings()

interface ProgramSettings {
  id: number
  sendVehicleStatus: boolean
  sendVehiclePositions: boolean
  sendUserOperationStatus: boolean
  sendUserAvailability: boolean
  sendAlarms: boolean
  updateExistingOperations: boolean
  userAvailabilityLifetimeDays: number
  webSocketReconnectTimeoutMinutes: number
  pollForActiveOperationBeforeFallbackMaxRetryCount: number
  pollForActiveOperationBeforeFallbackDelay: string
  heartbeatEndpointUrl: string
  heartbeatInterval: string | null
  ignoreStatus5: boolean
  ignoreStatus0: boolean
  ignoreStatus9: boolean
  addPropertyForAlarmTexts: boolean
  useFullyQualifiedSubnetAddressForConnect: boolean
  ignoreAlarmWithoutSubnetAddresses: boolean
  acceptCalloutsForSirens: boolean
  acceptSDSAsCalloutsWithPattern: boolean
}

const form = ref<ProgramSettings>({
  id: 1,
  sendVehicleStatus: false,
  sendVehiclePositions: false,
  sendUserOperationStatus: false,
  sendUserAvailability: false,
  sendAlarms: false,
  updateExistingOperations: false,
  userAvailabilityLifetimeDays: 30,
  webSocketReconnectTimeoutMinutes: 5,
  pollForActiveOperationBeforeFallbackMaxRetryCount: 3,
  pollForActiveOperationBeforeFallbackDelay: '00:00:05',
  heartbeatEndpointUrl: '',
  heartbeatInterval: null,
  ignoreStatus5: false,
  ignoreStatus0: false,
  ignoreStatus9: false,
  addPropertyForAlarmTexts: false,
  useFullyQualifiedSubnetAddressForConnect: false,
  ignoreAlarmWithoutSubnetAddresses: false,
  acceptCalloutsForSirens: false,
  acceptSDSAsCalloutsWithPattern: false
})

onMounted(async () => {
  const data = await fetchSettings<ProgramSettings>('program')
  if (data) form.value = data
})

async function save() {
  await updateSettings('program', form.value)
}

const booleanFields: { key: keyof ProgramSettings; label: string; description?: string }[] = [
  { key: 'sendVehicleStatus', label: 'Fahrzeugstatus senden', description: 'Fahrzeugstatus an Connect übertragen' },
  { key: 'sendVehiclePositions', label: 'Fahrzeugpositionen senden', description: 'GPS-Positionen an Connect übertragen' },
  { key: 'sendUserOperationStatus', label: 'Einsatzstatus senden', description: 'Benutzer-Einsatzstatus übertragen' },
  { key: 'sendUserAvailability', label: 'Verfügbarkeit senden', description: 'Benutzerverfügbarkeit übertragen' },
  { key: 'sendAlarms', label: 'Alarme senden', description: 'Alarme an Connect übertragen' },
  { key: 'updateExistingOperations', label: 'Bestehende Einsätze aktualisieren' },
  { key: 'ignoreStatus5', label: 'Status 5 ignorieren' },
  { key: 'ignoreStatus0', label: 'Status 0 ignorieren' },
  { key: 'ignoreStatus9', label: 'Status 9 ignorieren' },
  { key: 'addPropertyForAlarmTexts', label: 'Eigenschaft für Alarmtexte hinzufügen' },
  { key: 'useFullyQualifiedSubnetAddressForConnect', label: 'Vollqualifizierte Subnetzadresse verwenden' },
  { key: 'ignoreAlarmWithoutSubnetAddresses', label: 'Alarme ohne Subnetzadressen ignorieren' },
  { key: 'acceptCalloutsForSirens', label: 'Sirenen-Alarmierungen akzeptieren' },
  { key: 'acceptSDSAsCalloutsWithPattern', label: 'SDS als Alarmierung mit Muster akzeptieren' }
]
</script>

<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="text-2xl font-bold">Programmoptionen</h1>
        <p class="text-muted mt-1">Allgemeine Programmeinstellungen</p>
      </div>
      <UButton
        label="Speichern"
        icon="i-lucide-save"
        :loading="saving"
        @click="save"
      />
    </div>

    <UCard class="mb-6" variant="subtle">
      <div class="flex gap-3">
        <UIcon name="i-lucide-info" class="w-5 h-5 text-primary shrink-0 mt-0.5" />
        <div class="text-sm text-muted">
          <p>Hier legen Sie fest, welche Daten von TetraControl an Feuer Software Connect weitergeleitet werden. Sie können einzelne Funktionen wie Fahrzeugstatus, Positionsübertragung, Alarme oder Benutzerverfügbarkeit unabhängig voneinander aktivieren oder deaktivieren.</p>
          <p class="mt-2">Unter <strong>Zahlenwerte</strong> und <strong>Zeitspannen</strong> können Sie Timeout- und Wiederholungsparameter anpassen. Der <strong>Heartbeat</strong> sendet regelmäßig ein Signal an eine konfigurierbare URL, um die Erreichbarkeit des Systems zu überwachen.</p>
        </div>
      </div>
    </UCard>

    <div v-if="loading" class="flex items-center justify-center py-20">
      <UIcon name="i-lucide-loader-2" class="w-8 h-8 animate-spin text-primary" />
    </div>

    <div v-else class="space-y-6">
      <!-- Datenübertragung -->
      <UCard>
        <template #header>
          <h2 class="font-semibold">Datenübertragung & Verhalten</h2>
        </template>
        <div class="space-y-4">
          <div v-for="field in booleanFields" :key="field.key" class="flex items-center justify-between">
            <div>
              <p class="font-medium text-sm">{{ field.label }}</p>
              <p v-if="field.description" class="text-xs text-muted">{{ field.description }}</p>
            </div>
            <USwitch v-model="(form[field.key] as boolean)" />
          </div>
        </div>
      </UCard>

      <!-- Zahlenwerte -->
      <UCard>
        <template #header>
          <h2 class="font-semibold">Zahlenwerte</h2>
        </template>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label class="block text-sm font-medium mb-1">Verfügbarkeit Lebensdauer (Tage)</label>
            <UInputNumber v-model="form.userAvailabilityLifetimeDays" :min="1" class="w-full" />
          </div>
          <div>
            <label class="block text-sm font-medium mb-1">WebSocket Reconnect Timeout (Min.)</label>
            <UInputNumber v-model="form.webSocketReconnectTimeoutMinutes" :min="1" class="w-full" />
          </div>
          <div>
            <label class="block text-sm font-medium mb-1">Max. Wiederholungen vor Fallback</label>
            <UInputNumber v-model="form.pollForActiveOperationBeforeFallbackMaxRetryCount" :min="0" class="w-full" />
          </div>
        </div>
      </UCard>

      <!-- Zeitspannen und URLs -->
      <UCard>
        <template #header>
          <h2 class="font-semibold">Zeitspannen & URLs</h2>
        </template>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label class="block text-sm font-medium mb-1">Fallback-Verzögerung (HH:MM:SS)</label>
            <UInput v-model="form.pollForActiveOperationBeforeFallbackDelay" placeholder="00:00:05" />
          </div>
          <div>
            <label class="block text-sm font-medium mb-1">Heartbeat-Intervall (HH:MM:SS)</label>
            <UInput v-model="form.heartbeatInterval" placeholder="00:05:00 (leer = deaktiviert)" />
          </div>
          <div class="md:col-span-2">
            <label class="block text-sm font-medium mb-1">Heartbeat Endpoint URL</label>
            <UInput v-model="form.heartbeatEndpointUrl" placeholder="https://example.com/heartbeat" />
          </div>
        </div>
      </UCard>

      <div class="flex justify-end">
        <UButton
          label="Speichern"
          icon="i-lucide-save"
          :loading="saving"
          @click="save"
        />
      </div>
    </div>
  </div>
</template>
