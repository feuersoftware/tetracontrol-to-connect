<script setup lang="ts">
const { fetchSettings, updateSettings, loading, saving } = useSettings()

interface SirenStatusSettings {
  failureTranslations: Record<string, string>
}

const form = ref<SirenStatusSettings>({
  failureTranslations: {}
})

const newKey = ref('')
const newValue = ref('')

onMounted(async () => {
  const data = await fetchSettings<SirenStatusSettings>('siren-status')
  if (data) form.value = data
})

async function save() {
  await updateSettings('siren-status', form.value)
}

const entries = computed(() => Object.entries(form.value.failureTranslations))

function addEntry() {
  if (!newKey.value.trim()) return
  form.value.failureTranslations[newKey.value.trim()] = newValue.value
  newKey.value = ''
  newValue.value = ''
}

function removeEntry(key: string) {
  delete form.value.failureTranslations[key]
}

function updateKey(oldKey: string, newKeyVal: string) {
  const val = form.value.failureTranslations[oldKey]
  delete form.value.failureTranslations[oldKey]
  form.value.failureTranslations[newKeyVal] = val
}

function updateValue(key: string, val: string) {
  form.value.failureTranslations[key] = val
}
</script>

<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="text-2xl font-bold">Sirenen-Status</h1>
        <p class="text-muted mt-1">Sirenen Fehler-Übersetzungen konfigurieren</p>
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
          <p>Übersetzen Sie hier die Fehler-Codes, die Sirenen bei Störungen melden. Diese Übersetzungen helfen dabei, technische Fehlercodes in verständliche Beschreibungen für Feuer Software Connect umzuwandeln.</p>
          <p class="mt-2">Der <strong>Fehler-Code</strong> ist der numerische Code aus der Sirenen-Statusmeldung, die <strong>Übersetzung</strong> ist die Fehlerbeschreibung, die in Connect als Störungsmeldung angezeigt wird.</p>
        </div>
      </div>
    </UCard>

    <div v-if="loading" class="flex items-center justify-center py-20">
      <UIcon name="i-lucide-loader-2" class="w-8 h-8 animate-spin text-primary" />
    </div>

    <div v-else class="space-y-6">
      <UCard>
        <template #header>
          <h2 class="font-semibold">Fehler-Übersetzungen</h2>
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
              placeholder="Fehler-Code"
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
          <UInput v-model="newKey" placeholder="Neuer Fehler-Code" class="flex-1" size="sm" />
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
