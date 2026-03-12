<script setup lang="ts">
const { fetchSettings, updateSettings, loading, saving } = useSettings()

interface SirenCalloutSettings {
  useSirenCodeTranslationAsKeyword: boolean
  sirenCodeTranslations: Record<string, string>
}

const form = ref<SirenCalloutSettings>({
  useSirenCodeTranslationAsKeyword: false,
  sirenCodeTranslations: {}
})

const newKey = ref('')
const newValue = ref('')

onMounted(async () => {
  const data = await fetchSettings<SirenCalloutSettings>('siren-callout')
  if (data) form.value = data
})

async function save() {
  await updateSettings('siren-callout', form.value)
}

const entries = computed(() => Object.entries(form.value.sirenCodeTranslations))

function addEntry() {
  if (!newKey.value.trim()) return
  form.value.sirenCodeTranslations[newKey.value.trim()] = newValue.value
  newKey.value = ''
  newValue.value = ''
}

function removeEntry(key: string) {
  delete form.value.sirenCodeTranslations[key]
}

function updateKey(oldKey: string, newKeyVal: string) {
  const val = form.value.sirenCodeTranslations[oldKey]
  delete form.value.sirenCodeTranslations[oldKey]
  form.value.sirenCodeTranslations[newKeyVal] = val
}

function updateValue(key: string, val: string) {
  form.value.sirenCodeTranslations[key] = val
}
</script>

<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="text-2xl font-bold">Sirenen-Alarmierung</h1>
        <p class="text-muted mt-1">Sirenen-Code Übersetzungen konfigurieren</p>
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
          <p>Konfigurieren Sie hier die Übersetzung von Sirenen-Alarmierungscodes. Wenn eine Sirene einen Alarmierungscode empfängt, wird dieser anhand der konfigurierten Übersetzungen in einen verständlichen Text umgewandelt.</p>
          <p class="mt-2">Der <strong>Sirenen-Code</strong> ist der numerische Code aus der TETRA-Nachricht, die <strong>Übersetzung</strong> ist der Text, der in Connect angezeigt wird. Optional kann die Übersetzung als Einsatz-Stichwort verwendet werden.</p>
        </div>
      </div>
    </UCard>

    <div v-if="loading" class="flex items-center justify-center py-20">
      <UIcon name="i-lucide-loader-2" class="w-8 h-8 animate-spin text-primary" />
    </div>

    <div v-else class="space-y-6">
      <UCard>
        <template #header>
          <h2 class="font-semibold">Optionen</h2>
        </template>
        <div class="flex items-center justify-between">
          <div>
            <p class="font-medium text-sm">Sirenen-Code Übersetzung als Stichwort verwenden</p>
            <p class="text-xs text-muted">Übersetzte Sirenen-Codes werden als Einsatz-Stichwort genutzt</p>
          </div>
          <USwitch v-model="form.useSirenCodeTranslationAsKeyword" />
        </div>
      </UCard>

      <UCard>
        <template #header>
          <h2 class="font-semibold">Sirenen-Code Übersetzungen</h2>
        </template>

        <div v-if="entries.length === 0" class="text-sm text-muted py-4 text-center">
          Keine Übersetzungen konfiguriert
        </div>

        <div v-else class="space-y-2 mb-4">
          <div
            v-for="[key, value] in entries"
            :key="key"
            class="flex items-center gap-2"
          >
            <UInput
              :model-value="key"
              placeholder="Sirenen-Code"
              class="flex-1"
              size="sm"
              @update:model-value="(v: string) => updateKey(key, v)"
            />
            <UIcon name="i-lucide-arrow-right" class="w-4 h-4 text-muted shrink-0" />
            <UInput
              :model-value="value"
              placeholder="Übersetzung"
              class="flex-1"
              size="sm"
              @update:model-value="(v: string) => updateValue(key, v)"
            />
            <UButton
              icon="i-lucide-x"
              variant="ghost"
              color="error"
              size="xs"
              @click="removeEntry(key)"
            />
          </div>
        </div>

        <USeparator v-if="entries.length > 0" class="my-4" />

        <div class="flex items-center gap-2">
          <UInput v-model="newKey" placeholder="Neuer Code" class="flex-1" size="sm" />
          <UIcon name="i-lucide-arrow-right" class="w-4 h-4 text-muted shrink-0" />
          <UInput v-model="newValue" placeholder="Übersetzung" class="flex-1" size="sm" />
          <UButton
            icon="i-lucide-plus"
            variant="outline"
            size="sm"
            :disabled="!newKey.trim()"
            @click="addEntry"
          />
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
