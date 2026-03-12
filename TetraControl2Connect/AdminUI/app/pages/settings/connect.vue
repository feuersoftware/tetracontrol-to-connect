<script setup lang="ts">
const { fetchSettings, updateSettings, loading, saving } = useSettings()

interface SubnetAddress {
  id: number
  siteId: number
  name: string
  sna: string
  alarmDirectly: boolean
  gssi: string
}

interface Siren {
  id: number
  siteId: number
  name: string
  issi: string
  expectedHeartbeatInterval: string | null
}

interface Site {
  id: number
  name: string
  key: string
  subnetAddresses: SubnetAddress[]
  sirens: Siren[]
}

interface ConnectSettings {
  sites: Site[]
}

const form = ref<ConnectSettings>({
  sites: []
})

const expandedSites = ref<Set<number>>(new Set())

onMounted(async () => {
  const data = await fetchSettings<ConnectSettings>('connect')
  if (data) form.value = data
})

async function save() {
  await updateSettings('connect', form.value)
}

function addSite() {
  form.value.sites.push({
    id: 0,
    name: '',
    key: '',
    subnetAddresses: [],
    sirens: []
  })
  expandedSites.value.add(form.value.sites.length - 1)
}

function removeSite(index: number) {
  form.value.sites.splice(index, 1)
  expandedSites.value.delete(index)
}

function addSubnetAddress(siteIndex: number) {
  form.value.sites[siteIndex].subnetAddresses.push({
    id: 0,
    siteId: form.value.sites[siteIndex].id,
    name: '',
    sna: '',
    alarmDirectly: false,
    gssi: ''
  })
}

function removeSubnetAddress(siteIndex: number, subIndex: number) {
  form.value.sites[siteIndex].subnetAddresses.splice(subIndex, 1)
}

function addSiren(siteIndex: number) {
  form.value.sites[siteIndex].sirens.push({
    id: 0,
    siteId: form.value.sites[siteIndex].id,
    name: '',
    issi: '',
    expectedHeartbeatInterval: null
  })
}

function removeSiren(siteIndex: number, sirenIndex: number) {
  form.value.sites[siteIndex].sirens.splice(sirenIndex, 1)
}

function toggleSite(index: number) {
  if (expandedSites.value.has(index)) {
    expandedSites.value.delete(index)
  } else {
    expandedSites.value.add(index)
  }
}

const showKeys = ref<Set<number>>(new Set())

function toggleKeyVisibility(index: number) {
  if (showKeys.value.has(index)) {
    showKeys.value.delete(index)
  } else {
    showKeys.value.add(index)
  }
}
</script>

<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="text-2xl font-bold">Connect / Standorte</h1>
        <p class="text-muted mt-1">Standorte, Subnetzadressen und Sirenen verwalten</p>
      </div>
      <div class="flex gap-2">
        <UButton
          label="Standort hinzufügen"
          icon="i-lucide-plus"
          variant="outline"
          @click="addSite"
        />
        <UButton
          label="Speichern"
          icon="i-lucide-save"
          :loading="saving"
          @click="save"
        />
      </div>
    </div>

    <UCard class="mb-6" variant="subtle">
      <div class="flex gap-3">
        <UIcon name="i-lucide-info" class="w-5 h-5 text-primary shrink-0 mt-0.5" />
        <div class="text-sm text-muted">
          <p>Verwalten Sie hier Ihre Feuer Software Connect Standorte. Jeder Standort repräsentiert eine Connect-Instanz mit eigenem API-Key.</p>
          <p class="mt-2"><strong>Subnetzadressen (SNA)</strong> ordnen TETRA-Subnetze einem Standort zu. Wenn „Direkt alarmieren" aktiviert ist, werden Alarme sofort an diesen Standort weitergeleitet. Die <strong>GSSI</strong> ist die Gruppenadresse des Subnetzes.</p>
          <p class="mt-1"><strong>Sirenen</strong> werden über ihre ISSI identifiziert. Optional kann ein Heartbeat-Intervall konfiguriert werden, um die Erreichbarkeit der Sirene zu überwachen.</p>
        </div>
      </div>
    </UCard>

    <div v-if="loading" class="flex items-center justify-center py-20">
      <UIcon name="i-lucide-loader-2" class="w-8 h-8 animate-spin text-primary" />
    </div>

    <div v-else-if="form.sites.length === 0" class="text-center py-16">
      <UIcon name="i-lucide-building-2" class="w-12 h-12 text-muted mx-auto mb-4" />
      <p class="text-muted mb-4">Keine Standorte konfiguriert</p>
      <UButton label="Ersten Standort hinzufügen" icon="i-lucide-plus" @click="addSite" />
    </div>

    <div v-else class="space-y-4">
      <UCard v-for="(site, siteIndex) in form.sites" :key="siteIndex">
        <template #header>
          <div class="flex items-center justify-between">
            <button class="flex items-center gap-2 text-left flex-1" @click="toggleSite(siteIndex)">
              <UIcon
                :name="expandedSites.has(siteIndex) ? 'i-lucide-chevron-down' : 'i-lucide-chevron-right'"
                class="w-4 h-4"
              />
              <span class="font-semibold">{{ site.name || `Standort ${siteIndex + 1}` }}</span>
              <UBadge variant="subtle" color="neutral" size="sm">
                {{ site.subnetAddresses.length }} Subnetze, {{ site.sirens.length }} Sirenen
              </UBadge>
            </button>
            <UButton
              icon="i-lucide-trash-2"
              variant="ghost"
              color="error"
              size="sm"
              @click="removeSite(siteIndex)"
            />
          </div>
        </template>

        <div v-if="expandedSites.has(siteIndex)" class="space-y-6">
          <!-- Site basics -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium mb-1">Name</label>
              <UInput v-model="site.name" placeholder="Standortname" />
            </div>
            <div>
              <label class="block text-sm font-medium mb-1">API-Key</label>
              <div class="flex gap-2">
                <UInput
                  v-model="site.key"
                  :type="showKeys.has(siteIndex) ? 'text' : 'password'"
                  placeholder="API-Key"
                  class="flex-1"
                />
                <UButton
                  :icon="showKeys.has(siteIndex) ? 'i-lucide-eye-off' : 'i-lucide-eye'"
                  variant="outline"
                  @click="toggleKeyVisibility(siteIndex)"
                />
              </div>
            </div>
          </div>

          <USeparator />

          <!-- Subnet Addresses -->
          <div>
            <div class="flex items-center justify-between mb-3">
              <h3 class="font-semibold text-sm">Subnetzadressen</h3>
              <UButton
                label="Hinzufügen"
                icon="i-lucide-plus"
                variant="outline"
                size="sm"
                @click="addSubnetAddress(siteIndex)"
              />
            </div>

            <div v-if="site.subnetAddresses.length === 0" class="text-sm text-muted py-4 text-center border border-dashed border-default rounded-lg">
              Keine Subnetzadressen konfiguriert
            </div>

            <div v-else class="space-y-3">
              <div
                v-for="(subnet, subIndex) in site.subnetAddresses"
                :key="subIndex"
                class="border border-default rounded-lg p-3"
              >
                <div class="flex items-start gap-2">
                  <div class="flex-1 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
                    <div>
                      <label class="block text-xs font-medium mb-1">Name</label>
                      <UInput v-model="subnet.name" placeholder="Name" size="sm" />
                    </div>
                    <div>
                      <label class="block text-xs font-medium mb-1">SNA</label>
                      <UInput v-model="subnet.sna" placeholder="SNA" size="sm" />
                    </div>
                    <div>
                      <label class="block text-xs font-medium mb-1">GSSI</label>
                      <UInput v-model="subnet.gssi" placeholder="GSSI" size="sm" />
                    </div>
                    <div class="flex items-end gap-2">
                      <div class="flex-1">
                        <label class="block text-xs font-medium mb-1">Direkt alarmieren</label>
                        <USwitch v-model="subnet.alarmDirectly" />
                      </div>
                    </div>
                  </div>
                  <UButton
                    icon="i-lucide-x"
                    variant="ghost"
                    color="error"
                    size="xs"
                    class="mt-5"
                    @click="removeSubnetAddress(siteIndex, subIndex)"
                  />
                </div>
              </div>
            </div>
          </div>

          <USeparator />

          <!-- Sirens -->
          <div>
            <div class="flex items-center justify-between mb-3">
              <h3 class="font-semibold text-sm">Sirenen</h3>
              <UButton
                label="Hinzufügen"
                icon="i-lucide-plus"
                variant="outline"
                size="sm"
                @click="addSiren(siteIndex)"
              />
            </div>

            <div v-if="site.sirens.length === 0" class="text-sm text-muted py-4 text-center border border-dashed border-default rounded-lg">
              Keine Sirenen konfiguriert
            </div>

            <div v-else class="space-y-3">
              <div
                v-for="(siren, sirenIndex) in site.sirens"
                :key="sirenIndex"
                class="border border-default rounded-lg p-3"
              >
                <div class="flex items-start gap-2">
                  <div class="flex-1 grid grid-cols-1 sm:grid-cols-3 gap-3">
                    <div>
                      <label class="block text-xs font-medium mb-1">Name</label>
                      <UInput v-model="siren.name" placeholder="Sirenenname" size="sm" />
                    </div>
                    <div>
                      <label class="block text-xs font-medium mb-1">ISSI</label>
                      <UInput v-model="siren.issi" placeholder="ISSI" size="sm" />
                    </div>
                    <div>
                      <label class="block text-xs font-medium mb-1">Heartbeat-Intervall (HH:MM:SS)</label>
                      <UInput v-model="siren.expectedHeartbeatInterval" placeholder="Leer = deaktiviert" size="sm" />
                    </div>
                  </div>
                  <UButton
                    icon="i-lucide-x"
                    variant="ghost"
                    color="error"
                    size="xs"
                    class="mt-5"
                    @click="removeSiren(siteIndex, sirenIndex)"
                  />
                </div>
              </div>
            </div>
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
