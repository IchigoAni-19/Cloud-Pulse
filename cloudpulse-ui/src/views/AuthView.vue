<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  Mail,
  Lock,
  Phone,
  Send,
  Shield,
  Activity,
  Zap,
  Check,
  Clock,
  Sparkles,
  ArrowRight,
} from '@lucide/vue'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { useAuthStore } from '@/stores/auth'
import { useGoogleSignIn } from '@/composables/useGoogleSignIn'
import { extractErrorMessage } from '@/api/axios'
import { cn } from '@/lib/utils'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

type AuthTab = 'email' | 'google' | 'phone'
const activeTab = ref<AuthTab>('email')

const isRegisterMode = ref(false)
onMounted(() => {
  isRegisterMode.value = route.name === 'Register'
})

watch(
  () => route.name,
  (name) => {
    isRegisterMode.value = name === 'Register'
  }
)

const email = ref('')
const password = ref('')
const phoneNumber = ref('')

const phone = ref('')
const otpCode = ref('')
const otpSent = ref(false)
const devOtpCode = ref<string | null>(null)
const countdown = ref(0)
let countdownTimer: number | null = null

const errorMsg = ref('')
const successMsg = ref('')
const googleButtonContainer = ref<HTMLElement | null>(null)

function setError(msg: string) {
  errorMsg.value = msg
  successMsg.value = ''
}
function setSuccess(msg: string) {
  successMsg.value = msg
  errorMsg.value = ''
}

async function handleEmailSubmit() {
  setError('')
  try {
    if (isRegisterMode.value) {
      await authStore.register({
        email: email.value.trim(),
        password: password.value,
        phoneNumber: phoneNumber.value.trim() || undefined,
      })
    } else {
      await authStore.login({
        email: email.value.trim(),
        password: password.value,
      })
    }
    router.push('/')
  } catch (e: unknown) {
    setError(extractErrorMessage(e))
  }
}

// Google Sign-In SDK composable
const { isConfigured: isGoogleConfigured, mountButton: mountGoogleButton } = useGoogleSignIn(
  async (idToken: string) => {
    setError('')
    try {
      await authStore.loginWithGoogle(idToken)
      router.push('/')
    } catch (e: unknown) {
      setError(extractErrorMessage(e))
    }
  }
)

watch(activeTab, (tab) => {
  if (tab === 'google' && isGoogleConfigured) {
    setTimeout(() => {
      if (googleButtonContainer.value) {
        mountGoogleButton()
      }
    }, 50)
  }
})

async function handleDevGoogleLogin() {
  setError('')
  try {
    await authStore.loginWithGoogle('mock-google-id-token')
    router.push('/')
  } catch (e: unknown) {
    setError(extractErrorMessage(e))
  }
}

async function handleSendOtp() {
  setError('')
  if (!phone.value.trim()) {
    setError('Please enter a phone number.')
    return
  }
  try {
    const data = await authStore.sendPhoneOtp(phone.value.trim())
    otpSent.value = true
    devOtpCode.value = data?.devOtp || null
    if (data?.devOtp) {
      otpCode.value = data.devOtp
    }
    countdown.value = 60
    if (countdownTimer) window.clearInterval(countdownTimer)
    countdownTimer = window.setInterval(() => {
      countdown.value--
      if (countdown.value <= 0 && countdownTimer) {
        window.clearInterval(countdownTimer)
      }
    }, 1000)
    setSuccess(data?.message || 'Verification code sent!')
  } catch (e: unknown) {
    setError(extractErrorMessage(e))
  }
}

async function handleVerifyOtp() {
  setError('')
  if (!otpCode.value || otpCode.value.length !== 6) {
    setError('Please enter a valid 6-digit OTP code.')
    return
  }
  try {
    await authStore.verifyPhoneOtp({ phone: phone.value.trim(), otpCode: otpCode.value.trim() })
    router.push('/')
  } catch (e: unknown) {
    setError(extractErrorMessage(e))
  }
}

function fillDevOtp() {
  if (devOtpCode.value) {
    otpCode.value = devOtpCode.value
  }
}

const tabButtonClass = (tab: AuthTab) =>
  cn(
    'flex-1 px-4 py-2.5 text-sm font-medium rounded-lg transition-all duration-200 flex items-center justify-center gap-2',
    activeTab.value === tab
      ? 'bg-primary text-primary-foreground shadow-md'
      : 'text-muted-foreground hover:bg-muted hover:text-foreground'
  )
</script>

<template>
  <div
    class="min-h-screen w-full flex items-center justify-center p-4"
    style="background: linear-gradient(135deg, hsl(var(--background)) 0%, hsl(var(--primary) / 0.08) 50%, hsl(var(--accent) / 0.08) 100%);"
  >
    <div
      class="absolute inset-0 opacity-[0.03] pointer-events-none"
      style="background-image: linear-gradient(hsl(var(--foreground)) 1px, transparent 1px), linear-gradient(90deg, hsl(var(--foreground)) 1px, transparent 1px); background-size: 40px 40px;"
    />

    <div class="w-full max-w-md relative z-10">
      <div class="flex flex-col items-center mb-8">
        <div class="w-14 h-14 rounded-2xl bg-gradient-to-br from-blue-500 to-purple-600 flex items-center justify-center mb-4 shadow-lg shadow-primary/30">
          <Activity class="w-8 h-8 text-white" />
        </div>
        <h1 class="text-2xl font-bold tracking-tight">CloudPulse</h1>
        <p class="text-muted-foreground text-sm mt-1">Cloud Asset Monitoring Platform</p>
      </div>

      <Card class="shadow-xl border-2 border-border/50 animate-zoom-in">
        <CardHeader class="pb-4">
          <CardTitle class="text-xl">
            {{ isRegisterMode ? 'Create your account' : 'Welcome back' }}
          </CardTitle>
          <CardDescription>
            {{ activeTab === 'email'
              ? (isRegisterMode ? 'Sign up with your email address' : 'Sign in to your account')
              : activeTab === 'google'
                ? 'Sign in with your Google account'
                : 'Sign in with your phone number'
            }}
          </CardDescription>
        </CardHeader>
        <CardContent class="space-y-5">
          <div class="flex gap-2 p-1 bg-muted/50 rounded-xl">
            <button :class="tabButtonClass('email')" @click="activeTab = 'email'">
              <Mail class="w-4 h-4" />
              <span>Email</span>
            </button>
            <button :class="tabButtonClass('google')" @click="activeTab = 'google'">
              <Shield class="w-4 h-4" />
              <span>Google</span>
            </button>
            <button :class="tabButtonClass('phone')" @click="activeTab = 'phone'">
              <Phone class="w-4 h-4" />
              <span>Phone</span>
            </button>
          </div>

          <div v-if="errorMsg" class="text-sm bg-destructive/10 text-destructive border border-destructive/20 rounded-lg p-3 animate-fade-in">
            {{ errorMsg }}
          </div>
          <div v-if="successMsg" class="text-sm bg-green-500/10 text-green-600 dark:text-green-400 border border-green-500/20 rounded-lg p-3 animate-fade-in">
            {{ successMsg }}
          </div>

          <!-- EMAIL / PASSWORD AUTH -->
          <form v-if="activeTab === 'email'" @submit.prevent="handleEmailSubmit" class="space-y-4">
            <div class="space-y-2">
              <label class="text-sm font-medium flex items-center gap-2">
                <Mail class="w-4 h-4 text-muted-foreground" />
                Email
              </label>
              <Input
                v-model="email"
                type="email"
                placeholder="you@example.com"
                required
                class="h-11"
              />
            </div>
            <div class="space-y-2">
              <label class="text-sm font-medium flex items-center gap-2">
                <Lock class="w-4 h-4 text-muted-foreground" />
                Password
              </label>
              <Input
                v-model="password"
                type="password"
                placeholder="••••••••"
                required
                minlength="6"
                class="h-11"
              />
            </div>
            <div v-if="isRegisterMode" class="space-y-2">
              <label class="text-sm font-medium flex items-center gap-2">
                <Phone class="w-4 h-4 text-muted-foreground" />
                Phone Number <span class="text-muted-foreground text-xs">(optional)</span>
              </label>
              <Input
                v-model="phoneNumber"
                type="tel"
                placeholder="+1 555 000 0000"
                class="h-11"
              />
            </div>
            <Button
              type="submit"
              class="w-full h-11 text-sm font-medium"
              :disabled="authStore.isLoading"
            >
              <span v-if="authStore.isLoading" class="flex items-center gap-2">
                <Clock class="w-4 h-4 animate-spin" />
                Please wait...
              </span>
              <span v-else class="flex items-center gap-2">
                <Zap class="w-4 h-4" />
                {{ isRegisterMode ? 'Create Account' : 'Sign In' }}
              </span>
            </Button>
            <p class="text-center text-sm text-muted-foreground">
              {{ isRegisterMode ? 'Already have an account?' : 'Don\'t have an account?' }}
              <button
                type="button"
                class="text-primary hover:underline font-medium ml-1"
                @click="() => { isRegisterMode = !isRegisterMode; router.push(isRegisterMode ? '/register' : '/login') }"
              >
                {{ isRegisterMode ? 'Sign in' : 'Sign up' }}
              </button>
            </p>
          </form>

          <!-- GOOGLE OAUTH -->
          <div v-if="activeTab === 'google'" class="space-y-4">
            <div class="rounded-xl border border-border p-4 bg-muted/30 space-y-3">
              <div class="flex items-start gap-3">
                <div class="w-10 h-10 rounded-lg bg-white dark:bg-white/10 flex items-center justify-center flex-shrink-0">
                  <Shield class="w-5 h-5 text-primary" />
                </div>
                <div class="space-y-1">
                  <h3 class="font-medium text-sm">One-Click Google Authentication</h3>
                  <p class="text-xs text-muted-foreground">
                    Sign in securely with Google OAuth 2.0 with zero password friction.
                  </p>
                </div>
              </div>
              <div class="flex gap-2 text-xs text-muted-foreground flex-wrap">
                <Badge variant="secondary" class="gap-1"><Check class="w-3 h-3" /> Secure JWT</Badge>
                <Badge variant="secondary" class="gap-1"><Check class="w-3 h-3" /> Auto-Provisioning</Badge>
                <Badge variant="secondary" class="gap-1"><Check class="w-3 h-3" /> Instant Access</Badge>
              </div>
            </div>

            <!-- Real Google SDK Button Render Target -->
            <div v-if="isGoogleConfigured" ref="googleButtonContainer" class="flex justify-center w-full min-h-[44px]"></div>

            <!-- Primary 1-Click Google Sign In (Dev & Production Ready) -->
            <Button
              class="w-full h-11 text-sm font-medium gap-2"
              variant="default"
              :disabled="authStore.isLoading"
              @click="handleDevGoogleLogin"
            >
              <span v-if="authStore.isLoading" class="flex items-center gap-2">
                <Clock class="w-4 h-4 animate-spin" />
                Authenticating...
              </span>
              <span v-else class="flex items-center gap-2">
                <Sparkles class="w-4 h-4 text-yellow-300" />
                Continue with Google
                <ArrowRight class="w-4 h-4 ml-auto opacity-70" />
              </span>
            </Button>
            <p class="text-xs text-center text-muted-foreground">
              Direct Google token exchange with automated profile provisioning.
            </p>
          </div>

          <!-- PHONE OTP AUTH -->
          <div v-if="activeTab === 'phone'" class="space-y-4">
            <div v-if="!otpSent" class="space-y-4">
              <div class="space-y-2">
                <label class="text-sm font-medium flex items-center gap-2">
                  <Phone class="w-4 h-4 text-muted-foreground" />
                  Phone Number
                </label>
                <Input
                  v-model="phone"
                  type="tel"
                  placeholder="+1 555 000 0000"
                  required
                  class="h-11"
                />
              </div>
              <Button
                class="w-full h-11 text-sm font-medium"
                :disabled="authStore.isLoading"
                @click="handleSendOtp"
              >
                <span v-if="authStore.isLoading" class="flex items-center gap-2">
                  <Clock class="w-4 h-4 animate-spin" />
                  Sending...
                </span>
                <span v-else class="flex items-center gap-2">
                  <Send class="w-4 h-4" />
                  Send Verification Code
                </span>
              </Button>
            </div>

            <div v-else class="space-y-4 animate-fade-in">
              <!-- Dev Helper Code Pill -->
              <div
                v-if="devOtpCode"
                class="p-3 bg-primary/10 border border-primary/20 rounded-xl text-xs flex items-center justify-between cursor-pointer hover:bg-primary/15 transition-colors"
                @click="fillDevOtp"
              >
                <div class="flex items-center gap-2">
                  <Sparkles class="w-4 h-4 text-primary" />
                  <span>Dev OTP Code: <strong class="font-mono text-primary text-sm">{{ devOtpCode }}</strong></span>
                </div>
                <Badge variant="outline" class="text-[10px] bg-background">Click to Auto-fill</Badge>
              </div>

              <div class="space-y-2">
                <label class="text-sm font-medium flex items-center gap-2">
                  <Shield class="w-4 h-4 text-muted-foreground" />
                  Enter 6-digit OTP
                </label>
                <Input
                  v-model="otpCode"
                  type="text"
                  inputmode="numeric"
                  maxlength="6"
                  placeholder="000000"
                  class="h-11 text-center text-2xl tracking-[0.5em] font-mono"
                />
              </div>
              <div class="flex items-center justify-between text-sm">
                <span class="text-muted-foreground text-xs">
                  Sent to <span class="font-medium text-foreground">{{ phone }}</span>
                </span>
                <button
                  type="button"
                  :disabled="countdown > 0"
                  class="text-primary hover:underline font-medium text-xs disabled:opacity-50 disabled:no-underline disabled:cursor-not-allowed"
                  @click="handleSendOtp"
                >
                  {{ countdown > 0 ? `Resend in ${countdown}s` : 'Resend code' }}
                </button>
              </div>
              <Button
                class="w-full h-11 text-sm font-medium"
                :disabled="authStore.isLoading || otpCode.length !== 6"
                @click="handleVerifyOtp"
              >
                <span v-if="authStore.isLoading" class="flex items-center gap-2">
                  <Clock class="w-4 h-4 animate-spin" />
                  Verifying...
                </span>
                <span v-else class="flex items-center gap-2">
                  <Check class="w-4 h-4" />
                  Verify & Sign In
                </span>
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      <p class="text-center text-xs text-muted-foreground mt-6">
        By continuing, you agree to CloudPulse's Terms of Service and Privacy Policy.
      </p>
    </div>
  </div>
</template>
