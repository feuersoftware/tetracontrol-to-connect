<script setup lang="ts">
const { t } = useI18n()
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
        <h1 class="text-2xl font-bold">{{ t('nav.tetracontrol') }}</h1>
        <p class="text-muted mt-1">{{ t('tetracontrol.subtitle') }}</p>
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
          <p>{{ t('tetracontrol.info.connection') }}</p>
          <p class="mt-2">{{ t('tetracontrol.info.host') }}</p>
        </div>
      </div>
    </UCard>

    <div v-if="loading" class="flex items-center justify-center py-20">
      <UIcon name="i-lucide-loader-2" class="w-8 h-8 animate-spin text-primary" />
    </div>

    <div v-else>
      <UCard>
        <template #header>
          <h2 class="font-semibold">{{ t('tetracontrol.connection') }}</h2>
        </template>
        <div class="space-y-4">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <UFormField :label="t('tetracontrol.fields.host')">
              <UInput v-model="form.tetraControlHost" :placeholder="t('tetracontrol.fields.hostPlaceholder')" class="w-full" />
            </UFormField>
            <UFormField :label="t('tetracontrol.fields.port')">
              <UInput v-model.number="form.tetraControlPort" type="number" min="1" max="65535" class="w-full" />
            </UFormField>
          </div>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <UFormField :label="t('tetracontrol.fields.username')">
              <UInput v-model="form.tetraControlUsername" :placeholder="t('tetracontrol.fields.username')" class="w-full" />
            </UFormField>
            <UFormField :label="t('tetracontrol.fields.password')">
              <div class="flex gap-2 w-full">
                <UInput
                  v-model="form.tetraControlPassword"
                  :type="showPassword ? 'text' : 'password'"
                  :placeholder="t('tetracontrol.fields.password')"
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
