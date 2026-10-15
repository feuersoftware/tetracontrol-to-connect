<script setup lang="ts">
const route = useRoute()
const { t, locale, locales, setLocale } = useI18n()

const navigationItems = computed(() => [
  {
    label: t('nav.dashboard'),
    icon: 'i-lucide-layout-dashboard',
    to: '/',
    active: route.path === '/'
  },
  {
    label: t('nav.program'),
    icon: 'i-lucide-settings',
    to: '/settings/program',
    active: route.path === '/settings/program'
  },
  {
    label: t('nav.tetracontrol'),
    icon: 'i-lucide-radio',
    to: '/settings/tetracontrol',
    active: route.path === '/settings/tetracontrol'
  },
  {
    label: t('nav.connect'),
    icon: 'i-lucide-network',
    to: '/settings/connect',
    active: route.path === '/settings/connect'
  },
  {
    label: t('nav.status'),
    icon: 'i-lucide-activity',
    to: '/settings/status',
    active: route.path === '/settings/status'
  },
  {
    label: t('nav.pattern'),
    icon: 'i-lucide-regex',
    to: '/settings/pattern',
    active: route.path === '/settings/pattern'
  },
  {
    label: t('nav.severity'),
    icon: 'i-lucide-alert-triangle',
    to: '/settings/severity',
    active: route.path === '/settings/severity'
  },
  {
    label: t('nav.sirenCallout'),
    icon: 'i-lucide-siren',
    to: '/settings/siren-callout',
    active: route.path === '/settings/siren-callout'
  },
  {
    label: t('nav.sirenStatus'),
    icon: 'i-lucide-bell',
    to: '/settings/siren-status',
    active: route.path === '/settings/siren-status'
  },
  {
    label: t('nav.backups'),
    icon: 'i-lucide-archive',
    to: '/backups',
    active: route.path === '/backups'
  },
  {
    label: t('nav.live'),
    icon: 'i-lucide-monitor',
    to: '/live',
    active: route.path === '/live'
  }
])

const sidebarOpen = ref(false)

// Browser tab title follows the active navigation entry
const activeItem = computed(() => navigationItems.value.find(item => item.active))
useHead({ title: () => activeItem.value?.label })

const languageItems = computed(() => locales.value.map(l => ({
  label: l.name,
  type: 'checkbox' as const,
  checked: l.code === locale.value,
  onSelect: () => setLocale(l.code)
})))

// Sidebar links on the dark-red theme background (matches Connect Portal)
const navigationUi = {
  link: 'text-inherit hover:text-white hover:before:bg-white/10 data-active:text-white data-active:before:bg-white/15',
  linkLeadingIcon: 'text-inherit group-hover:text-white group-data-active:text-white'
}
</script>

<template>
  <div class="min-h-screen bg-default flex">
    <!-- Sidebar -->
    <aside
      class="hidden lg:flex flex-col w-72 h-dvh sticky top-0 bg-theme text-theme shrink-0"
    >
      <div class="p-4 border-b border-white/15">
        <div class="flex items-center gap-2">
          <UIcon name="i-lucide-radio-tower" class="w-6 h-6 text-white" />
          <h1 class="text-lg font-bold text-white">{{ t('app.title') }}</h1>
        </div>
        <p class="text-sm mt-1">{{ t('app.admin') }}</p>
      </div>
      <nav class="flex-1 overflow-y-auto p-2">
        <UNavigationMenu
          :items="navigationItems"
          orientation="vertical"
          color="neutral"
          :ui="navigationUi"
          class="w-full"
        />
      </nav>
      <div class="px-4 py-2 border-t border-white/15 text-xs flex items-center justify-between">
        <span>{{ t('app.copyright', { year: new Date().getFullYear() }) }}</span>
        <div class="flex items-center">
          <UDropdownMenu :items="languageItems" :content="{ side: 'top' }">
            <UButton
              icon="i-lucide-languages"
              :label="locale.toUpperCase()"
              :aria-label="t('app.language')"
              color="neutral"
              variant="ghost"
              class="text-white hover:bg-white/10"
            />
          </UDropdownMenu>
          <UColorModeButton color="neutral" class="text-white hover:bg-white/10" />
        </div>
      </div>
    </aside>

    <!-- Mobile sidebar toggle -->
    <div class="lg:hidden fixed top-0 left-0 right-0 z-50 bg-theme text-white p-3 flex items-center gap-3">
      <UButton
        icon="i-lucide-menu"
        :aria-label="t('app.openMenu')"
        color="neutral"
        variant="ghost"
        class="text-white hover:bg-white/10"
        @click="sidebarOpen = !sidebarOpen"
      />
      <div class="flex items-center gap-2">
        <UIcon name="i-lucide-radio-tower" class="w-5 h-5 text-white" />
        <span class="font-bold text-sm text-white">{{ t('app.fullTitle') }}</span>
      </div>
      <div class="ml-auto flex items-center">
        <UDropdownMenu :items="languageItems">
          <UButton
            icon="i-lucide-languages"
            :label="locale.toUpperCase()"
            :aria-label="t('app.language')"
            color="neutral"
            variant="ghost"
            class="text-white hover:bg-white/10"
          />
        </UDropdownMenu>
        <UColorModeButton color="neutral" class="text-white hover:bg-white/10" />
      </div>
    </div>

    <!-- Mobile sidebar drawer -->
    <Teleport to="body">
      <Transition name="slide">
        <div
          v-if="sidebarOpen"
          class="lg:hidden fixed inset-0 z-40"
          @click.self="sidebarOpen = false"
        >
          <div class="absolute inset-0 bg-black/50" @click="sidebarOpen = false" />
          <aside class="relative w-72 h-full bg-theme text-theme flex flex-col z-50">
            <div class="p-4 border-b border-white/15 flex items-center justify-between">
              <div class="flex items-center gap-2">
                <UIcon name="i-lucide-radio-tower" class="w-6 h-6 text-white" />
                <h1 class="text-lg font-bold text-white">{{ t('app.shortTitle') }}</h1>
              </div>
              <UButton icon="i-lucide-x" color="neutral" variant="ghost" size="sm" class="text-white hover:bg-white/10" @click="sidebarOpen = false" />
            </div>
            <nav class="flex-1 overflow-y-auto p-2">
              <UNavigationMenu
                :items="navigationItems"
                orientation="vertical"
                color="neutral"
                :ui="navigationUi"
                class="w-full"
                @click="sidebarOpen = false"
              />
            </nav>
            <div class="p-4 border-t border-white/15 text-xs text-center">
              {{ t('app.copyright', { year: new Date().getFullYear() }) }}
            </div>
          </aside>
        </div>
      </Transition>
    </Teleport>

    <!-- Main content -->
    <main class="flex-1 min-w-0 lg:p-0 pt-14">
      <div class="max-w-5xl mx-auto p-6">
        <slot />
      </div>
    </main>
  </div>
</template>

<style scoped>
.slide-enter-active,
.slide-leave-active {
  transition: all 0.3s ease;
}
.slide-enter-from,
.slide-leave-to {
  opacity: 0;
  transform: translateX(-100%);
}
</style>
