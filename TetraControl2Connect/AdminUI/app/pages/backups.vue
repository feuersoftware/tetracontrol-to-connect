<script setup lang="ts">
const toast = useToast()
const { t, locale } = useI18n()
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

// Error codes returned by POST /settings/import; the backend message is English and not shown.
const importErrorKeys: Record<string, string> = {
  invalid_json: 'backups.import.errors.invalidJson',
  invalid_settings: 'backups.import.errors.invalidSettings'
}

async function importSettings() {
  if (!importFile.value || !importConfirmed.value) return
  const content = await importFile.value.text()
  if (!content.trim()) {
    toast.add({
      title: t('backups.import.errorTitle'),
      description: t('backups.import.errors.emptyFile'),
      color: 'error'
    })
    return
  }
  importing.value = true
  try {
    await $fetch(`${apiBase}/settings/import`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: content
    })
    importConfirmed.value = false
    toast.add({
      title: t('backups.import.successTitle'),
      description: t('backups.import.success'),
      color: 'success'
    })
    await fetchBackups()
  } catch (e: any) {
    toast.add({
      title: t('backups.import.errorTitle'),
      description: t(importErrorKeys[e?.data?.error] ?? 'backups.import.errors.generic'),
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
  } catch {
    toast.add({
      title: t('backups.loadErrorTitle'),
      description: t('backups.loadError'),
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
      title: t('backups.createdTitle'),
      description: t('backups.created'),
      color: 'success'
    })
    await fetchBackups()
  } catch {
    toast.add({
      title: t('common.error'),
      description: t('backups.createError'),
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
      title: t('backups.restoredTitle'),
      description: t('backups.restored'),
      color: 'success'
    })
  } catch {
    toast.add({
      title: t('common.error'),
      description: t('backups.restoreError'),
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
      title: t('backups.deletedTitle'),
      description: t('backups.deleted'),
      color: 'success'
    })
    await fetchBackups()
  } catch {
    toast.add({
      title: t('common.error'),
      description: t('backups.deleteError'),
      color: 'error'
    })
  }
}

function formatDate(dateStr: string) {
  const date = new Date(dateStr)
  return date.toLocaleDateString(locale.value, {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

// Backup descriptions are language-neutral codes from the backend; translate them for display.
const backendDescriptionKeys: Record<string, string> = {
  automatic: 'backups.descriptions.automatic',
  automatic_before_import: 'backups.descriptions.automaticBeforeImport',
  manual: 'backups.descriptions.manual'
}

function displayDescription(description: string) {
  const key = backendDescriptionKeys[description]
  return key ? t(key) : description
}

onMounted(fetchBackups)
</script>

<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="text-2xl font-bold">{{ t('nav.backups') }}</h1>
        <p class="text-muted mt-1">{{ t('backups.subtitle') }}</p>
      </div>
      <UButton
        :label="t('backups.create')"
        color="neutral"
        icon="i-lucide-plus"
        :loading="creating"
        @click="createBackup"
      />
    </div>

    <UCard class="mb-6" variant="subtle">
      <div class="flex gap-3">
        <UIcon name="i-lucide-info" class="w-5 h-5 text-primary shrink-0 mt-0.5" />
        <div class="text-sm text-muted">
          <p>{{ t('backups.info.automatic') }}</p>
          <i18n-t scope="global" keypath="backups.info.restore" tag="p" class="mt-2">
            <template #action>
              <strong>{{ t('backups.restore') }}</strong>
            </template>
          </i18n-t>
        </div>
      </div>
    </UCard>

    <UCard class="mb-6">
      <h2 class="font-semibold mb-2">{{ t('backups.import.title') }}</h2>
      <p class="text-sm text-muted mb-4">
        {{ t('backups.import.description') }}
      </p>
      <input
        type="file"
        accept=".json,application/json"
        :aria-label="t('backups.import.selectFile')"
        :disabled="importing"
        @change="selectImportFile"
      >
      <UCheckbox
        v-model="importConfirmed"
        :label="t('backups.import.confirm')"
        :disabled="!importFile || importing"
        class="my-4"
      />
      <UButton
        :label="t('backups.import.submit')"
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
      <p class="text-lg font-medium text-muted">{{ t('backups.empty.title') }}</p>
      <p class="text-sm text-muted mt-1">{{ t('backups.empty.description') }}</p>
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
              <p class="font-medium text-sm">{{ displayDescription(backup.description) }}</p>
              <p class="text-xs text-muted">{{ formatDate(backup.createdAt) }}</p>
            </div>
          </div>
          <div class="flex items-center gap-2">
            <UButton
              :label="t('backups.restore')"
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
              :aria-label="t('common.delete')"
              @click="deleteBackup(backup.id)"
            />
          </div>
        </div>
      </UCard>
    </div>
  </div>
</template>
