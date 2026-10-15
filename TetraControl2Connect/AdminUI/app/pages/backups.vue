<script setup lang="ts">
const toast = useToast()
const config = useRuntimeConfig()
const apiBase = config.public.apiBase

interface Backup {
  id: number
  createdAt: string
  description: string
}

const backups = ref<Backup[]>([])
const loading = ref(false)
const restoring = ref<number | null>(null)
const creating = ref(false)
const importFile = ref<File | null>(null)
const importing = ref(false)
const importConfirmed = ref(false)

function selectImportFile(event: Event) {
  importFile.value = (event.target as HTMLInputElement).files?.[0] ?? null
  importConfirmed.value = false
}

async function importSettings() {
  if (!importFile.value || !importConfirmed.value) return
  importing.value = true
  try {
    await $fetch(`${apiBase}/settings/import`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: await importFile.value.text()
    })
    importConfirmed.value = false
    toast.add({
      title: 'Einstellungen importiert',
      description: 'Bitte starten Sie die Anwendung neu, damit alle Dienste die importierten Einstellungen verwenden.',
      color: 'success'
    })
    await fetchBackups()
  } catch (e: any) {
    toast.add({
      title: 'Import fehlgeschlagen',
      description: e?.data?.message || 'Bitte prüfen Sie die appsettings.json. Die Einstellungen wurden nicht geändert.',
      color: 'error'
    })
  } finally {
    importing.value = false
  }
}

async function fetchBackups() {
  loading.value = true
  try {
    backups.value = await $fetch<Backup[]>(`${apiBase}/backups`)
  } catch (e: any) {
    toast.add({
      title: 'Fehler beim Laden',
      description: e?.data?.message || e?.message || 'Sicherungen konnten nicht geladen werden.',
      color: 'error'
    })
  } finally {
    loading.value = false
  }
}

async function createBackup() {
  creating.value = true
  try {
    await $fetch(`${apiBase}/backups`, { method: 'POST' })
    toast.add({
      title: 'Sicherung erstellt',
      description: 'Eine manuelle Sicherung wurde erfolgreich erstellt.',
      color: 'success'
    })
    await fetchBackups()
  } catch (e: any) {
    toast.add({
      title: 'Fehler',
      description: e?.data?.message || e?.message || 'Sicherung konnte nicht erstellt werden.',
      color: 'error'
    })
  } finally {
    creating.value = false
  }
}

async function restoreBackup(id: number) {
  restoring.value = id
  try {
    await $fetch(`${apiBase}/backups/${id}/restore`, { method: 'POST' })
    toast.add({
      title: 'Wiederhergestellt',
      description: 'Die Einstellungen wurden erfolgreich wiederhergestellt.',
      color: 'success'
    })
  } catch (e: any) {
    toast.add({
      title: 'Fehler',
      description: e?.data?.message || e?.message || 'Wiederherstellung fehlgeschlagen.',
      color: 'error'
    })
  } finally {
    restoring.value = null
  }
}

async function deleteBackup(id: number) {
  try {
    await $fetch(`${apiBase}/backups/${id}`, { method: 'DELETE' })
    toast.add({
      title: 'Gelöscht',
      description: 'Die Sicherung wurde gelöscht.',
      color: 'success'
    })
    await fetchBackups()
  } catch (e: any) {
    toast.add({
      title: 'Fehler',
      description: e?.data?.message || e?.message || 'Sicherung konnte nicht gelöscht werden.',
      color: 'error'
    })
  }
}

function formatDate(dateStr: string) {
  const date = new Date(dateStr)
  return date.toLocaleDateString('de-DE', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

onMounted(fetchBackups)
</script>

<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="text-2xl font-bold">Sicherungen</h1>
        <p class="text-muted mt-1">Wiederherstellungspunkte der Einstellungen</p>
      </div>
      <UButton
        label="Sicherung erstellen"
        icon="i-lucide-plus"
        :loading="creating"
        @click="createBackup"
      />
    </div>

    <UCard class="mb-6" variant="subtle">
      <div class="flex gap-3">
        <UIcon name="i-lucide-info" class="w-5 h-5 text-primary shrink-0 mt-0.5" />
        <div class="text-sm text-muted">
          <p>Bei jeder Änderung an den Einstellungen wird automatisch eine tägliche Sicherung erstellt (eine pro Tag). Zusätzlich können Sie jederzeit manuell eine Sicherung anlegen.</p>
          <p class="mt-2">Über <strong>Wiederherstellen</strong> können Sie alle Einstellungen auf den Stand eines früheren Zeitpunkts zurücksetzen. Die aktuelle Konfiguration wird dabei vollständig durch die gesicherten Werte ersetzt.</p>
        </div>
      </div>
    </UCard>

    <UCard class="mb-6">
      <h2 class="font-semibold mb-2">appsettings.json importieren</h2>
      <p class="text-sm text-muted mb-4">
        Wählen Sie Ihre bisherige appsettings.json aus. Enthaltene Einstellungsbereiche werden vollständig ersetzt;
        nicht enthaltene Bereiche bleiben erhalten. Vor dem Import wird automatisch eine Sicherung erstellt.
        Anschließend ist ein Neustart der Anwendung erforderlich.
      </p>
      <input
        type="file"
        accept=".json,application/json"
        aria-label="appsettings.json auswählen"
        :disabled="importing"
        @change="selectImportFile"
      >
      <UCheckbox
        v-model="importConfirmed"
        label="Enthaltene Einstellungsbereiche mit den Werten aus der Datei ersetzen"
        :disabled="!importFile || importing"
        class="my-4"
      />
      <UButton
        label="Datei importieren"
        icon="i-lucide-upload"
        :disabled="!importFile || !importConfirmed"
        :loading="importing"
        @click="importSettings"
      />
    </UCard>

    <div v-if="loading" class="flex items-center justify-center py-20">
      <UIcon name="i-lucide-loader-2" class="w-8 h-8 animate-spin text-primary" />
    </div>

    <div v-else-if="backups.length === 0" class="text-center py-20">
      <UIcon name="i-lucide-archive" class="w-12 h-12 text-muted mx-auto mb-4" />
      <p class="text-lg font-medium text-muted">Keine Sicherungen vorhanden</p>
      <p class="text-sm text-muted mt-1">Sicherungen werden automatisch erstellt, sobald Sie Einstellungen ändern.</p>
    </div>

    <div v-else class="space-y-3">
      <UCard v-for="backup in backups" :key="backup.id">
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-3">
            <UIcon
              :name="backup.description === 'Automatische Sicherung' ? 'i-lucide-clock' : 'i-lucide-save'"
              class="w-5 h-5 text-muted shrink-0"
            />
            <div>
              <p class="font-medium text-sm">{{ backup.description }}</p>
              <p class="text-xs text-muted">{{ formatDate(backup.createdAt) }}</p>
            </div>
          </div>
          <div class="flex items-center gap-2">
            <UButton
              label="Wiederherstellen"
              icon="i-lucide-undo-2"
              variant="soft"
              size="sm"
              :loading="restoring === backup.id"
              @click="restoreBackup(backup.id)"
            />
            <UButton
              icon="i-lucide-trash-2"
              variant="ghost"
              color="error"
              size="sm"
              @click="deleteBackup(backup.id)"
            />
          </div>
        </div>
      </UCard>
    </div>
  </div>
</template>
