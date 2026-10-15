<script setup lang="ts">
import { de, en } from '@nuxt/ui/locale'

const { locale, t } = useI18n()

// Nuxt UI's built-in texts follow the selected language
const uiLocale = computed(() => (locale.value === 'en' ? en : de))

useHead({
  htmlAttrs: { lang: locale },
  titleTemplate: title => title ? `${title} · ${t('app.fullTitle')}` : t('app.fullTitle')
})
</script>

<template>
  <UApp :locale="uiLocale">
    <NuxtLayout>
      <NuxtPage />
    </NuxtLayout>
  </UApp>
</template>
