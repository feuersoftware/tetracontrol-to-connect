<script setup lang="ts">
const { fetchSettings, updateSettings, loading, saving } = useSettings()
const toast = useToast()

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

const patternFields: { key: keyof Omit<PatternSettings, 'additionalProperties' | 'id'>; label: string; hint: string }[] = [
  { key: 'numberPattern', label: 'Einsatznummer', hint: 'z.B. eine fortlaufende Nummer wie "E2024-001234"' },
  { key: 'keywordPattern', label: 'Stichwort', hint: 'z.B. "B3" oder "THL1"' },
  { key: 'factsPattern', label: 'Sachverhalt', hint: 'Freitext-Beschreibung des Einsatzes' },
  { key: 'reporterNamePattern', label: 'Meldername', hint: 'Name des Meldenden' },
  { key: 'reporterPhoneNumberPattern', label: 'Melder-Telefonnummer', hint: 'Telefonnummer des Meldenden' },
  { key: 'cityPattern', label: 'Stadt', hint: 'Ortsname des Einsatzortes' },
  { key: 'streetPattern', label: 'Straße', hint: 'Straßenname des Einsatzortes' },
  { key: 'houseNumberPattern', label: 'Hausnummer', hint: 'Hausnummer am Einsatzort' },
  { key: 'zipCodePattern', label: 'PLZ', hint: 'Postleitzahl des Einsatzortes' },
  { key: 'districtPattern', label: 'Ortsteil', hint: 'Ortsteil oder Stadtteil' },
  { key: 'latitudePattern', label: 'Breitengrad', hint: 'Geografische Breite (Dezimalgrad)' },
  { key: 'longitudePattern', label: 'Längengrad', hint: 'Geografische Länge (Dezimalgrad)' },
  { key: 'ricPattern', label: 'RIC', hint: 'Radio Identification Code' }
]

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
  let prompt = `Ich brauche einen regulären Ausdruck (Regex), der einen bestimmten Wert aus einer TETRA-Funkalarmnachricht (SDS) extrahiert.\n\n`
  prompt += `## Aufgabe\n`
  prompt += `Erstelle ein Regex-Muster, das den Wert für das Feld „${aiFieldLabel.value}" aus dem folgenden Alarmtext extrahiert.\n\n`
  prompt += `## Beispiel-Alarmtext\n`
  prompt += `\`\`\`\n${testInput.value}\n\`\`\`\n\n`
  prompt += `## Gesuchter Wert\n`
  prompt += `Im Beispieltext ist der Wert für „${aiFieldLabel.value}": **${aiExpectedValue.value}**\n\n`
  prompt += `## Wichtige Hinweise zur Nachrichtenstruktur\n`
  prompt += `- Der Alarmtext folgt einem festen Aufbau mit Trennzeichen (z.B. Zeilenumbrüche, Semikolons, Tabs oder andere Separatoren)\n`
  prompt += `- Der angegebene Wert „${aiExpectedValue.value}" ist nur ein Beispiel aus dieser konkreten Nachricht. In anderen Nachrichten steht an derselben Position ein anderer Wert (z.B. statt „${aiExpectedValue.value}" könnte dort auch ein komplett anderer Text stehen)\n`
  prompt += `- Das Muster muss die **Position des Wertes innerhalb der Nachrichtenstruktur** erkennen, nicht den konkreten Wert selbst\n`
  prompt += `- Orientiere dich an den umgebenden Trennzeichen, Schlüsselwörtern oder der Feldposition, um den richtigen Abschnitt zu treffen\n\n`
  if (aiCurrentPattern.value) {
    prompt += `## Bestehendes Muster (zur Verbesserung)\n`
    prompt += `\`${aiCurrentPattern.value}\`\n\n`
  }
  prompt += `## Technische Anforderungen\n`
  prompt += `- Regex-Syntax: C# / .NET (System.Text.RegularExpressions)\n`
  prompt += `- Verwende eine benannte Capture-Gruppe: \`(?<value>...)\`\n`
  prompt += `- Das Muster soll robust sein und auch bei leicht abweichenden Werten an derselben Position matchen\n`
  prompt += `- Gib nur das fertige Regex-Muster zurück, ohne Code-Blöcke oder Erklärung\n`
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
  toast.add({ title: 'Prompt kopiert', description: 'Füge den Prompt in dein bevorzugtes AI-Tool ein (z.B. Gemini, Claude, Copilot).', color: 'success' })
}

function doCopyPrompt() {
  const prompt = buildAiPrompt()
  navigator.clipboard.writeText(prompt)
  aiModalOpen.value = false
  toast.add({ title: 'Kopiert', description: 'AI-Prompt wurde in die Zwischenablage kopiert.', color: 'success' })
}
</script>

<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="text-2xl font-bold">Muster (Pattern)</h1>
        <p class="text-muted mt-1">Regex-Muster für die Alarm-Auswertung</p>
      </div>
      <UButton
        label="Speichern"
        icon="i-lucide-save"
        :loading="saving"
        @click="save"
      />
    </div>

    <UCard class="mb-6" variant="subtle">
      <div class="flex gap-3">
        <UIcon name="i-lucide-info" class="w-5 h-5 text-primary shrink-0 mt-0.5" />
        <div class="text-sm text-muted">
          <p>Definiere hier die regulären Ausdrücke (Regex), mit denen Informationen aus TETRA-Alarmmeldungen (SDS) extrahiert werden. Jedes Feld benötigt ein Regex-Muster, das den gewünschten Wert aus dem Alarmtext herausfiltert.</p>
          <p class="mt-2">Verwende <strong>benannte Capture-Gruppen</strong> wie <code class="bg-default px-1 py-0.5 rounded text-xs">(?&lt;value&gt;...)</code>, um den extrahierten Wert zu kennzeichnen. Du kannst deine Muster direkt mit dem <strong>Regex-Tester</strong> überprüfen oder dir über die <strong>AI-Hilfe</strong> Vorschläge generieren lassen.</p>
        </div>
      </div>
    </UCard>

    <!-- Global test input -->
    <UCard class="mb-6">
      <template #header>
        <div class="flex items-center gap-2">
          <UIcon name="i-lucide-flask-conical" class="w-4 h-4 text-primary" />
          <h2 class="font-semibold">Regex-Tester</h2>
        </div>
      </template>
      <div>
        <label class="block text-sm font-medium mb-1">Beispiel-Alarmtext</label>
        <UTextarea
          v-model="testInput"
          placeholder="Füge hier einen Beispiel-Alarmtext ein, um die Muster zu testen..."
          :rows="6"
          class="w-full"
        />
        <p class="text-xs text-muted mt-1">Dieser Text wird zum Testen aller Muster und für die AI-Hilfe verwendet. Klicke auf das Reagenzglas-Symbol neben einem Muster, um das Ergebnis zu sehen.</p>
      </div>
    </UCard>

    <div v-if="loading" class="flex items-center justify-center py-20">
      <UIcon name="i-lucide-loader-2" class="w-8 h-8 animate-spin text-primary" />
    </div>

    <div v-else class="space-y-6">
      <UCard>
        <template #header>
          <h2 class="font-semibold">Standard-Muster</h2>
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
                  title="Muster testen"
                  @click="toggleTest(field.key)"
                />
                <UButton
                  icon="i-lucide-sparkles"
                  variant="ghost"
                  size="xs"
                  title="AI-Hilfe"
                  :disabled="!testInput"
                  @click="openAiAssistant(field.label, form[field.key] as string)"
                />
              </div>
            </div>
            <p class="text-xs text-muted mb-1">{{ field.hint }}</p>
            <UTextarea
              v-model="(form[field.key] as string)"
              placeholder="Regex-Muster eingeben"
              :color="!isValidRegex(form[field.key] as string) ? 'error' : undefined"
              :rows="2"
              class="w-full font-mono"
            />
            <p v-if="!isValidRegex(form[field.key] as string)" class="text-xs text-error mt-1">
              Ungültiger regulärer Ausdruck
            </p>

            <!-- Inline test result -->
            <div v-if="testingField === field.key && testInput" class="mt-2 p-3 rounded-lg bg-elevated border border-default">
              <div v-if="!form[field.key]" class="text-xs text-muted">
                Kein Muster eingetragen.
              </div>
              <div v-else-if="!isValidRegex(form[field.key] as string)" class="text-xs text-error">
                Das Muster ist ungültig und kann nicht getestet werden.
              </div>
              <div v-else>
                <div class="flex items-center gap-2 mb-1">
                  <div :class="['w-2 h-2 rounded-full', getMatch(form[field.key] as string).match ? 'bg-green-500' : 'bg-red-500']" />
                  <span class="text-xs font-medium">
                    {{ getMatch(form[field.key] as string).match ? 'Treffer gefunden' : 'Kein Treffer' }}
                  </span>
                </div>
                <div v-if="getMatch(form[field.key] as string).groups.length > 0" class="mt-1">
                  <span class="text-xs text-muted">Capture-Gruppen:</span>
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
            <h2 class="font-semibold">Zusätzliche Eigenschaften</h2>
            <UButton
              label="Hinzufügen"
              icon="i-lucide-plus"
              variant="outline"
              size="sm"
              @click="addProperty"
            />
          </div>
        </template>

        <div v-if="form.additionalProperties.length === 0" class="text-sm text-muted py-4 text-center">
          Keine zusätzlichen Eigenschaften konfiguriert
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
                  <label class="block text-xs font-medium mb-1">Name</label>
                  <UInput v-model="prop.name" placeholder="Eigenschaftsname" size="sm" />
                </div>
                <div>
                  <div class="flex items-center justify-between mb-1">
                    <label class="block text-xs font-medium">Muster (Regex)</label>
                    <UButton
                      icon="i-lucide-sparkles"
                      variant="ghost"
                      size="xs"
                      title="AI-Hilfe"
                      :disabled="!testInput"
                      @click="openAiAssistant(prop.name || 'Zusätzliche Eigenschaft', prop.pattern)"
                    />
                  </div>
                  <UTextarea
                    v-model="prop.pattern"
                    placeholder="Regex-Muster"
                    size="sm"
                    :color="!isValidRegex(prop.pattern) ? 'error' : undefined"
                    :rows="2"
                    class="w-full font-mono"
                  />
                  <p v-if="!isValidRegex(prop.pattern)" class="text-xs text-error mt-1">
                    Ungültiger regulärer Ausdruck
                  </p>
                  <!-- Test result for additional property -->
                  <div v-if="testInput && prop.pattern && isValidRegex(prop.pattern)" class="mt-1">
                    <div class="flex items-center gap-1">
                      <div :class="['w-1.5 h-1.5 rounded-full', getMatch(prop.pattern).match ? 'bg-green-500' : 'bg-red-500']" />
                      <span class="text-xs text-muted">
                        {{ getMatch(prop.pattern).match ? 'Treffer' : 'Kein Treffer' }}
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
          label="Speichern"
          icon="i-lucide-save"
          :loading="saving"
          @click="save"
        />
      </div>
    </div>

    <!-- AI Assistant Modal -->
    <UModal v-model:open="aiModalOpen" :ui="{ width: 'sm:max-w-2xl' }">
      <template #content>
        <div class="p-6 space-y-6">
          <div class="flex items-center gap-3">
            <UIcon name="i-lucide-sparkles" class="w-6 h-6 text-primary" />
            <div>
              <h2 class="text-lg font-semibold">AI-Regex-Hilfe</h2>
              <p class="text-sm text-muted">Feld: {{ aiFieldLabel }}</p>
            </div>
          </div>

          <div>
            <label class="block text-sm font-medium mb-2">Beispiel-Alarmtext</label>
            <div class="p-4 rounded-lg bg-elevated border border-default text-sm font-mono whitespace-pre-wrap break-all max-h-48 overflow-y-auto leading-relaxed">{{ testInput }}</div>
          </div>

          <UFormField label="Erwarteter Wert" required>
            <template #description>Welcher Wert soll für &bdquo;{{ aiFieldLabel }}&ldquo; aus dem Beispieltext extrahiert werden?</template>
            <UTextarea
              v-model="aiExpectedValue"
              placeholder="z.B. den konkreten Wert aus dem Beispieltext oben hier einfügen"
              :rows="2"
              class="w-full"
            />
          </UFormField>

          <div v-if="aiCurrentPattern">
            <label class="block text-sm font-medium mb-1">Aktuelles Muster</label>
            <code class="block p-3 rounded-lg bg-elevated border border-default text-sm font-mono break-all">{{ aiCurrentPattern }}</code>
          </div>

          <div class="flex items-start gap-3 p-4 rounded-lg bg-elevated border border-default">
            <UCheckbox v-model="aiPrivacyConfirmed" class="mt-0.5" />
            <label class="text-sm text-muted cursor-pointer leading-relaxed" @click="aiPrivacyConfirmed = !aiPrivacyConfirmed">
              Ich bestätige, dass der Beispieltext <strong>keine vertraulichen oder datenschutzrelevanten Daten</strong> enthält. Der Text wird an einen externen AI-Dienst übermittelt.
            </label>
          </div>

          <div class="flex items-center justify-between pt-2 border-t border-default">
            <UButton label="Abbrechen" variant="ghost" @click="aiModalOpen = false" />
            <div class="flex items-center gap-2">
              <UButton
                icon="i-lucide-clipboard-copy"
                label="Prompt kopieren"
                variant="outline"
                :disabled="!aiExpectedValue"
                @click="doCopyPrompt"
              />
              <UButton
                icon="i-lucide-external-link"
                label="ChatGPT öffnen"
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
