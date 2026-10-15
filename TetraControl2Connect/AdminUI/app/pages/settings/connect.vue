<script setup lang="ts">
const { t } = useI18n()
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
  const site = form.value.sites[siteIndex]
  if (!site) return
  site.subnetAddresses.push({
    id: 0,
    siteId: site.id,
    name: '',
    sna: '',
    alarmDirectly: false,
    gssi: ''
  })
}

function removeSubnetAddress(siteIndex: number, subIndex: number) {
  form.value.sites[siteIndex]?.subnetAddresses.splice(subIndex, 1)
}

function addSiren(siteIndex: number) {
  const site = form.value.sites[siteIndex]
  if (!site) return
  site.sirens.push({
    id: 0,
    siteId: site.id,
    name: '',
    issi: '',
    expectedHeartbeatInterval: null
  })
}

function removeSiren(siteIndex: number, sirenIndex: number) {
  form.value.sites[siteIndex]?.sirens.splice(sirenIndex, 1)
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
        <h1 class="text-2xl font-bold">{{ t('nav.connect') }}</h1>
        <p class="text-muted mt-1">{{ t('connect.subtitle') }}</p>
      </div>
      <div class="flex gap-2">
        <UButton
          :label="t('connect.addSite')"
          icon="i-lucide-plus"
          color="error"
          @click="addSite"
        />
        <UButton
          :label="t('common.save')"
          color="neutral"
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
          <p>{{ t('connect.info.sites') }}</p>
          <i18n-t scope="global" keypath="connect.info.subnets" tag="p" class="mt-2">
            <template #sna><strong>{{ t('connect.info.subnetsLabel') }}</strong></template>
            <template #gssi><strong>GSSI</strong></template>
          </i18n-t>
          <i18n-t scope="global" keypath="connect.info.sirens" tag="p" class="mt-1">
            <template #sirens><strong>{{ t('connect.info.sirensLabel') }}</strong></template>
          </i18n-t>
        </div>
      </div>
    </UCard>

    <div v-if="loading" class="flex items-center justify-center py-20">
      <UIcon name="i-lucide-loader-2" class="w-8 h-8 animate-spin text-primary" />
    </div>

    <div v-else-if="form.sites.length === 0" class="text-center py-16">
      <UIcon name="i-lucide-building-2" class="w-12 h-12 text-muted mx-auto mb-4" />
      <p class="text-muted mb-4">{{ t('connect.noSites') }}</p>
      <UButton :label="t('connect.addFirstSite')" icon="i-lucide-plus" @click="addSite" />
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
              <span class="font-semibold">{{ site.name || t('connect.siteFallbackName', { n: siteIndex + 1 }) }}</span>
              <UBadge variant="subtle" color="neutral" size="sm">
                {{ t('connect.siteSummary', { subnets: site.subnetAddresses.length, sirens: site.sirens.length }) }}
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
              <label class="block text-sm font-medium mb-1">{{ t('common.name') }}</label>
              <UInput v-model="site.name" :placeholder="t('connect.fields.siteName')" />
            </div>
            <div>
              <label class="block text-sm font-medium mb-1">{{ t('connect.fields.apiKey') }}</label>
              <div class="flex gap-2">
                <UInput
                  v-model="site.key"
                  :type="showKeys.has(siteIndex) ? 'text' : 'password'"
                  :placeholder="t('connect.fields.apiKey')"
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
              <h3 class="font-semibold text-sm">{{ t('connect.subnets') }}</h3>
              <UButton
                :label="t('common.add')"
                icon="i-lucide-plus"
                variant="outline"
                size="sm"
                @click="addSubnetAddress(siteIndex)"
              />
            </div>

            <div v-if="site.subnetAddresses.length === 0" class="text-sm text-muted py-4 text-center border border-dashed border-default rounded-lg">
              {{ t('connect.noSubnets') }}
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
                      <label class="block text-xs font-medium mb-1">{{ t('common.name') }}</label>
                      <UInput v-model="subnet.name" :placeholder="t('common.name')" size="sm" />
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
                        <label class="block text-xs font-medium mb-1">{{ t('connect.fields.alarmDirectly') }}</label>
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
              <h3 class="font-semibold text-sm">{{ t('connect.sirens') }}</h3>
              <UButton
                :label="t('common.add')"
                icon="i-lucide-plus"
                variant="outline"
                size="sm"
                @click="addSiren(siteIndex)"
              />
            </div>

            <div v-if="site.sirens.length === 0" class="text-sm text-muted py-4 text-center border border-dashed border-default rounded-lg">
              {{ t('connect.noSirens') }}
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
                      <label class="block text-xs font-medium mb-1">{{ t('common.name') }}</label>
                      <UInput v-model="siren.name" :placeholder="t('connect.fields.sirenName')" size="sm" />
                    </div>
                    <div>
                      <label class="block text-xs font-medium mb-1">ISSI</label>
                      <UInput v-model="siren.issi" placeholder="ISSI" size="sm" />
                    </div>
                    <div>
                      <label class="block text-xs font-medium mb-1">{{ t('connect.fields.heartbeatInterval') }}</label>
                      <UInput
                        :model-value="siren.expectedHeartbeatInterval ?? undefined"
                        :placeholder="t('connect.fields.heartbeatIntervalPlaceholder')"
                        size="sm"
                        @update:model-value="siren.expectedHeartbeatInterval = $event || null"
                      />
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
