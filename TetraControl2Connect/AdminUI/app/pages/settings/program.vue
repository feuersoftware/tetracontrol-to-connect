<script setup lang="ts">
const { t } = useI18n()
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

const booleanFields = computed<{ key: keyof ProgramSettings; label: string; description?: string }[]>(() => [
  { key: 'sendVehicleStatus', label: t('program.fields.sendVehicleStatus.label'), description: t('program.fields.sendVehicleStatus.description') },
  { key: 'sendVehiclePositions', label: t('program.fields.sendVehiclePositions.label'), description: t('program.fields.sendVehiclePositions.description') },
  { key: 'sendUserOperationStatus', label: t('program.fields.sendUserOperationStatus.label'), description: t('program.fields.sendUserOperationStatus.description') },
  { key: 'sendUserAvailability', label: t('program.fields.sendUserAvailability.label'), description: t('program.fields.sendUserAvailability.description') },
  { key: 'sendAlarms', label: t('program.fields.sendAlarms.label'), description: t('program.fields.sendAlarms.description') },
  { key: 'updateExistingOperations', label: t('program.fields.updateExistingOperations.label') },
  { key: 'ignoreStatus5', label: t('program.fields.ignoreStatus', { n: 5 }) },
  { key: 'ignoreStatus0', label: t('program.fields.ignoreStatus', { n: 0 }) },
  { key: 'ignoreStatus9', label: t('program.fields.ignoreStatus', { n: 9 }) },
  { key: 'addPropertyForAlarmTexts', label: t('program.fields.addPropertyForAlarmTexts.label') },
  { key: 'useFullyQualifiedSubnetAddressForConnect', label: t('program.fields.useFullyQualifiedSubnetAddressForConnect.label') },
  { key: 'ignoreAlarmWithoutSubnetAddresses', label: t('program.fields.ignoreAlarmWithoutSubnetAddresses.label') },
  { key: 'acceptCalloutsForSirens', label: t('program.fields.acceptCalloutsForSirens.label') },
  { key: 'acceptSDSAsCalloutsWithPattern', label: t('program.fields.acceptSDSAsCalloutsWithPattern.label') }
])
</script>

<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="text-2xl font-bold">{{ t('nav.program') }}</h1>
        <p class="text-muted mt-1">{{ t('program.subtitle') }}</p>
      </div>
      <UButton
        :label="t('common.save')"
        color="neutral"
        icon="i-lucide-save"
        :loading="saving"
        @click="save"
      />
    </div>

    <UCard class="mb-6" variant="subtle">
      <div class="flex gap-3">
        <UIcon name="i-lucide-info" class="w-5 h-5 text-primary shrink-0 mt-0.5" />
        <div class="text-sm text-muted">
          <p>{{ t('program.info.dataForwarding') }}</p>
          <i18n-t scope="global" keypath="program.info.parameters" tag="p" class="mt-2">
            <template #numbers><strong>{{ t('program.info.numbersLabel') }}</strong></template>
            <template #timespans><strong>{{ t('program.info.timespansLabel') }}</strong></template>
            <template #heartbeat><strong>{{ t('program.info.heartbeatLabel') }}</strong></template>
          </i18n-t>
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
          <h2 class="font-semibold">{{ t('program.sections.dataTransfer') }}</h2>
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
          <h2 class="font-semibold">{{ t('program.sections.numbers') }}</h2>
        </template>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label class="block text-sm font-medium mb-1">{{ t('program.fields.userAvailabilityLifetimeDays') }}</label>
            <UInputNumber v-model="form.userAvailabilityLifetimeDays" :min="1" class="w-full" />
          </div>
          <div>
            <label class="block text-sm font-medium mb-1">{{ t('program.fields.webSocketReconnectTimeoutMinutes') }}</label>
            <UInputNumber v-model="form.webSocketReconnectTimeoutMinutes" :min="1" class="w-full" />
          </div>
          <div>
            <label class="block text-sm font-medium mb-1">{{ t('program.fields.fallbackMaxRetryCount') }}</label>
            <UInputNumber v-model="form.pollForActiveOperationBeforeFallbackMaxRetryCount" :min="0" class="w-full" />
          </div>
        </div>
      </UCard>

      <!-- Zeitspannen und URLs -->
      <UCard>
        <template #header>
          <h2 class="font-semibold">{{ t('program.sections.timespans') }}</h2>
        </template>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label class="block text-sm font-medium mb-1">{{ t('program.fields.fallbackDelay') }}</label>
            <UInput v-model="form.pollForActiveOperationBeforeFallbackDelay" placeholder="00:00:05" />
          </div>
          <div>
            <label class="block text-sm font-medium mb-1">{{ t('program.fields.heartbeatInterval') }}</label>
            <UInput
              :model-value="form.heartbeatInterval ?? undefined"
              :placeholder="t('program.fields.heartbeatIntervalPlaceholder')"
              @update:model-value="form.heartbeatInterval = $event || null"
            />
          </div>
          <div class="md:col-span-2">
            <label class="block text-sm font-medium mb-1">{{ t('program.fields.heartbeatEndpointUrl') }}</label>
            <UInput v-model="form.heartbeatEndpointUrl" placeholder="https://example.com/heartbeat" />
          </div>
        </div>
      </UCard>

      <div class="flex justify-end">
        <UButton
          :label="t('common.save')"
          color="neutral"
          icon="i-lucide-save"
          :loading="saving"
          @click="save"
        />
      </div>
    </div>
  </div>
</template>
