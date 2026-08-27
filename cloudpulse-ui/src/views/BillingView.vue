<script setup lang="ts">
import { ref, computed } from 'vue'
import {
  CreditCard,
  Shield,
  Check,
  Zap,
  Cloud,
  Clock,
  RefreshCw,
  Activity,
  BarChart3,
  AlertTriangle,
} from '@lucide/vue'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { useAuthStore } from '@/stores/auth'
import { apiClient } from '@/api/axios'

const authStore = useAuthStore()
const isUpgrading = ref(false)
const errorMsg = ref('')
const successMsg = ref('')

const currentTier = computed(() => authStore.user?.subscriptionTier || 'Free')

async function handleUpgrade() {
  errorMsg.value = ''
  successMsg.value = ''
  isUpgrading.value = true
  try {
    const orderRes = await apiClient.post('/payments/create-order', { planTier: 'Pro' })
    const orderData = orderRes.data

    const mockSignature = `mock-sig-${orderData?.orderId || Date.now()}`
    await apiClient.post('/payments/verify', {
      orderId: orderData?.orderId,
      signature: mockSignature,
      planTier: 'Pro',
    })

    if (authStore.user) {
      authStore.user = { ...authStore.user, subscriptionTier: 'Pro' }
      localStorage.setItem('user', JSON.stringify(authStore.user))
    }
    successMsg.value = 'Successfully upgraded to Pro tier! Enjoy unlimited monitoring.'
  } catch (e: any) {
    errorMsg.value = e.response?.data?.message || e.message || 'Payment processing failed. Please try again.'
  } finally {
    isUpgrading.value = false
  }
}
</script>

<template>
  <div class="container mx-auto px-4 py-8 space-y-8 max-w-6xl">
    <div class="text-center space-y-3 max-w-2xl mx-auto">
      <div class="inline-flex items-center gap-2 px-3 py-1.5 rounded-full bg-primary/10 text-primary text-sm font-medium border border-primary/20">
        <CreditCard class="w-4 h-4" />
        Billing & Plans
      </div>
      <h1 class="text-3xl md:text-4xl font-bold tracking-tight">
        Choose the plan that fits your cloud
      </h1>
      <p class="text-muted-foreground">
        Start free, upgrade when you need more. No contracts, cancel anytime.
      </p>
      <div class="pt-2">
        <Badge variant="outline" class="px-3 py-1">
          Current plan:
          <span class="ml-1.5 font-semibold">{{ currentTier }}</span>
        </Badge>
      </div>
    </div>

    <div v-if="errorMsg" class="max-w-2xl mx-auto text-sm bg-destructive/10 text-destructive border border-destructive/20 rounded-xl p-4 animate-fade-in flex items-start gap-3">
      <AlertTriangle class="w-5 h-5 flex-shrink-0 mt-0.5" />
      <div>
        <p class="font-semibold">Upgrade Failed</p>
        <p class="mt-0.5 opacity-90">{{ errorMsg }}</p>
      </div>
    </div>
    <div v-if="successMsg" class="max-w-2xl mx-auto text-sm bg-green-500/10 text-green-600 dark:text-green-400 border border-green-500/20 rounded-xl p-4 animate-fade-in flex items-start gap-3">
      <Shield class="w-5 h-5 flex-shrink-0 mt-0.5" />
      <div>
        <p class="font-semibold">Upgrade Successful!</p>
        <p class="mt-0.5 opacity-90">{{ successMsg }}</p>
      </div>
    </div>

    <div class="grid grid-cols-1 md:grid-cols-2 gap-6 items-stretch max-w-5xl mx-auto">
      <Card
        class="border-border/60 shadow-sm flex flex-col transition-all duration-300 hover:shadow-lg relative overflow-hidden"
      >
        <div
          v-if="currentTier === 'Free'"
          class="absolute top-0 right-0"
        >
          <div class="bg-muted text-muted-foreground text-[10px] uppercase tracking-wider font-bold px-3 py-1.5 rounded-bl-xl border-l border-b border-border">
            Current Plan
          </div>
        </div>
        <CardHeader class="pb-4 space-y-3">
          <div class="flex items-center gap-2">
            <Cloud class="w-5 h-5 text-muted-foreground" />
            <CardTitle class="text-xl">Free</CardTitle>
          </div>
          <div class="flex items-baseline gap-1">
            <span class="text-4xl font-extrabold tracking-tight">$0</span>
            <span class="text-muted-foreground text-sm">/month</span>
          </div>
          <CardDescription class="text-sm">
            Perfect for side projects and getting started with cloud monitoring.
          </CardDescription>
        </CardHeader>
        <CardContent class="pt-2 space-y-5 flex-1 flex flex-col">
          <ul class="space-y-3 text-sm">
            <li class="flex items-start gap-3">
              <div class="w-5 h-5 rounded-full bg-green-500/15 flex items-center justify-center flex-shrink-0 mt-0.5">
                <Check class="w-3 h-3 text-green-600 dark:text-green-400" />
              </div>
              <span>
                Max <span class="font-semibold">3 cloud assets</span>
              </span>
            </li>
            <li class="flex items-start gap-3">
              <div class="w-5 h-5 rounded-full bg-green-500/15 flex items-center justify-center flex-shrink-0 mt-0.5">
                <Check class="w-3 h-3 text-green-600 dark:text-green-400" />
              </div>
              <span>
                <span class="font-semibold">60-sec</span> check interval
              </span>
            </li>
            <li class="flex items-start gap-3">
              <div class="w-5 h-5 rounded-full bg-green-500/15 flex items-center justify-center flex-shrink-0 mt-0.5">
                <Check class="w-3 h-3 text-green-600 dark:text-green-400" />
              </div>
              <span>Basic dashboard overview</span>
            </li>
            <li class="flex items-start gap-3">
              <div class="w-5 h-5 rounded-full bg-green-500/15 flex items-center justify-center flex-shrink-0 mt-0.5">
                <Check class="w-3 h-3 text-green-600 dark:text-green-400" />
              </div>
              <span>Email support</span>
            </li>
          </ul>
          <div class="mt-auto pt-2">
            <Button
              variant="outline"
              disabled
              class="w-full h-11 cursor-not-allowed"
            >
              <span class="flex items-center gap-2">
                <Check class="w-4 h-4" />
                Current Plan
              </span>
            </Button>
          </div>
        </CardContent>
      </Card>

      <Card
        class="relative flex flex-col transition-all duration-300 shadow-xl"
        style="background:
          linear-gradient(hsl(var(--card)), hsl(var(--card))) padding-box,
          linear-gradient(135deg, hsl(var(--primary)) 0%, hsl(var(--accent)) 50%, hsl(var(--chart-4)) 100%) border-box;
          border: 2px solid transparent;"
      >
        <div class="absolute -top-3 left-1/2 -translate-x-1/2">
          <Badge class="px-4 py-1 text-xs shadow-lg shadow-primary/30" style="background: linear-gradient(135deg, hsl(var(--primary)), hsl(var(--accent)));">
            <Zap class="w-3 h-3 mr-1" />
            Most Popular
          </Badge>
        </div>
        <div
          v-if="currentTier === 'Pro'"
          class="absolute top-0 right-0"
        >
          <div class="bg-gradient-to-r from-primary to-accent text-primary-foreground text-[10px] uppercase tracking-wider font-bold px-3 py-1.5 rounded-bl-xl border-l border-b border-primary/30">
            Current Plan
          </div>
        </div>
        <CardHeader class="pb-4 space-y-3 pt-6">
          <div class="flex items-center gap-2">
            <Zap class="w-5 h-5 text-primary" />
            <CardTitle class="text-xl">Pro</CardTitle>
          </div>
          <div class="flex items-baseline gap-1">
            <span class="text-4xl font-extrabold tracking-tight bg-gradient-to-r from-primary to-accent bg-clip-text text-transparent">
              $29
            </span>
            <span class="text-muted-foreground text-sm">/month</span>
          </div>
          <CardDescription class="text-sm">
            For growing teams and production workloads with serious monitoring needs.
          </CardDescription>
        </CardHeader>
        <CardContent class="pt-2 space-y-5 flex-1 flex flex-col">
          <ul class="space-y-3 text-sm">
            <li class="flex items-start gap-3">
              <div class="w-5 h-5 rounded-full bg-primary/15 flex items-center justify-center flex-shrink-0 mt-0.5">
                <Check class="w-3 h-3 text-primary" />
              </div>
              <span>
                <span class="font-semibold">Unlimited</span> cloud assets
              </span>
            </li>
            <li class="flex items-start gap-3">
              <div class="w-5 h-5 rounded-full bg-primary/15 flex items-center justify-center flex-shrink-0 mt-0.5">
                <Check class="w-3 h-3 text-primary" />
              </div>
              <span>
                <span class="font-semibold">10-sec</span> ping sweeps
              </span>
            </li>
            <li class="flex items-start gap-3">
              <div class="w-5 h-5 rounded-full bg-primary/15 flex items-center justify-center flex-shrink-0 mt-0.5">
                <Check class="w-3 h-3 text-primary" />
              </div>
              <span>Real-time incident alerts</span>
            </li>
            <li class="flex items-start gap-3">
              <div class="w-5 h-5 rounded-full bg-primary/15 flex items-center justify-center flex-shrink-0 mt-0.5">
                <Check class="w-3 h-3 text-primary" />
              </div>
              <span>Priority 24/7 support</span>
            </li>
            <li class="flex items-start gap-3">
              <div class="w-5 h-5 rounded-full bg-primary/15 flex items-center justify-center flex-shrink-0 mt-0.5">
                <Check class="w-3 h-3 text-primary" />
              </div>
              <span>Full metrics history (90+ days)</span>
            </li>
            <li class="flex items-start gap-3">
              <div class="w-5 h-5 rounded-full bg-primary/15 flex items-center justify-center flex-shrink-0 mt-0.5">
                <Check class="w-3 h-3 text-primary" />
              </div>
              <span>Advanced analytics & reporting</span>
            </li>
          </ul>
          <div class="mt-auto pt-2 space-y-2">
            <Button
              v-if="currentTier !== 'Pro'"
              class="w-full h-11 font-semibold shadow-lg shadow-primary/30 hover:shadow-primary/40 transition-shadow"
              style="background: linear-gradient(135deg, hsl(var(--primary)), hsl(var(--accent)));"
              :disabled="isUpgrading"
              @click="handleUpgrade"
            >
              <span v-if="isUpgrading" class="flex items-center gap-2">
                <RefreshCw class="w-4 h-4 animate-spin" />
                Processing...
              </span>
              <span v-else class="flex items-center gap-2">
                <Zap class="w-4 h-4" />
                Upgrade to Pro
              </span>
            </Button>
            <Button
              v-else
              variant="outline"
              disabled
              class="w-full h-11 cursor-not-allowed"
            >
              <span class="flex items-center gap-2">
                <Check class="w-4 h-4" />
                You're on Pro
              </span>
            </Button>
            <p class="text-[11px] text-center text-muted-foreground pt-1">
              Secure checkout • Cancel anytime • 14-day money-back guarantee
            </p>
          </div>
        </CardContent>
      </Card>
    </div>

    <div class="max-w-5xl mx-auto pt-4">
      <Card class="border-border/60 bg-muted/30">
        <CardContent class="p-5">
          <div class="grid grid-cols-2 md:grid-cols-4 gap-4 text-center">
            <div class="space-y-1">
              <BarChart3 class="w-6 h-6 text-primary mx-auto" />
              <p class="text-2xl font-bold">99.9%</p>
              <p class="text-xs text-muted-foreground">SLA Uptime</p>
            </div>
            <div class="space-y-1">
              <Activity class="w-6 h-6 text-primary mx-auto" />
              <p class="text-2xl font-bold">10s</p>
              <p class="text-xs text-muted-foreground">Min Check</p>
            </div>
            <div class="space-y-1">
              <Clock class="w-6 h-6 text-primary mx-auto" />
              <p class="text-2xl font-bold">90+</p>
              <p class="text-xs text-muted-foreground">Days History</p>
            </div>
            <div class="space-y-1">
              <Shield class="w-6 h-6 text-primary mx-auto" />
              <p class="text-2xl font-bold">24/7</p>
              <p class="text-xs text-muted-foreground">Pro Support</p>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  </div>
</template>
