<script setup lang="ts">
import type { StepperItem } from '@nuxt/ui'

definePageMeta({ layout: false })
const { t } = useI18n()
useHead({ title: () => t('nav.setup') })

const { updateSettings, saving } = useSettings()
const toast = useToast()

const stepper = ref()
const completed = ref(false)

const steps = computed<StepperItem[]>(() => [
  { slot: 'welcome', title: t('setup.steps.welcome'), icon: 'i-lucide-rocket' },
  { slot: 'tetracontrol', title: t('nav.tetracontrol'), icon: 'i-lucide-radio' },
  { slot: 'connect', title: t('setup.steps.connect'), icon: 'i-lucide-network' },
  { slot: 'options', title: t('setup.steps.options'), icon: 'i-lucide-settings' },
  { slot: 'summary', title: t('setup.steps.summary'), icon: 'i-lucide-check-circle' }
])

// TetraControl settings
const tetraControl = ref({
  id: 1,
  tetraControlHost: 'localhost',
  tetraControlPort: 8085,
  tetraControlUsername: 'Connect',
  tetraControlPassword: 'Connect'
})

// First Connect site
const site = ref({
  id: 0,
  connectSettingsId: 1,
  name: '',
  key: '',
  subnetAddresses: [] as any[],
  sirens: [] as any[]
})

// Program options (all entity fields with sensible defaults)
const programOptions = ref({
  id: 1,
  sendVehicleStatus: true,
  sendVehiclePositions: true,
  sendUserOperationStatus: true,
  sendUserAvailability: true,
  sendAlarms: true,
  updateExistingOperations: false,
  userAvailabilityLifetimeDays: 30,
  webSocketReconnectTimeoutMinutes: 5,
  pollForActiveOperationBeforeFallbackMaxRetryCount: 3,
  pollForActiveOperationBeforeFallbackDelay: '00:00:05',
  heartbeatEndpointUrl: '',
  heartbeatInterval: null as string | null,
  ignoreStatus5: false,
  ignoreStatus0: false,
  ignoreStatus9: false,
  addPropertyForAlarmTexts: false,
  useFullyQualifiedSubnetAddressForConnect: false,
  ignoreAlarmWithoutSubnetAddresses: false,
  acceptCalloutsForSirens: false,
  acceptSDSAsCalloutsWithPattern: false
})

async function saveAll() {
  let success = true

  success = await updateSettings('tetracontrol', tetraControl.value) && success

  const connectSettings = {
    id: 1,
    sites: [site.value]
  }
  success = await updateSettings('connect', connectSettings) && success
  success = await updateSettings('program', programOptions.value) && success

  if (success) {
    completed.value = true
    toast.add({
      title: t('setup.toast.completedTitle'),
      description: t('setup.toast.completedDescription'),
      color: 'success'
    })
  }
}
</script>

<template>
  <div class="min-h-screen bg-default">
    <UApp>
      <div class="max-w-3xl mx-auto px-4 py-8">
        <!-- Header -->
        <div class="text-center mb-8">
          <h1 class="text-3xl font-bold">{{ t('app.title') }}</h1>
          <p class="text-muted mt-2">{{ t('setup.subtitle') }}</p>
        </div>

        <!-- Stepper -->
        <UStepper
          ref="stepper"
          :items="steps"
          color="primary"
          size="md"
          class="mb-8"
        >
          <!-- Step: Welcome -->
          <template #welcome>
            <UCard>
              <div class="text-center py-8 space-y-6">
                <div class="inline-flex p-4 rounded-full bg-primary/10">
                  <UIcon name="i-lucide-rocket" class="w-12 h-12 text-primary" />
                </div>
                <div>
                  <h2 class="text-xl font-bold mb-2">{{ t('setup.welcome.title') }}</h2>
                  <p class="text-muted max-w-lg mx-auto">
                    {{ t('setup.welcome.intro') }}
                  </p>
                </div>
                <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 max-w-lg mx-auto text-left">
                  <div class="flex items-start gap-2">
                    <UIcon name="i-lucide-radio" class="w-5 h-5 text-primary shrink-0 mt-0.5" />
                    <div>
                      <p class="text-sm font-medium">{{ t('nav.tetracontrol') }}</p>
                      <p class="text-xs text-muted">{{ t('setup.welcome.tetracontrolHint') }}</p>
                    </div>
                  </div>
                  <div class="flex items-start gap-2">
                    <UIcon name="i-lucide-network" class="w-5 h-5 text-primary shrink-0 mt-0.5" />
                    <div>
                      <p class="text-sm font-medium">{{ t('setup.steps.connect') }}</p>
                      <p class="text-xs text-muted">{{ t('setup.welcome.connectHint') }}</p>
                    </div>
                  </div>
                  <div class="flex items-start gap-2">
                    <UIcon name="i-lucide-settings" class="w-5 h-5 text-primary shrink-0 mt-0.5" />
                    <div>
                      <p class="text-sm font-medium">{{ t('setup.steps.options') }}</p>
                      <p class="text-xs text-muted">{{ t('setup.welcome.optionsHint') }}</p>
                    </div>
                  </div>
                </div>
              </div>
              <template #footer>
                <div class="flex justify-end">
                  <UButton :label="t('setup.next')" icon="i-lucide-arrow-right" trailing @click="stepper?.next()" />
                </div>
              </template>
            </UCard>
          </template>

          <!-- Step: TetraControl -->
          <template #tetracontrol>
            <UCard>
              <div class="space-y-6">
                <div>
                  <h2 class="text-xl font-bold mb-1">{{ t('setup.tetracontrol.title') }}</h2>
                  <p class="text-sm text-muted">{{ t('setup.tetracontrol.description') }}</p>
                </div>

                <div class="space-y-4">
                  <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
                    <UFormField :label="t('setup.tetracontrol.hostLabel')" :description="t('setup.tetracontrol.hostDescription')" class="sm:col-span-2">
                      <UInput v-model="tetraControl.tetraControlHost" :placeholder="t('setup.tetracontrol.hostPlaceholder')" class="w-full" />
                    </UFormField>
                    <UFormField :label="t('setup.tetracontrol.portLabel')" :description="t('setup.tetracontrol.portDescription')">
                      <UInput v-model.number="tetraControl.tetraControlPort" type="number" class="w-full" />
                    </UFormField>
                  </div>
                  <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <UFormField :label="t('setup.tetracontrol.usernameLabel')">
                      <UInput v-model="tetraControl.tetraControlUsername" class="w-full" />
                    </UFormField>
                    <UFormField :label="t('setup.tetracontrol.passwordLabel')">
                      <UInput v-model="tetraControl.tetraControlPassword" type="password" class="w-full" />
                    </UFormField>
                  </div>
                </div>
              </div>
              <template #footer>
                <div class="flex items-center justify-between">
                  <UButton :label="t('setup.back')" icon="i-lucide-arrow-left" variant="outline" @click="stepper?.prev()" />
                  <UButton
                    :label="t('setup.next')"
                    icon="i-lucide-arrow-right"
                    trailing
                    :disabled="!tetraControl.tetraControlHost || tetraControl.tetraControlPort <= 0"
                    @click="stepper?.next()"
                  />
                </div>
              </template>
            </UCard>
          </template>

          <!-- Step: Connect Site -->
          <template #connect>
            <UCard>
              <div class="space-y-6">
                <div>
                  <h2 class="text-xl font-bold mb-1">{{ t('setup.connect.title') }}</h2>
                  <p class="text-sm text-muted">{{ t('setup.connect.description') }}</p>
                </div>

                <div class="space-y-4">
                  <UFormField :label="t('setup.connect.siteNameLabel')" required :description="t('setup.connect.siteNameDescription')">
                    <UInput v-model="site.name" :placeholder="t('setup.connect.siteNamePlaceholder')" class="w-full" />
                  </UFormField>
                  <UFormField :label="t('setup.connect.apiKeyLabel')" required>
                    <UInput v-model="site.key" :placeholder="t('setup.connect.apiKeyPlaceholder')" type="password" class="w-full" />
                    <template #description>
                      <i18n-t keypath="setup.connect.apiKeyDescription" scope="global">
                        <template #portal>
                          <a href="https://connect.feuersoftware.com/v2/app/interfaces" target="_blank" class="text-primary hover:underline">{{ t('setup.connect.portalLink') }}</a>
                        </template>
                      </i18n-t>
                    </template>
                  </UFormField>
                </div>

                <UCard variant="subtle">
                  <div class="flex gap-3">
                    <UIcon name="i-lucide-info" class="w-5 h-5 text-primary shrink-0 mt-0.5" />
                    <div class="text-sm text-muted">
                      <p>{{ t('setup.connect.info') }}</p>
                    </div>
                  </div>
                </UCard>
              </div>
              <template #footer>
                <div class="flex items-center justify-between">
                  <UButton :label="t('setup.back')" icon="i-lucide-arrow-left" variant="outline" @click="stepper?.prev()" />
                  <UButton
                    :label="t('setup.next')"
                    icon="i-lucide-arrow-right"
                    trailing
                    :disabled="!site.name || !site.key"
                    @click="stepper?.next()"
                  />
                </div>
              </template>
            </UCard>
          </template>

          <!-- Step: Options -->
          <template #options>
            <UCard>
              <div class="space-y-6">
                <div>
                  <h2 class="text-xl font-bold mb-1">{{ t('nav.program') }}</h2>
                  <p class="text-sm text-muted">{{ t('setup.options.description') }}</p>
                </div>

                <div class="space-y-4">
                  <div class="space-y-3">
                    <div class="flex items-center justify-between py-2 border-b border-default">
                      <div>
                        <p class="text-sm font-medium">{{ t('setup.options.vehiclePositions') }}</p>
                        <p class="text-xs text-muted">{{ t('setup.options.vehiclePositionsHint') }}</p>
                      </div>
                      <USwitch v-model="programOptions.sendVehiclePositions" />
                    </div>
                    <div class="flex items-center justify-between py-2 border-b border-default">
                      <div>
                        <p class="text-sm font-medium">{{ t('setup.options.vehicleStatus') }}</p>
                        <p class="text-xs text-muted">{{ t('setup.options.vehicleStatusHint') }}</p>
                      </div>
                      <USwitch v-model="programOptions.sendVehicleStatus" />
                    </div>
                    <div class="flex items-center justify-between py-2 border-b border-default">
                      <div>
                        <p class="text-sm font-medium">{{ t('setup.options.userAvailability') }}</p>
                        <p class="text-xs text-muted">{{ t('setup.options.userAvailabilityHint') }}</p>
                      </div>
                      <USwitch v-model="programOptions.sendUserAvailability" />
                    </div>
                    <div class="flex items-center justify-between py-2 border-b border-default">
                      <div>
                        <p class="text-sm font-medium">{{ t('setup.options.userOperationStatus') }}</p>
                        <p class="text-xs text-muted">{{ t('setup.options.userOperationStatusHint') }}</p>
                      </div>
                      <USwitch v-model="programOptions.sendUserOperationStatus" />
                    </div>
                    <div class="flex items-center justify-between py-2">
                      <div>
                        <p class="text-sm font-medium">{{ t('setup.options.alarms') }}</p>
                        <p class="text-xs text-muted">{{ t('setup.options.alarmsHint') }}</p>
                      </div>
                      <USwitch v-model="programOptions.sendAlarms" />
                    </div>
                  </div>
                </div>

                <UCard v-if="programOptions.sendAlarms" variant="subtle">
                  <div class="flex gap-3">
                    <UIcon name="i-lucide-alert-triangle" class="w-5 h-5 text-warning shrink-0 mt-0.5" />
                    <div class="text-sm text-muted">
                      <i18n-t keypath="setup.options.alarmsWarning" tag="p" scope="global">
                        <template #important>
                          <strong>{{ t('setup.options.alarmsWarningImportant') }}</strong>
                        </template>
                        <template #pattern>
                          <strong>{{ t('nav.pattern') }}</strong>
                        </template>
                      </i18n-t>
                      <p class="mt-2">{{ t('setup.options.alarmsWarningDetails') }}</p>
                    </div>
                  </div>
                </UCard>
              </div>
              <template #footer>
                <div class="flex items-center justify-between">
                  <UButton :label="t('setup.back')" icon="i-lucide-arrow-left" variant="outline" @click="stepper?.prev()" />
                  <UButton :label="t('setup.next')" icon="i-lucide-arrow-right" trailing @click="stepper?.next()" />
                </div>
              </template>
            </UCard>
          </template>

          <!-- Step: Summary -->
          <template #summary>
            <UCard v-if="!completed">
              <div class="space-y-6">
                <div>
                  <h2 class="text-xl font-bold mb-1">{{ t('setup.summary.title') }}</h2>
                  <p class="text-sm text-muted">{{ t('setup.summary.description') }}</p>
                </div>

                <div class="space-y-4">
                  <UCard>
                    <template #header>
                      <div class="flex items-center gap-2">
                        <UIcon name="i-lucide-radio" class="w-4 h-4 text-primary" />
                        <h3 class="font-semibold text-sm">{{ t('nav.tetracontrol') }}</h3>
                      </div>
                    </template>
                    <dl class="grid grid-cols-2 gap-2 text-sm">
                      <dt class="text-muted">{{ t('setup.tetracontrol.hostLabel') }}</dt>
                      <dd>{{ tetraControl.tetraControlHost }}:{{ tetraControl.tetraControlPort }}</dd>
                      <dt class="text-muted">{{ t('setup.summary.user') }}</dt>
                      <dd>{{ tetraControl.tetraControlUsername }}</dd>
                    </dl>
                  </UCard>

                  <UCard>
                    <template #header>
                      <div class="flex items-center gap-2">
                        <UIcon name="i-lucide-network" class="w-4 h-4 text-primary" />
                        <h3 class="font-semibold text-sm">{{ t('setup.summary.connectSite') }}</h3>
                      </div>
                    </template>
                    <dl class="grid grid-cols-2 gap-2 text-sm">
                      <dt class="text-muted">{{ t('setup.summary.site') }}</dt>
                      <dd>{{ site.name }}</dd>
                      <dt class="text-muted">{{ t('setup.connect.apiKeyLabel') }}</dt>
                      <dd>{{ site.key ? '••••••••' : '—' }}</dd>
                    </dl>
                  </UCard>

                  <UCard>
                    <template #header>
                      <div class="flex items-center gap-2">
                        <UIcon name="i-lucide-settings" class="w-4 h-4 text-primary" />
                        <h3 class="font-semibold text-sm">{{ t('setup.summary.dataTransfer') }}</h3>
                      </div>
                    </template>
                    <div class="flex flex-wrap gap-2">
                      <UBadge v-if="programOptions.sendVehiclePositions" variant="subtle" color="success">{{ t('setup.options.vehiclePositions') }}</UBadge>
                      <UBadge v-if="programOptions.sendVehicleStatus" variant="subtle" color="success">{{ t('setup.options.vehicleStatus') }}</UBadge>
                      <UBadge v-if="programOptions.sendUserAvailability" variant="subtle" color="success">{{ t('setup.options.userAvailability') }}</UBadge>
                      <UBadge v-if="programOptions.sendUserOperationStatus" variant="subtle" color="success">{{ t('setup.options.userOperationStatus') }}</UBadge>
                      <UBadge v-if="programOptions.sendAlarms" variant="subtle" color="success">{{ t('setup.summary.alarms') }}</UBadge>
                      <UBadge
                        v-if="!programOptions.sendVehiclePositions && !programOptions.sendVehicleStatus && !programOptions.sendUserAvailability && !programOptions.sendUserOperationStatus && !programOptions.sendAlarms"
                        variant="subtle"
                        color="warning"
                      >
                        {{ t('setup.summary.noData') }}
                      </UBadge>
                    </div>
                  </UCard>
                </div>
              </div>
              <template #footer>
                <div class="flex items-center justify-between">
                  <UButton :label="t('setup.back')" icon="i-lucide-arrow-left" variant="outline" @click="stepper?.prev()" />
                  <UButton :label="t('setup.summary.saveConfiguration')" icon="i-lucide-save" :loading="saving" @click="saveAll" />
                </div>
              </template>
            </UCard>

            <!-- Completed -->
            <UCard v-else>
              <div class="text-center py-8 space-y-6">
                <div class="inline-flex p-4 rounded-full bg-green-500/10">
                  <UIcon name="i-lucide-check-circle" class="w-12 h-12 text-green-500" />
                </div>
                <div>
                  <h2 class="text-xl font-bold mb-2">{{ t('setup.completed.title') }}</h2>
                  <p class="text-muted max-w-lg mx-auto">
                    {{ t('setup.completed.description') }}
                  </p>
                </div>

                <UCard variant="subtle" class="max-w-lg mx-auto text-left">
                  <div class="flex gap-3">
                    <UIcon name="i-lucide-list-checks" class="w-5 h-5 text-primary shrink-0 mt-0.5" />
                    <div class="text-sm text-muted">
                      <p class="font-medium text-foreground">{{ t('setup.completed.nextSteps') }}</p>
                      <ul class="mt-2 space-y-1 list-disc list-inside">
                        <li><strong>{{ t('nav.pattern') }}:</strong> {{ t('setup.completed.nextStepPattern') }}</li>
                        <li><strong>{{ t('setup.completed.subnetAddresses') }}:</strong> {{ t('setup.completed.nextStepSubnetAddresses') }}</li>
                        <li><strong>{{ t('setup.completed.statusMappings') }}:</strong> {{ t('setup.completed.nextStepStatusMappings') }}</li>
                      </ul>
                    </div>
                  </div>
                </UCard>

                <div class="flex items-center justify-center gap-3">
                  <UButton
                    :label="t('setup.completed.configurePattern')"
                    icon="i-lucide-regex"
                    to="/settings/pattern"
                  />
                  <UButton
                    :label="t('setup.completed.toDashboard')"
                    icon="i-lucide-layout-dashboard"
                    variant="outline"
                    to="/"
                  />
                </div>
              </div>
            </UCard>
          </template>
        </UStepper>

        <!-- Skip link -->
        <div v-if="!completed" class="text-center mt-4">
          <NuxtLink to="/" class="text-sm text-muted hover:underline">
            {{ t('setup.skip') }}
          </NuxtLink>
        </div>

        <!-- Footer -->
        <div class="text-center mt-8 text-xs text-muted">
          {{ t('app.copyright', { year: new Date().getFullYear() }) }}
        </div>
      </div>
    </UApp>
  </div>
</template>
