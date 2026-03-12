<script setup lang="ts">
const { fetchAllSettings, loading } = useSettings()

interface SectionInfo {
  slug: string
  label: string
  description: string
  icon: string
  to: string
}

const sections: SectionInfo[] = [
  { slug: 'program', label: 'Programmoptionen', description: 'Allgemeine Programmeinstellungen', icon: 'i-lucide-settings', to: '/settings/program' },
  { slug: 'tetracontrol', label: 'TetraControl', description: 'TetraControl Verbindungseinstellungen', icon: 'i-lucide-radio', to: '/settings/tetracontrol' },
  { slug: 'connect', label: 'Connect / Standorte', description: 'Standorte, Subnetze und Sirenen', icon: 'i-lucide-network', to: '/settings/connect' },
  { slug: 'status', label: 'Status', description: 'Status-Zuordnungen', icon: 'i-lucide-activity', to: '/settings/status' },
  { slug: 'pattern', label: 'Muster (Pattern)', description: 'Regex-Muster für Alarmauswertung', icon: 'i-lucide-regex', to: '/settings/pattern' },
  { slug: 'severity', label: 'Schweregrade', description: 'Schweregrad-Übersetzungen', icon: 'i-lucide-alert-triangle', to: '/settings/severity' },
  { slug: 'siren-callout', label: 'Sirenen-Alarmierung', description: 'Sirenen-Code Übersetzungen', icon: 'i-lucide-siren', to: '/settings/siren-callout' },
  { slug: 'siren-status', label: 'Sirenen-Status', description: 'Sirenen Fehler-Übersetzungen', icon: 'i-lucide-bell', to: '/settings/siren-status' }
]

interface OverviewSection {
  section: string
  exists: boolean
}

const allSettings = ref<OverviewSection[]>([])

onMounted(async () => {
  const data = await fetchAllSettings()
  if (data) allSettings.value = data.sections ?? []
})

function sectionExists(slug: string): boolean {
  return allSettings.value.some(s => s.section === slug && s.exists)
}

const needsSetup = computed(() => {
  if (allSettings.value.length === 0) return false
  return !sectionExists('tetracontrol') || !sectionExists('connect')
})
</script>

<template>
  <div>
    <div class="mb-8">
      <h1 class="text-2xl font-bold">Dashboard</h1>
      <p class="text-muted mt-1">Übersicht aller Einstellungsbereiche</p>
    </div>

    <div v-if="loading" class="flex items-center justify-center py-20">
      <UIcon name="i-lucide-loader-2" class="w-8 h-8 animate-spin text-primary" />
    </div>

    <div v-else>
      <!-- Setup banner -->
      <UCard v-if="needsSetup" class="mb-6 ring-2 ring-primary">
        <div class="flex items-center gap-4">
          <div class="p-3 rounded-full bg-primary/10">
            <UIcon name="i-lucide-rocket" class="w-8 h-8 text-primary" />
          </div>
          <div class="flex-1">
            <h3 class="font-semibold">Ersteinrichtung erforderlich</h3>
            <p class="text-sm text-muted mt-1">
              TetraControl und Connect sind noch nicht konfiguriert. Der Einrichtungsassistent führt Sie in wenigen Schritten durch die Grundkonfiguration.
            </p>
          </div>
          <UButton
            label="Einrichtung starten"
            icon="i-lucide-arrow-right"
            trailing
            to="/setup"
          />
        </div>
      </UCard>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
      <NuxtLink
        v-for="section in sections"
        :key="section.slug"
        :to="section.to"
        class="block"
      >
        <UCard class="hover:ring-2 hover:ring-primary transition-all cursor-pointer h-full">
          <div class="flex items-start gap-4">
            <div class="p-2 rounded-lg bg-primary/10">
              <UIcon :name="section.icon" class="w-6 h-6 text-primary" />
            </div>
            <div class="flex-1 min-w-0">
              <h3 class="font-semibold">{{ section.label }}</h3>
              <p class="text-sm text-muted mt-1">{{ section.description }}</p>
              <UBadge
                v-if="sectionExists(section.slug)"
                variant="subtle"
                color="success"
                class="mt-2"
              >
                Konfiguriert
              </UBadge>
              <UBadge
                v-else-if="allSettings.length > 0"
                variant="subtle"
                color="neutral"
                class="mt-2"
              >
                Nicht konfiguriert
              </UBadge>
            </div>
          </div>
        </UCard>
      </NuxtLink>
      </div>
    </div>
  </div>
</template>
