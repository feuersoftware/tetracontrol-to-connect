<script setup lang="ts">
const { fetchSettings, updateSettings, loading, saving } = useSettings()

interface StatusSettings {
  id: number
  availableStatus: string
  limitedAvailableStatus: string
  notAvailableStatus: string
  comingStatus: string
  notComingStatus: string
  comingLaterStatus: string
}

const form = ref<StatusSettings>({
  id: 1,
  availableStatus: '',
  limitedAvailableStatus: '',
  notAvailableStatus: '',
  comingStatus: '',
  notComingStatus: '',
  comingLaterStatus: ''
})

onMounted(async () => {
  const data = await fetchSettings<StatusSettings>('status')
  if (data) form.value = data
})

async function save() {
  await updateSettings('status', form.value)
}

const fields: { key: keyof StatusSettings; label: string; description: string }[] = [
  { key: 'availableStatus', label: 'Verfügbar', description: 'Status-Wert für verfügbare Einsatzkräfte' },
  { key: 'limitedAvailableStatus', label: 'Eingeschränkt verfügbar', description: 'Status-Wert für eingeschränkt verfügbare Einsatzkräfte' },
  { key: 'notAvailableStatus', label: 'Nicht verfügbar', description: 'Status-Wert für nicht verfügbare Einsatzkräfte' },
  { key: 'comingStatus', label: 'Kommt', description: 'Status-Wert für zugesagte Einsatzkräfte' },
  { key: 'notComingStatus', label: 'Kommt nicht', description: 'Status-Wert für absagende Einsatzkräfte' },
  { key: 'comingLaterStatus', label: 'Kommt später', description: 'Status-Wert für verzögert zugesagte Einsatzkräfte' }
]
</script>

<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="text-2xl font-bold">Status</h1>
        <p class="text-muted mt-1">Status-Zuordnungen für Einsatzkräfte</p>
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
          <p>Definieren Sie hier die TETRA-Statuswerte, die den verschiedenen Verfügbarkeits-Zuständen in Feuer Software Connect entsprechen.</p>
          <p class="mt-2">Wenn ein Benutzer seinen Status am TETRA-Funkgerät ändert, wird der numerische Statuswert mit dieser Tabelle abgeglichen und der entsprechende Verfügbarkeitsstatus an Connect übertragen. Tragen Sie die Statusnummern als kommagetrennte Werte ein (z.B. „1,2").</p>
        </div>
      </div>
    </UCard>

    <div v-if="loading" class="flex items-center justify-center py-20">
      <UIcon name="i-lucide-loader-2" class="w-8 h-8 animate-spin text-primary" />
    </div>

    <div v-else>
      <UCard>
        <template #header>
          <h2 class="font-semibold">Status-Zuordnungen</h2>
        </template>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div v-for="field in fields" :key="field.key">
            <label class="block text-sm font-medium mb-1">{{ field.label }}</label>
            <p class="text-xs text-muted mb-1">{{ field.description }}</p>
            <UInput v-model="form[field.key]" :placeholder="field.label" />
          </div>
        </div>
      </UCard>

      <div class="flex justify-end mt-6">
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
