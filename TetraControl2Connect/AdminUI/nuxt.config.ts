export default defineNuxtConfig({
  compatibilityDate: '2025-07-11',
  ssr: false,

  modules: ['@nuxt/ui'],

  css: ['~/assets/css/main.css'],

  devServer: {
    port: 3000
  },

  runtimeConfig: {
    public: {
      apiBase: '/api'
    }
  },

  // In development, proxy API and SignalR requests to the .NET backend
  devProxy: {
    '/api': {
      target: 'http://localhost:5050',
      changeOrigin: true
    },
    '/hubs': {
      target: 'http://localhost:5050',
      changeOrigin: true,
      ws: true
    }
  }
})
