export function useSettings() {
  const toast = useToast()
  const { t } = useI18n()
  const config = useRuntimeConfig()
  const apiBase = config.public.apiBase

  const loading = ref(false)
  const saving = ref(false)

  async function fetchAllSettings(): Promise<Record<string, any> | null> {
    loading.value = true
    try {
      return await $fetch<Record<string, any>>(`${apiBase}/settings`)
    } catch {
      toast.add({
        title: t('settings.loadErrorTitle'),
        description: t('settings.loadAllError'),
        color: 'error'
      })
      return null
    } finally {
      loading.value = false
    }
  }

  async function fetchSettings<T = any>(sectionName: string): Promise<T | null> {
    loading.value = true
    try {
      return await $fetch<T>(`${apiBase}/settings/${sectionName}`)
    } catch {
      toast.add({
        title: t('settings.loadErrorTitle'),
        description: t('settings.loadError', { section: sectionName }),
        color: 'error'
      })
      return null
    } finally {
      loading.value = false
    }
  }

  async function updateSettings(sectionName: string, data: any): Promise<boolean> {
    saving.value = true
    try {
      await $fetch(`${apiBase}/settings/${sectionName}`, {
        method: 'PUT',
        body: data
      })
      toast.add({
        title: t('settings.savedTitle'),
        description: t('settings.saved', { section: sectionName }),
        color: 'success'
      })
      return true
    } catch {
      toast.add({
        title: t('settings.saveErrorTitle'),
        description: t('settings.saveError', { section: sectionName }),
        color: 'error'
      })
      return false
    } finally {
      saving.value = false
    }
  }

  return {
    loading,
    saving,
    fetchAllSettings,
    fetchSettings,
    updateSettings
  }
}
