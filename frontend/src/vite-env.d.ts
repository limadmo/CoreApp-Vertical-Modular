/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_URL: string
  readonly VITE_APP_NAME: string
  readonly VITE_APP_VERSION: string
  readonly VITE_DEV_PORT: string
  readonly VITE_DEV_HOST: string
  readonly VITE_DEMO_TENANT_ID: string
  readonly VITE_DEMO_TENANT_NAME: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}