export default defineNuxtConfig({
  compatibilityDate: '2025-07-11',
  ssr: false,

  modules: ['@nuxt/ui', '@nuxtjs/i18n'],

  css: ['~/assets/css/main.css'],

  // Fonts are bundled via @fontsource-variable/public-sans (see main.css),
  // so disable @nuxt/fonts to avoid fetching from external providers.
  ui: {
    fonts: false
  },

  // Icons are bundled into the client from the locally installed @iconify-json
  // packages; never fetch missing icons from the external Iconify API.
  icon: {
    provider: 'none',
    fallbackToApi: false,
    clientBundle: {
      scan: true
    }
  },

  i18n: {
    locales: [
      { code: 'de', language: 'de-DE', name: 'Deutsch', file: 'de.json' },
      { code: 'en', language: 'en-US', name: 'English', file: 'en.json' }
    ],
    defaultLocale: 'de',
    // Keep URLs unchanged; the chosen language is remembered in a cookie
    strategy: 'no_prefix',
    detectBrowserLanguage: {
      useCookie: true,
      cookieKey: 'i18n_locale',
      fallbackLocale: 'de'
    }
  },

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
  },

  // Opt in to Nuxt 5 behaviour to test the upgrade ahead of its release
  // https://nuxt.com/docs/5.x/getting-started/upgrade
  future: {
    compatibilityVersion: 5
  }
})
