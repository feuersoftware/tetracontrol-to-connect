<script setup lang="ts">
const { fetchSettings, updateSettings, loading, saving } = useSettings()
const toast = useToast()
const { t } = useI18n()

interface AdditionalProperty {
  id: number
  patternSettingsId: number
  name: string
  pattern: string
}

interface PatternSettings {
  id: number
  numberPattern: string
  keywordPattern: string
  factsPattern: string
  reporterNamePattern: string
  reporterPhoneNumberPattern: string
  cityPattern: string
  streetPattern: string
  houseNumberPattern: string
  zipCodePattern: string
  districtPattern: string
  latitudePattern: string
  longitudePattern: string
  ricPattern: string
  additionalProperties: AdditionalProperty[]
}

const form = ref<PatternSettings>({
  id: 1,
  numberPattern: '',
  keywordPattern: '',
  factsPattern: '',
  reporterNamePattern: '',
  reporterPhoneNumberPattern: '',
  cityPattern: '',
  streetPattern: '',
  houseNumberPattern: '',
  zipCodePattern: '',
  districtPattern: '',
  latitudePattern: '',
  longitudePattern: '',
  ricPattern: '',
  additionalProperties: []
})

onMounted(async () => {
  const data = await fetchSettings<PatternSettings>('pattern')
  if (data) form.value = data
})

async function save() {
  await updateSettings('pattern', form.value)
}

function addProperty() {
  form.value.additionalProperties.push({ id: 0, patternSettingsId: 1, name: '', pattern: '' })
}

function removeProperty(index: number) {
  form.value.additionalProperties.splice(index, 1)
}

type PatternFieldKey = keyof Omit<PatternSettings, 'additionalProperties' | 'id'>

const patternFieldKeys: PatternFieldKey[] = [
  'numberPattern',
  'keywordPattern',
  'factsPattern',
  'reporterNamePattern',
  'reporterPhoneNumberPattern',
  'cityPattern',
  'streetPattern',
  'houseNumberPattern',
  'zipCodePattern',
  'districtPattern',
  'latitudePattern',
  'longitudePattern',
  'ricPattern'
]

const patternFields = computed<{ key: PatternFieldKey; label: string; hint: string }[]>(() =>
  patternFieldKeys.map(key => ({
    key,
    label: t(`pattern.fields.${key}.label`),
    hint: t(`pattern.fields.${key}.hint`)
  }))
)

// Technical snippets kept out of i18n messages (contain vue-i18n special characters)
const namedGroupExample = '(?<value>...)'
const regexSyntax = 'C# / .NET (System.Text.RegularExpressions)'

// Regex tester
const testInput = ref('')
const testingField = ref<string | null>(null)

function toggleTest(fieldKey: string) {
  testingField.value = testingField.value === fieldKey ? null : fieldKey
}

function getMatch(pattern: string): { match: boolean; groups: string[] } {
  if (!pattern || !testInput.value) return { match: false, groups: [] }
  try {
    const regex = new RegExp(pattern)
    const result = regex.exec(testInput.value)
    if (result) {
      const groups = result.slice(1).filter(g => g !== undefined)
      return { match: true, groups }
    }
    return { match: false, groups: [] }
  } catch {
    return { match: false, groups: [] }
  }
}

function isValidRegex(pattern: string): boolean {
  if (!pattern) return true
  try {
    new RegExp(pattern)
    return true
  } catch {
    return false
  }
}

// AI assistant modal
const aiModalOpen = ref(false)
const aiFieldLabel = ref('')
const aiCurrentPattern = ref('')
const aiExpectedValue = ref('')
const aiPrivacyConfirmed = ref(false)

function openAiAssistant(fieldLabel: string, currentPattern: string) {
  aiFieldLabel.value = fieldLabel
  aiCurrentPattern.value = currentPattern
  aiExpectedValue.value = ''
  aiPrivacyConfirmed.value = false
  aiModalOpen.value = true
}

function buildAiPrompt(): string {
  const field = aiFieldLabel.value
  const value = aiExpectedValue.value
  let prompt = `${t('pattern.ai.prompt.intro')}\n\n`
  prompt += `## ${t('pattern.ai.prompt.taskHeading')}\n`
  prompt += `${t('pattern.ai.prompt.task', { field })}\n\n`
  prompt += `## ${t('pattern.ai.prompt.sampleHeading')}\n`
  prompt += `\`\`\`\n${testInput.value}\n\`\`\`\n\n`
  prompt += `## ${t('pattern.ai.prompt.expectedHeading')}\n`
  prompt += `${t('pattern.ai.prompt.expected', { field, value: `**${value}**` })}\n\n`
  prompt += `## ${t('pattern.ai.prompt.structureHeading')}\n`
  prompt += `- ${t('pattern.ai.prompt.structureSeparators')}\n`
  prompt += `- ${t('pattern.ai.prompt.structureExample', { value })}\n`
  prompt += `- ${t('pattern.ai.prompt.structurePosition')}\n`
  prompt += `- ${t('pattern.ai.prompt.structureOrientation')}\n\n`
  if (aiCurrentPattern.value) {
    prompt += `## ${t('pattern.ai.prompt.existingHeading')}\n`
    prompt += `\`${aiCurrentPattern.value}\`\n\n`
  }
  prompt += `## ${t('pattern.ai.prompt.requirementsHeading')}\n`
  prompt += `- ${t('pattern.ai.prompt.requirementSyntax', { syntax: regexSyntax })}\n`
  prompt += `- ${t('pattern.ai.prompt.requirementNamedGroup', { group: `\`${namedGroupExample}\`` })}\n`
  prompt += `- ${t('pattern.ai.prompt.requirementRobust')}\n`
  prompt += `- ${t('pattern.ai.prompt.requirementOutput')}\n`
  return prompt
}

function doOpenChatGPT() {
  const prompt = buildAiPrompt()
  aiModalOpen.value = false
  window.open(`https://chatgpt.com/?q=${encodeURIComponent(prompt)}`, '_blank')
}

async function doCopyAndOpen() {
  const prompt = buildAiPrompt()
  await navigator.clipboard.writeText(prompt)
  aiModalOpen.value = false
  toast.add({ title: t('pattern.ai.promptCopiedTitle'), description: t('pattern.ai.promptCopiedDescription'), color: 'success' })
}

function doCopyPrompt() {
  const prompt = buildAiPrompt()
  navigator.clipboard.writeText(prompt)
  aiModalOpen.value = false
  toast.add({ title: t('pattern.ai.copiedTitle'), description: t('pattern.ai.copiedDescription'), color: 'success' })
}
</script>

<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="text-2xl font-bold">{{ t('nav.pattern') }}</h1>
        <p class="text-muted mt-1">{{ t('pattern.subtitle') }}</p>
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
          <p>{{ t('pattern.intro.text') }}</p>
          <i18n-t keypath="pattern.intro.usage" tag="p" class="mt-2" scope="global">
            <template #namedGroups><strong>{{ t('pattern.intro.namedGroups') }}</strong></template>
            <template #example><code class="bg-default px-1 py-0.5 rounded text-xs">{{ namedGroupExample }}</code></template>
            <template #tester><strong>{{ t('pattern.intro.tester') }}</strong></template>
            <template #aiHelp><strong>{{ t('pattern.intro.aiHelp') }}</strong></template>
          </i18n-t>
        </div>
      </div>
    </UCard>

    <!-- Global test input -->
    <UCard class="mb-6">
      <template #header>
        <div class="flex items-center gap-2">
          <UIcon name="i-lucide-flask-conical" class="w-4 h-4 text-primary" />
          <h2 class="font-semibold">{{ t('pattern.tester.title') }}</h2>
        </div>
      </template>
      <div>
        <label class="block text-sm font-medium mb-1">{{ t('pattern.tester.sampleLabel') }}</label>
        <UTextarea
          v-model="testInput"
          :placeholder="t('pattern.tester.samplePlaceholder')"
          :rows="6"
          class="w-full"
        />
        <p class="text-xs text-muted mt-1">{{ t('pattern.tester.sampleHint') }}</p>
      </div>
    </UCard>

    <div v-if="loading" class="flex items-center justify-center py-20">
      <UIcon name="i-lucide-loader-2" class="w-8 h-8 animate-spin text-primary" />
    </div>

    <div v-else class="space-y-6">
      <UCard>
        <template #header>
          <h2 class="font-semibold">{{ t('pattern.standardPatterns') }}</h2>
        </template>
        <div class="space-y-5">
          <div v-for="field in patternFields" :key="field.key">
            <div class="flex items-center justify-between mb-1">
              <label class="block text-sm font-medium">{{ field.label }}</label>
              <div class="flex items-center gap-1">
                <UButton
                  icon="i-lucide-flask-conical"
                  variant="ghost"
                  size="xs"
                  :color="testingField === field.key ? 'primary' : 'neutral'"
                  :title="t('pattern.testPattern')"
                  @click="toggleTest(field.key)"
                />
                <UButton
                  icon="i-lucide-sparkles"
                  variant="ghost"
                  size="xs"
                  :title="t('pattern.aiHelp')"
                  :disabled="!testInput"
                  @click="openAiAssistant(field.label, form[field.key] as string)"
                />
              </div>
            </div>
            <p class="text-xs text-muted mb-1">{{ field.hint }}</p>
            <UTextarea
              v-model="(form[field.key] as string)"
              :placeholder="t('pattern.patternPlaceholder')"
              :color="!isValidRegex(form[field.key] as string) ? 'error' : undefined"
              :rows="2"
              class="w-full font-mono"
            />
            <p v-if="!isValidRegex(form[field.key] as string)" class="text-xs text-error mt-1">
              {{ t('pattern.invalidRegex') }}
            </p>

            <!-- Inline test result -->
            <div v-if="testingField === field.key && testInput" class="mt-2 p-3 rounded-lg bg-elevated border border-default">
              <div v-if="!form[field.key]" class="text-xs text-muted">
                {{ t('pattern.noPattern') }}
              </div>
              <div v-else-if="!isValidRegex(form[field.key] as string)" class="text-xs text-error">
                {{ t('pattern.invalidPatternCannotTest') }}
              </div>
              <div v-else>
                <div class="flex items-center gap-2 mb-1">
                  <div :class="['w-2 h-2 rounded-full', getMatch(form[field.key] as string).match ? 'bg-green-500' : 'bg-red-500']" />
                  <span class="text-xs font-medium">
                    {{ getMatch(form[field.key] as string).match ? t('pattern.matchFound') : t('pattern.noMatch') }}
                  </span>
                </div>
                <div v-if="getMatch(form[field.key] as string).groups.length > 0" class="mt-1">
                  <span class="text-xs text-muted">{{ t('pattern.captureGroups') }}</span>
                  <div class="flex flex-wrap gap-1 mt-1">
                    <UBadge
                      v-for="(group, gi) in getMatch(form[field.key] as string).groups"
                      :key="gi"
                      variant="subtle"
                      color="success"
                      size="sm"
                    >
                      {{ group }}
                    </UBadge>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </UCard>

      <UCard>
        <template #header>
          <div class="flex items-center justify-between">
            <h2 class="font-semibold">{{ t('pattern.additional.title') }}</h2>
            <UButton
              :label="t('common.add')"
              icon="i-lucide-plus"
              variant="outline"
              size="sm"
              @click="addProperty"
            />
          </div>
        </template>

        <div v-if="form.additionalProperties.length === 0" class="text-sm text-muted py-4 text-center">
          {{ t('pattern.additional.empty') }}
        </div>

        <div v-else class="space-y-3">
          <div
            v-for="(prop, index) in form.additionalProperties"
            :key="index"
            class="border border-default rounded-lg p-3"
          >
            <div class="flex items-start gap-2">
              <div class="flex-1 grid grid-cols-1 sm:grid-cols-2 gap-3">
                <div>
                  <label class="block text-xs font-medium mb-1">{{ t('common.name') }}</label>
                  <UInput v-model="prop.name" :placeholder="t('pattern.additional.namePlaceholder')" size="sm" />
                </div>
                <div>
                  <div class="flex items-center justify-between mb-1">
                    <label class="block text-xs font-medium">{{ t('pattern.additional.patternLabel') }}</label>
                    <UButton
                      icon="i-lucide-sparkles"
                      variant="ghost"
                      size="xs"
                      :title="t('pattern.aiHelp')"
                      :disabled="!testInput"
                      @click="openAiAssistant(prop.name || t('pattern.additional.fallbackName'), prop.pattern)"
                    />
                  </div>
                  <UTextarea
                    v-model="prop.pattern"
                    :placeholder="t('pattern.additional.patternPlaceholder')"
                    size="sm"
                    :color="!isValidRegex(prop.pattern) ? 'error' : undefined"
                    :rows="2"
                    class="w-full font-mono"
                  />
                  <p v-if="!isValidRegex(prop.pattern)" class="text-xs text-error mt-1">
                    {{ t('pattern.invalidRegex') }}
                  </p>
                  <!-- Test result for additional property -->
                  <div v-if="testInput && prop.pattern && isValidRegex(prop.pattern)" class="mt-1">
                    <div class="flex items-center gap-1">
                      <div :class="['w-1.5 h-1.5 rounded-full', getMatch(prop.pattern).match ? 'bg-green-500' : 'bg-red-500']" />
                      <span class="text-xs text-muted">
                        {{ getMatch(prop.pattern).match ? t('pattern.match') : t('pattern.noMatch') }}
                      </span>
                    </div>
                  </div>
                </div>
              </div>
              <UButton
                icon="i-lucide-x"
                variant="ghost"
                color="error"
                size="xs"
                class="mt-5"
                @click="removeProperty(index)"
              />
            </div>
          </div>
        </div>
      </UCard>

      <div class="flex justify-end">
        <UButton
          :label="t('common.save')"
          color="neutral"
          icon="i-lucide-save"
          :loading="saving"
          @click="save"
        />
      </div>
    </div>

    <!-- AI Assistant Modal -->
    <UModal v-model:open="aiModalOpen" :ui="{ content: 'sm:max-w-2xl' }">
      <template #content>
        <div class="p-6 space-y-6">
          <div class="flex items-center gap-3">
            <UIcon name="i-lucide-sparkles" class="w-6 h-6 text-primary" />
            <div>
              <h2 class="text-lg font-semibold">{{ t('pattern.ai.title') }}</h2>
              <p class="text-sm text-muted">{{ t('pattern.ai.field', { field: aiFieldLabel }) }}</p>
            </div>
          </div>

          <div>
            <label class="block text-sm font-medium mb-2">{{ t('pattern.ai.sampleLabel') }}</label>
            <div class="p-4 rounded-lg bg-elevated border border-default text-sm font-mono whitespace-pre-wrap break-all max-h-48 overflow-y-auto leading-relaxed">{{ testInput }}</div>
          </div>

          <UFormField :label="t('pattern.ai.expectedValue')" required>
            <template #description>{{ t('pattern.ai.expectedValueDescription', { field: aiFieldLabel }) }}</template>
            <UTextarea
              v-model="aiExpectedValue"
              :placeholder="t('pattern.ai.expectedValuePlaceholder')"
              :rows="2"
              class="w-full"
            />
          </UFormField>

          <div v-if="aiCurrentPattern">
            <label class="block text-sm font-medium mb-1">{{ t('pattern.ai.currentPattern') }}</label>
            <code class="block p-3 rounded-lg bg-elevated border border-default text-sm font-mono break-all">{{ aiCurrentPattern }}</code>
          </div>

          <div class="flex items-start gap-3 p-4 rounded-lg bg-elevated border border-default">
            <UCheckbox v-model="aiPrivacyConfirmed" class="mt-0.5" />
            <i18n-t
              keypath="pattern.ai.privacy"
              tag="label"
              scope="global"
              class="text-sm text-muted cursor-pointer leading-relaxed"
              @click="aiPrivacyConfirmed = !aiPrivacyConfirmed"
            >
              <template #noSensitiveData><strong>{{ t('pattern.ai.noSensitiveData') }}</strong></template>
            </i18n-t>
          </div>

          <div class="flex items-center justify-between pt-2 border-t border-default">
            <UButton :label="t('common.cancel')" variant="ghost" @click="aiModalOpen = false" />
            <div class="flex items-center gap-2">
              <UButton
                icon="i-lucide-clipboard-copy"
                :label="t('pattern.ai.copyPrompt')"
                variant="outline"
                :disabled="!aiExpectedValue"
                @click="doCopyPrompt"
              />
              <UButton
                icon="i-lucide-external-link"
                :label="t('pattern.ai.openChatGpt')"
                :disabled="!aiExpectedValue || !aiPrivacyConfirmed"
                @click="doOpenChatGPT"
              />
            </div>
          </div>
        </div>
      </template>
    </UModal>
  </div>
</template>
