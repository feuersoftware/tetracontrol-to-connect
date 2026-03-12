<script setup lang="ts">
const route = useRoute()

const navigationItems = computed(() => [
  {
    label: 'Dashboard',
    icon: 'i-lucide-layout-dashboard',
    to: '/',
    active: route.path === '/'
  },
  {
    label: 'Programmoptionen',
    icon: 'i-lucide-settings',
    to: '/settings/program',
    active: route.path === '/settings/program'
  },
  {
    label: 'TetraControl',
    icon: 'i-lucide-radio',
    to: '/settings/tetracontrol',
    active: route.path === '/settings/tetracontrol'
  },
  {
    label: 'Connect / Standorte',
    icon: 'i-lucide-network',
    to: '/settings/connect',
    active: route.path === '/settings/connect'
  },
  {
    label: 'Status',
    icon: 'i-lucide-activity',
    to: '/settings/status',
    active: route.path === '/settings/status'
  },
  {
    label: 'Muster (Pattern)',
    icon: 'i-lucide-regex',
    to: '/settings/pattern',
    active: route.path === '/settings/pattern'
  },
  {
    label: 'Schweregrade',
    icon: 'i-lucide-alert-triangle',
    to: '/settings/severity',
    active: route.path === '/settings/severity'
  },
  {
    label: 'Sirenen-Alarmierung',
    icon: 'i-lucide-siren',
    to: '/settings/siren-callout',
    active: route.path === '/settings/siren-callout'
  },
  {
    label: 'Sirenen-Status',
    icon: 'i-lucide-bell',
    to: '/settings/siren-status',
    active: route.path === '/settings/siren-status'
  },
  {
    label: 'Sicherungen',
    icon: 'i-lucide-archive',
    to: '/backups',
    active: route.path === '/backups'
  },
  {
    label: 'Live-Ansicht',
    icon: 'i-lucide-monitor',
    to: '/live',
    active: route.path === '/live'
  }
])

const sidebarOpen = ref(false)
</script>

<template>
  <div class="min-h-screen bg-default flex">
    <!-- Sidebar -->
    <aside
      class="hidden lg:flex flex-col w-72 border-r border-default bg-elevated shrink-0"
    >
      <div class="p-4 border-b border-default">
        <div class="flex items-center gap-2">
          <UIcon name="i-lucide-radio-tower" class="w-6 h-6 text-primary" />
          <h1 class="text-lg font-bold">TetraControl2Connect</h1>
        </div>
        <p class="text-sm text-muted mt-1">Admin</p>
      </div>
      <nav class="flex-1 overflow-y-auto p-2">
        <UNavigationMenu
          :items="navigationItems"
          orientation="vertical"
          class="w-full"
        />
      </nav>
      <div class="p-4 border-t border-default text-xs text-muted text-center">
        © {{ new Date().getFullYear() }} FeuerSoftware GmbH
      </div>
    </aside>

    <!-- Mobile sidebar toggle -->
    <div class="lg:hidden fixed top-0 left-0 right-0 z-50 bg-elevated border-b border-default p-3 flex items-center gap-3">
      <UButton
        icon="i-lucide-menu"
        variant="ghost"
        @click="sidebarOpen = !sidebarOpen"
      />
      <div class="flex items-center gap-2">
        <UIcon name="i-lucide-radio-tower" class="w-5 h-5 text-primary" />
        <span class="font-bold text-sm">TetraControl2Connect Admin</span>
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
          <aside class="relative w-72 h-full bg-elevated border-r border-default flex flex-col z-50">
            <div class="p-4 border-b border-default flex items-center justify-between">
              <div class="flex items-center gap-2">
                <UIcon name="i-lucide-radio-tower" class="w-6 h-6 text-primary" />
                <h1 class="text-lg font-bold">TC2C Admin</h1>
              </div>
              <UButton icon="i-lucide-x" variant="ghost" size="sm" @click="sidebarOpen = false" />
            </div>
            <nav class="flex-1 overflow-y-auto p-2">
              <UNavigationMenu
                :items="navigationItems"
                orientation="vertical"
                class="w-full"
                @click="sidebarOpen = false"
              />
            </nav>
            <div class="p-4 border-t border-default text-xs text-muted text-center">
              © {{ new Date().getFullYear() }} FeuerSoftware GmbH
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
