<script setup lang="ts">
const { fetchSettings, updateSettings, loading, saving } = useSettings()

interface TetraControlSettings {
  id: number
  tetraControlHost: string
  tetraControlPort: number
  tetraControlUsername: string
  tetraControlPassword: string
}

const form = ref<TetraControlSettings>({
  id: 1,
  tetraControlHost: '',
  tetraControlPort: 8080,
  tetraControlUsername: '',
  tetraControlPassword: ''
})

const showPassword = ref(false)

onMounted(async () => {
  const data = await fetchSettings<TetraControlSettings>('tetracontrol')
  if (data) form.value = data
})

async function save() {
  await updateSettings('tetracontrol', form.value)
}
</script>

<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="text-2xl font-bold">TetraControl</h1>
        <p class="text-muted mt-1">Verbindungseinstellungen zum TetraControl-Server</p>
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
          <p>Konfiguriere hier die Verbindung zum TetraControl-Server. TetraControl2Connect verbindet sich per WebSocket mit TetraControl und empfängt in Echtzeit Status-, Positions- und SDS-Nachrichten.</p>
          <p class="mt-2">Gib die IP-Adresse oder den Hostnamen des TetraControl-Servers sowie den WebSocket-Port an.</p>
        </div>
      </div>
    </UCard>

    <div v-if="loading" class="flex items-center justify-center py-20">
      <UIcon name="i-lucide-loader-2" class="w-8 h-8 animate-spin text-primary" />
    </div>

    <div v-else>
      <UCard>
        <template #header>
          <h2 class="font-semibold">Verbindung</h2>
        </template>
        <div class="space-y-4">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <UFormField label="Host">
              <UInput v-model="form.tetraControlHost" placeholder="z.B. 192.168.1.100" class="w-full" />
            </UFormField>
            <UFormField label="Port">
              <UInput v-model.number="form.tetraControlPort" type="number" min="1" max="65535" class="w-full" />
            </UFormField>
          </div>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <UFormField label="Benutzername">
              <UInput v-model="form.tetraControlUsername" placeholder="Benutzername" class="w-full" />
            </UFormField>
            <UFormField label="Passwort">
              <div class="flex gap-2 w-full">
                <UInput
                  v-model="form.tetraControlPassword"
                  :type="showPassword ? 'text' : 'password'"
                  placeholder="Passwort"
                  class="flex-1"
                />
                <UButton
                  :icon="showPassword ? 'i-lucide-eye-off' : 'i-lucide-eye'"
                  variant="outline"
                  @click="showPassword = !showPassword"
                />
              </div>
            </UFormField>
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
