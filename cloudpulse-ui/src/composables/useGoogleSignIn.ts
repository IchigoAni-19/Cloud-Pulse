import { ref, onBeforeUnmount } from 'vue'

declare global {
  interface Window {
    google?: {
      accounts: {
        id: {
          initialize: (options: {
            client_id: string
            callback: (response: { credential: string; select_by?: string }) => void
            auto_select?: boolean
            cancel_on_tap_outside?: boolean
          }) => void
          renderButton: (
            element: HTMLElement,
            options: {
              type?: 'standard' | 'icon'
              theme?: 'outline' | 'filled_blue' | 'filled_black'
              size?: 'large' | 'medium' | 'small'
              text?: 'signin_with' | 'signup_with' | 'continue_with' | 'signin'
              shape?: 'rectangular' | 'pill' | 'circle' | 'square'
              logo_alignment?: 'left' | 'center'
              width?: string | number
              locale?: string
              click_listener?: () => void
            }
          ) => void
          prompt: (options?: { mode?: 'popup' | 'redirect' }) => void
          disableAutoSelect: () => void
          storeCredential: (
            credential: { id: string; password: string },
            callback?: (response: { success: boolean }) => void
          ) => void
          cancel: () => void
          revoke: (
            hint: string,
            callback?: (response: { successful: boolean; error: string }) => void
          ) => void
        }
      }
    }
  }
}

const GIS_SDK_URL = 'https://accounts.google.com/gsi/client'

let gisScriptPromise: Promise<void> | null = null

function loadGisSdk(): Promise<void> {
  if (gisScriptPromise) return gisScriptPromise

  gisScriptPromise = new Promise<void>((resolve, reject) => {
    if (typeof window === 'undefined') {
      reject(new Error('Cannot load GIS SDK on server'))
      return
    }

    if (window.google?.accounts?.id) {
      resolve()
      return
    }

    if (document.querySelector<HTMLScriptElement>(`script[src="${GIS_SDK_URL}"]`)) {
      setTimeout(() => {
        if (window.google?.accounts?.id) {
          resolve()
        } else {
          reject(new Error('GIS SDK failed to initialize'))
        }
      }, 1000)
      return
    }

    const script = document.createElement('script')
    script.src = GIS_SDK_URL
    script.async = true
    script.defer = true
    script.referrerPolicy = 'strict-origin-when-cross-origin'
    script.onload = () => {
      if (window.google?.accounts?.id) {
        resolve()
      } else {
        reject(new Error('GIS SDK loaded but google.accounts.id is unavailable'))
      }
    }
    script.onerror = () => reject(new Error('Failed to load Google Identity Services SDK'))
    document.head.appendChild(script)
  })

  return gisScriptPromise
}

export function useGoogleSignIn(
  onCredential: (idToken: string) => void | Promise<void>,
  options?: { clientId?: string; autoSelect?: boolean }
) {
  const clientId = (options?.clientId ?? import.meta.env.VITE_GOOGLE_CLIENT_ID ?? '').trim()
  const isConfigured = Boolean(clientId)
  const isLoading = ref(false)
  const buttonEl = ref<HTMLElement | null>(null)
  let mounted = false

  async function initialize() {
    if (!isConfigured) return
    isLoading.value = true
    try {
      await loadGisSdk()
      window.google!.accounts.id.initialize({
        client_id: clientId,
        auto_select: options?.autoSelect ?? false,
        cancel_on_tap_outside: true,
        callback: async (response) => {
          if (response?.credential) {
            try {
              await onCredential(response.credential)
            } catch (e) {
              console.error('[GIS] Credential exchange failed:', e)
            }
          }
        }
      })
      mounted = true
    } catch (e) {
      console.warn('[GIS] Failed to initialize Google Sign-In SDK:', e)
    } finally {
      isLoading.value = false
    }
  }

  function renderButton(target: HTMLElement, buttonOptions?: Record<string, unknown>) {
    if (!isConfigured || !window.google?.accounts?.id) return
    window.google.accounts.id.renderButton(target, {
      type: 'standard',
      theme: 'outline',
      size: 'large',
      text: 'signin_with',
      shape: 'rectangular',
      logo_alignment: 'left',
      width: '100%',
      ...buttonOptions
    })
  }

  async function promptOneTap() {
    if (!isConfigured || !mounted) return
    try {
      await loadGisSdk()
      window.google!.accounts.id.prompt()
    } catch (e) {
      // silently ignore — user will fall back to the button
    }
  }

  async function mountButton() {
    if (!isConfigured) return
    await initialize()
    requestAnimationFrame(() => {
      if (buttonEl.value) renderButton(buttonEl.value)
    })
  }

  onBeforeUnmount(() => {
    if (window.google?.accounts?.id) {
      try {
        window.google.accounts.id.cancel()
      } catch {
        /* ignore */
      }
    }
  })

  return {
    clientId,
    isConfigured,
    isLoading,
    buttonEl,
    mountButton,
    promptOneTap,
    loadGisSdk
  }
}
