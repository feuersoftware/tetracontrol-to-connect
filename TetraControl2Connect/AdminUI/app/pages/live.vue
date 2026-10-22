<script setup lang="ts">
import type { BadgeProps } from '@nuxt/ui'

import * as signalR from '@microsoft/signalr'

interface Message {
  type: string
  source: string
  destination: string
  status: string
  statusCode: string
  statusText: string
  text: string
  radioId: number
  radioName: string
  latitude: number
  longitude: number
  timestamp: string
}

const { t, locale } = useI18n()

const messages = ref<Message[]>([])
const isConnected = ref(false)
const hubConnected = ref(false)
const maxMessages = 200

let connection: signalR.HubConnection | null = null

function getTypeLabel(type: string): string {
  switch (type) {
    case 'status': return t('live.types.status')
    case 'pos': return t('live.types.position')
    case 'sds': return 'SDS'
    case 'callout': return t('live.types.callout')
    default: return type
  }
}

function getTypeColor(type: string): BadgeProps['color'] {
  switch (type) {
    case 'status': return 'primary'
    case 'pos': return 'success'
    case 'sds': return 'warning'
    case 'callout': return 'error'
    default: return 'neutral'
  }
}

function formatTime(timestamp: string): string {
  return new Date(timestamp).toLocaleTimeString(locale.value)
}

function clearMessages() {
  messages.value = []
}

onMounted(async () => {
  const hubUrl = `${window.location.origin}/hubs/messages`

  connection = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl)
    .withAutomaticReconnect()
    .build()

  connection.on('MessageReceived', (msg: Message) => {
    messages.value.unshift(msg)
    if (messages.value.length > maxMessages) {
      messages.value = messages.value.slice(0, maxMessages)
    }
  })

  connection.on('ConnectionStateChanged', (state: { isConnected: boolean }) => {
    isConnected.value = state.isConnected
  })

  connection.onclose(() => { hubConnected.value = false })
  connection.onreconnected(() => { hubConnected.value = true })

  try {
    await connection.start()
    hubConnected.value = true
  } catch (err) {
    console.error('SignalR connection failed:', err)
  }
})

onUnmounted(() => {
  connection?.stop()
})
</script>

<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="text-2xl font-bold">{{ t('nav.live') }}</h1>
        <p class="text-muted mt-1">{{ t('live.subtitle') }}</p>
      </div>
      <div class="flex items-center gap-3">
        <div class="flex items-center gap-2">
          <div :class="['w-2.5 h-2.5 rounded-full', hubConnected ? 'bg-green-500' : 'bg-red-500']" />
          <span class="text-sm">{{ hubConnected ? t('live.hubConnected') : t('live.hubDisconnected') }}</span>
        </div>
        <div class="flex items-center gap-2">
          <div :class="['w-2.5 h-2.5 rounded-full', isConnected ? 'bg-green-500' : 'bg-red-500']" />
          <span class="text-sm">{{ isConnected ? t('live.tetraControlConnected') : t('live.tetraControlDisconnected') }}</span>
        </div>
        <UButton
          :label="t('live.clear')"
          icon="i-lucide-trash-2"
          variant="outline"
          size="sm"
          @click="clearMessages"
        />
      </div>
    </div>

    <div v-if="messages.length === 0" class="text-center py-16">
      <UIcon name="i-lucide-radio" class="w-12 h-12 text-muted mx-auto mb-4 animate-pulse" />
      <p class="text-muted">{{ t('live.waiting') }}</p>
    </div>

    <div v-else class="space-y-2">
      <div
        v-for="(msg, index) in messages"
        :key="index"
        class="border border-default rounded-lg p-3 flex items-start gap-3"
      >
        <UBadge :color="getTypeColor(msg.type)" variant="subtle" class="mt-0.5 shrink-0">
          {{ getTypeLabel(msg.type) }}
        </UBadge>
        <div class="flex-1 min-w-0">
          <div class="flex items-center gap-2 text-sm">
            <span class="font-medium">{{ msg.source || t('live.unknownSource') }}</span>
            <span v-if="msg.destination" class="text-muted">→ {{ msg.destination }}</span>
          </div>
          <div class="text-sm text-muted mt-1">
            <span v-if="msg.status">{{ t('live.statusValue', { status: msg.status }) }}</span>
            <span v-if="msg.statusCode"> ({{ msg.statusCode }})</span>
            <span v-if="msg.statusText"> – {{ msg.statusText }}</span>
            <span v-if="msg.text"> | {{ msg.text }}</span>
            <span v-if="msg.latitude && msg.longitude"> | {{ msg.latitude.toFixed(5) }}, {{ msg.longitude.toFixed(5) }}</span>
          </div>
        </div>
        <span class="text-xs text-muted shrink-0">{{ formatTime(msg.timestamp) }}</span>
      </div>
    </div>
  </div>
</template>
