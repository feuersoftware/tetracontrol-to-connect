<script setup lang="ts">
const { t } = useI18n()
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

const fields = computed<{ key: Exclude<keyof StatusSettings, 'id'>; label: string; description: string }[]>(() => [
  { key: 'availableStatus', label: t('status.fields.availableStatus.label'), description: t('status.fields.availableStatus.description') },
  { key: 'limitedAvailableStatus', label: t('status.fields.limitedAvailableStatus.label'), description: t('status.fields.limitedAvailableStatus.description') },
  { key: 'notAvailableStatus', label: t('status.fields.notAvailableStatus.label'), description: t('status.fields.notAvailableStatus.description') },
  { key: 'comingStatus', label: t('status.fields.comingStatus.label'), description: t('status.fields.comingStatus.description') },
  { key: 'notComingStatus', label: t('status.fields.notComingStatus.label'), description: t('status.fields.notComingStatus.description') },
  { key: 'comingLaterStatus', label: t('status.fields.comingLaterStatus.label'), description: t('status.fields.comingLaterStatus.description') }
])
</script>

<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="text-2xl font-bold">{{ t('nav.status') }}</h1>
        <p class="text-muted mt-1">{{ t('status.subtitle') }}</p>
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
          <p>{{ t('status.info.definition') }}</p>
          <p class="mt-2">{{ t('status.info.matching') }}</p>
        </div>
      </div>
    </UCard>

    <div v-if="loading" class="flex items-center justify-center py-20">
      <UIcon name="i-lucide-loader-2" class="w-8 h-8 animate-spin text-primary" />
    </div>

    <div v-else>
      <UCard>
        <template #header>
          <h2 class="font-semibold">{{ t('status.mappings') }}</h2>
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
