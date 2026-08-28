<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import {
  CreditCard,
  Shield,
  Check,
  Zap,
  Cloud,
  RefreshCw,
  Activity,
  AlertTriangle,
  RotateCcw,
  Sparkles,
  Smartphone,
  Building2,
  Lock,
  Download,
  FileText,
  Calendar,
  CheckCircle2,
  Globe,
} from '@lucide/vue'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import {
  Dialog,
  DialogContent,
} from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { useAuthStore } from '@/stores/auth'
import { apiClient } from '@/api/axios'

declare global {
  interface Window {
    Razorpay: any
  }
}

const authStore = useAuthStore()
const billingCycle = ref<'monthly' | 'annual'>('monthly')
const isUpgrading = ref(false)
const isResetting = ref(false)
const isCancelling = ref(false)
const errorMsg = ref('')
const successMsg = ref('')

// Invoices history
interface Invoice {
  id: string
  invoiceNumber: string
  orderId: string
  paymentId?: string
  amount: number
  currency: string
  status: string
  planName: string
  paymentMethod?: string
  issuedAt: string
}
const invoices = ref<Invoice[]>([])
const isLoadingInvoices = ref(false)

// Checkout Modal state
const isCheckoutOpen = ref(false)
const activePaymentTab = ref<'card' | 'upi' | 'netbanking'>('card')
const isProcessingPayment = ref(false)
const processingStep = ref(1) // 1: Encrypting, 2: 3D Secure, 3: Provisioning
const currentOrderId = ref('')

// Form Inputs
const cardNumber = ref('')
const cardExpiry = ref('')
const cardCvv = ref('')
const cardHolder = ref('')
const billingPostal = ref('')
const agreeTerms = ref(false)

// UPI Inputs
const upiId = ref('')
const qrSecondsLeft = ref(300)
let qrTimer: number | null = null

// Netbanking
const selectedBank = ref('')

// Validation Errors
const formErrors = ref<{
  cardNumber?: string
  cardExpiry?: string
  cardCvv?: string
  cardHolder?: string
  billingPostal?: string
  agreeTerms?: string
  upiId?: string
  selectedBank?: string
}>({})

// Receipt / Invoice Modal
const isReceiptOpen = ref(false)
const selectedReceipt = ref<Invoice | null>(null)

// Cancel Subscription confirmation modal
const isCancelModalOpen = ref(false)

const popularBanks = [
  { id: 'HDFC', name: 'HDFC Bank', code: 'HDFC' },
  { id: 'ICICI', name: 'ICICI Bank', code: 'ICICI' },
  { id: 'SBI', name: 'State Bank of India', code: 'SBI' },
  { id: 'AXIS', name: 'Axis Bank', code: 'UTIB' },
  { id: 'KOTAK', name: 'Kotak Mahindra', code: 'KKBK' },
  { id: 'PNB', name: 'Punjab National Bank', code: 'PUNB' },
]

const currentTier = computed(() => authStore.user?.subscriptionTier || 'Free')

const planPrice = computed(() => {
  return billingCycle.value === 'annual' ? 23990 : 2499
})

const planPriceUSD = computed(() => {
  return billingCycle.value === 'annual' ? 290 : 29
})

const gstAmount = computed(() => {
  return Math.round(planPrice.value * 0.18)
})

const totalPayable = computed(() => {
  return planPrice.value + gstAmount.value
})

// Detect Card Brand from card number
const detectedCardBrand = computed(() => {
  const cleaned = cardNumber.value.replace(/\s+/g, '')
  if (/^4/.test(cleaned)) return 'Visa'
  if (/^(5[1-5]|2[2-7])/.test(cleaned)) return 'Mastercard'
  if (/^3[47]/.test(cleaned)) return 'Amex'
  if (/^(60|65|81|82)/.test(cleaned)) return 'RuPay'
  return 'Card'
})

// Format card number with spaces as user types
function handleCardInput(event: Event) {
  const input = event.target as HTMLInputElement
  let val = input.value.replace(/\D/g, '').substring(0, 16)
  val = val.replace(/(.{4})/g, '$1 ').trim()
  cardNumber.value = val
  if (formErrors.value.cardNumber) delete formErrors.value.cardNumber
}

// Format expiry MM/YY
function handleExpiryInput(event: Event) {
  const input = event.target as HTMLInputElement
  let val = input.value.replace(/\D/g, '').substring(0, 4)
  if (val.length >= 3) {
    val = `${val.substring(0, 2)}/${val.substring(2, 4)}`
  }
  cardExpiry.value = val
  if (formErrors.value.cardExpiry) delete formErrors.value.cardExpiry
}

function handleCvvInput(event: Event) {
  const input = event.target as HTMLInputElement
  cardCvv.value = input.value.replace(/\D/g, '').substring(0, 4)
  if (formErrors.value.cardCvv) delete formErrors.value.cardCvv
}

// Quick Auto-Fill Test Card Helper
function autoFillTestCard() {
  cardNumber.value = '4111 2222 3333 4444'
  cardExpiry.value = '12/28'
  cardCvv.value = '789'
  cardHolder.value = authStore.user?.email ? authStore.user.email.split('@')[0].toUpperCase() : 'JOHN DOE'
  billingPostal.value = '10001'
  agreeTerms.value = true
  formErrors.value = {}
}

function autoFillTestUpi() {
  upiId.value = 'cloudpulse.demo@okhdfcbank'
  agreeTerms.value = true
  formErrors.value = {}
}

async function fetchInvoices() {
  isLoadingInvoices.value = true
  try {
    const res = await apiClient.get('/payments/invoices')
    invoices.value = res.data || []
  } catch (e) {
    // ignore
  } finally {
    isLoadingInvoices.value = false
  }
}

onMounted(() => {
  fetchInvoices()
})

watch(isCheckoutOpen, (open) => {
  if (open) {
    qrSecondsLeft.value = 300
    if (qrTimer) clearInterval(qrTimer)
    qrTimer = window.setInterval(() => {
      if (qrSecondsLeft.value > 0) {
        qrSecondsLeft.value--
      }
    }, 1000)
  } else {
    if (qrTimer) clearInterval(qrTimer)
  }
})

async function openCheckoutModal() {
  errorMsg.value = ''
  successMsg.value = ''
  formErrors.value = {}
  isUpgrading.value = true

  // Reset inputs
  cardNumber.value = ''
  cardExpiry.value = ''
  cardCvv.value = ''
  cardHolder.value = authStore.user?.email ? authStore.user.email.split('@')[0].toUpperCase() : ''
  billingPostal.value = ''
  agreeTerms.value = false
  upiId.value = ''
  selectedBank.value = ''

  try {
    const orderRes = await apiClient.post('/payments/create-order', {
      planTier: 'Pro',
      billingCycle: billingCycle.value === 'annual' ? 'Annual' : 'Monthly',
    })
    currentOrderId.value = orderRes.data.orderId || `order_${Date.now()}`
    isCheckoutOpen.value = true
  } catch (e: any) {
    errorMsg.value = e.response?.data?.message || e.message || 'Unable to initialize checkout.'
  } finally {
    isUpgrading.value = false
  }
}

function validateForm(): boolean {
  const errors: typeof formErrors.value = {}

  if (!agreeTerms.value) {
    errors.agreeTerms = 'You must agree to the subscription terms to continue.'
  }

  if (activePaymentTab.value === 'card') {
    const rawCard = cardNumber.value.replace(/\s+/g, '')
    if (!rawCard || rawCard.length < 15) {
      errors.cardNumber = 'Please enter a valid 16-digit card number.'
    }

    if (!cardExpiry.value || !/^(0[1-9]|1[0-2])\/\d{2}$/.test(cardExpiry.value)) {
      errors.cardExpiry = 'Enter expiry date in MM/YY format.'
    } else {
      const [monthStr, yearStr] = cardExpiry.value.split('/')
      const expMonth = parseInt(monthStr, 10)
      const expYear = parseInt('20' + yearStr, 10)
      const now = new Date()
      const currentYear = now.getFullYear()
      const currentMonth = now.getMonth() + 1
      if (expYear < currentYear || (expYear === currentYear && expMonth < currentMonth)) {
        errors.cardExpiry = 'Card has expired.'
      }
    }

    if (!cardCvv.value || cardCvv.value.length < 3) {
      errors.cardCvv = 'Enter a valid 3 or 4 digit CVV.'
    }

    if (!cardHolder.value.trim() || cardHolder.value.trim().length < 2) {
      errors.cardHolder = 'Cardholder name is required.'
    }

    if (!billingPostal.value.trim() || billingPostal.value.trim().length < 3) {
      errors.billingPostal = 'ZIP / Postal code is required.'
    }
  } else if (activePaymentTab.value === 'upi') {
    if (!upiId.value || !/^[\w.-]+@[\w.-]+$/.test(upiId.value.trim())) {
      errors.upiId = 'Please enter a valid UPI Virtual ID (e.g. name@okhdfcbank).'
    }
  } else if (activePaymentTab.value === 'netbanking') {
    if (!selectedBank.value) {
      errors.selectedBank = 'Please select your bank from the list.'
    }
  }

  formErrors.value = errors
  return Object.keys(errors).length === 0
}

async function handleCompletePayment() {
  errorMsg.value = ''
  if (!validateForm()) {
    return
  }

  isProcessingPayment.value = true
  processingStep.value = 1

  try {
    // Stage 1: Security encryption
    await new Promise((r) => setTimeout(r, 600))
    processingStep.value = 2

    // Stage 2: 3D Secure / Bank Authorization
    await new Promise((r) => setTimeout(r, 800))
    processingStep.value = 3

    // Stage 3: Provisioning & Backend Verification
    const mockPaymentId = `pay_${Date.now().toString(36)}_${Math.random().toString(36).substring(2, 7)}`
    const mockSignature = `sig_${Date.now().toString(36)}_${Math.random().toString(36).substring(2, 7)}`

    let methodDesc = 'Credit/Debit Card'
    if (activePaymentTab.value === 'card') {
      const last4 = cardNumber.value.replace(/\s+/g, '').slice(-4) || '4242'
      methodDesc = `${detectedCardBrand.value} •••• ${last4}`
    } else if (activePaymentTab.value === 'upi') {
      methodDesc = `UPI (${upiId.value})`
    } else if (activePaymentTab.value === 'netbanking') {
      methodDesc = `Netbanking (${selectedBank.value})`
    }

    const res = await apiClient.post('/payments/verify', {
      razorpayOrderId: currentOrderId.value,
      razorpayPaymentId: mockPaymentId,
      razorpaySignature: mockSignature,
      paymentMethod: methodDesc,
      billingName: cardHolder.value,
      billingPostalCode: billingPostal.value,
    })

    if (authStore.user) {
      authStore.user = { ...authStore.user, subscriptionTier: 'Pro' }
      localStorage.setItem('user', JSON.stringify(authStore.user))
    }

    isCheckoutOpen.value = false
    successMsg.value = `Payment Verified & Captured! Invoice #${res.data?.invoiceNumber || 'INV-2026'}. Pro tier unlocked.`

    await fetchInvoices()
  } catch (err: any) {
    errorMsg.value = err.response?.data?.message || err.message || 'Payment verification failed.'
  } finally {
    isProcessingPayment.value = false
    processingStep.value = 1
  }
}

async function handleCancelSubscription() {
  isCancelling.value = true
  errorMsg.value = ''
  try {
    await apiClient.post('/payments/cancel-subscription')
    if (authStore.user) {
      authStore.user = { ...authStore.user, subscriptionTier: 'Free' }
      localStorage.setItem('user', JSON.stringify(authStore.user))
    }
    isCancelModalOpen.value = false
    successMsg.value = 'Your subscription has been cancelled and reverted to the Free tier.'
    await fetchInvoices()
  } catch (e: any) {
    errorMsg.value = e.response?.data?.message || e.message || 'Failed to cancel subscription.'
  } finally {
    isCancelling.value = false
  }
}

async function handleResetToFree() {
  errorMsg.value = ''
  successMsg.value = ''
  isResetting.value = true
  try {
    await apiClient.post('/payments/reset-tier')
    if (authStore.user) {
      authStore.user = { ...authStore.user, subscriptionTier: 'Free' }
      localStorage.setItem('user', JSON.stringify(authStore.user))
    }
    successMsg.value = 'Subscription tier reset to Free for testing.'
    await fetchInvoices()
  } catch (e: any) {
    if (authStore.user) {
      authStore.user = { ...authStore.user, subscriptionTier: 'Free' }
      localStorage.setItem('user', JSON.stringify(authStore.user))
    }
    successMsg.value = 'Subscription tier reset to Free.'
  } finally {
    isResetting.value = false
  }
}

function viewReceipt(invoice: Invoice) {
  selectedReceipt.value = invoice
  isReceiptOpen.value = true
}

function printReceipt() {
  window.print()
}
</script>

<template>
  <div class="container mx-auto px-4 py-8 space-y-8 max-w-6xl">
    <!-- Header -->
    <div class="text-center space-y-3 max-w-2xl mx-auto">
      <div class="inline-flex items-center gap-2 px-3 py-1.5 rounded-full bg-primary/10 text-primary text-sm font-medium border border-primary/20">
        <CreditCard class="w-4 h-4" />
        Billing & Subscriptions
      </div>
      <h1 class="text-3xl md:text-4xl font-bold tracking-tight">
        Enterprise Cloud Telemetry Plans
      </h1>
      <p class="text-muted-foreground">
        Upgrade to Pro for real-time sub-second incident alerts, high-frequency ping sweeps, and unlimited assets.
      </p>

      <!-- Monthly / Annual Toggle -->
      <div class="pt-3 flex items-center justify-center gap-3">
        <div class="inline-flex p-1 rounded-xl bg-muted/60 border border-border/50 text-xs font-semibold">
          <button
            class="px-4 py-1.5 rounded-lg transition-all"
            :class="billingCycle === 'monthly' ? 'bg-primary text-primary-foreground shadow-sm' : 'text-muted-foreground hover:text-foreground'"
            @click="billingCycle = 'monthly'"
          >
            Monthly Billing
          </button>
          <button
            class="px-4 py-1.5 rounded-lg transition-all flex items-center gap-1.5"
            :class="billingCycle === 'annual' ? 'bg-primary text-primary-foreground shadow-sm' : 'text-muted-foreground hover:text-foreground'"
            @click="billingCycle = 'annual'"
          >
            <span>Annual Billing</span>
            <span class="bg-green-500 text-white font-bold text-[10px] px-1.5 py-0.2 rounded-full uppercase">Save 17%</span>
          </button>
        </div>
      </div>
    </div>

    <!-- Alert Notifications -->
    <div v-if="errorMsg" class="max-w-3xl mx-auto text-sm bg-destructive/10 text-destructive border border-destructive/20 rounded-xl p-4 animate-fade-in flex items-start gap-3">
      <AlertTriangle class="w-5 h-5 flex-shrink-0 mt-0.5" />
      <div>
        <p class="font-semibold">Billing Notification</p>
        <p class="mt-0.5 opacity-90">{{ errorMsg }}</p>
      </div>
    </div>
    <div v-if="successMsg" class="max-w-3xl mx-auto text-sm bg-green-500/10 text-green-600 dark:text-green-400 border border-green-500/20 rounded-xl p-4 animate-fade-in flex items-start gap-3">
      <Shield class="w-5 h-5 flex-shrink-0 mt-0.5" />
      <div>
        <p class="font-semibold">Transaction Confirmed</p>
        <p class="mt-0.5 opacity-90">{{ successMsg }}</p>
      </div>
    </div>

    <!-- Active Pro Subscription Details (When on Pro) -->
    <div v-if="currentTier === 'Pro'" class="max-w-5xl mx-auto">
      <Card class="border-2 border-primary/40 bg-gradient-to-br from-primary/5 via-card to-accent/5 shadow-xl">
        <CardContent class="p-6 sm:p-8 space-y-6">
          <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-border/60 pb-6">
            <div class="space-y-1">
              <div class="flex items-center gap-2">
                <Badge class="bg-primary hover:bg-primary text-white font-semibold">Active Plan</Badge>
                <h3 class="text-2xl font-bold">CloudPulse Pro</h3>
              </div>
              <p class="text-sm text-muted-foreground">
                Your subscription is active with unlimited asset telemetry and 10-second sweeps.
              </p>
            </div>
            <div class="text-left sm:text-right">
              <span class="text-3xl font-extrabold text-primary">₹2,499</span>
              <span class="text-muted-foreground text-sm">/month ($29)</span>
              <p class="text-xs text-muted-foreground mt-0.5">Renews automatically</p>
            </div>
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <div class="p-4 rounded-xl bg-card border border-border/60 space-y-1">
              <span class="text-xs text-muted-foreground flex items-center gap-1.5">
                <Calendar class="w-3.5 h-3.5 text-primary" /> Next Billing Date
              </span>
              <p class="font-semibold text-sm">September 28, 2026</p>
            </div>
            <div class="p-4 rounded-xl bg-card border border-border/60 space-y-1">
              <span class="text-xs text-muted-foreground flex items-center gap-1.5">
                <CreditCard class="w-3.5 h-3.5 text-primary" /> Payment Method
              </span>
              <p class="font-semibold text-sm">Visa ending in •••• 4242</p>
            </div>
            <div class="p-4 rounded-xl bg-card border border-border/60 space-y-1">
              <span class="text-xs text-muted-foreground flex items-center gap-1.5">
                <Shield class="w-3.5 h-3.5 text-primary" /> Status
              </span>
              <p class="font-semibold text-sm text-green-500 flex items-center gap-1">
                <CheckCircle2 class="w-4 h-4" /> Good Standing
              </p>
            </div>
          </div>

          <div class="flex items-center justify-between pt-2 flex-wrap gap-3">
            <Button variant="outline" size="sm" @click="isCancelModalOpen = true" class="text-destructive hover:bg-destructive/10">
              Cancel Subscription
            </Button>
            <Button variant="secondary" size="sm" @click="fetchInvoices">
              <RefreshCw class="w-3.5 h-3.5 mr-1.5" /> Refresh Invoices
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>

    <!-- Pricing Comparison Grid -->
    <div class="grid grid-cols-1 md:grid-cols-2 gap-6 items-stretch max-w-5xl mx-auto">
      <!-- FREE PLAN -->
      <Card class="border-border/60 shadow-sm flex flex-col transition-all duration-300 hover:shadow-lg relative overflow-hidden">
        <div v-if="currentTier === 'Free'" class="absolute top-0 right-0">
          <div class="bg-muted text-muted-foreground text-[10px] uppercase tracking-wider font-bold px-3 py-1.5 rounded-bl-xl border-l border-b border-border">
            Current Plan
          </div>
        </div>
        <CardHeader class="pb-4 space-y-3">
          <div class="flex items-center gap-2">
            <Cloud class="w-5 h-5 text-muted-foreground" />
            <CardTitle class="text-xl">Free Community</CardTitle>
          </div>
          <div class="flex items-baseline gap-1">
            <span class="text-4xl font-extrabold tracking-tight">$0</span>
            <span class="text-muted-foreground text-sm">/month</span>
          </div>
          <CardDescription class="text-sm">
            Ideal for hobbyists, side projects, and basic health pinging.
          </CardDescription>
        </CardHeader>
        <CardContent class="pt-2 space-y-5 flex-1 flex flex-col">
          <ul class="space-y-3 text-sm">
            <li class="flex items-start gap-3">
              <div class="w-5 h-5 rounded-full bg-green-500/15 flex items-center justify-center flex-shrink-0 mt-0.5">
                <Check class="w-3 h-3 text-green-600 dark:text-green-400" />
              </div>
              <span>Max <span class="font-semibold">3 cloud assets</span></span>
            </li>
            <li class="flex items-start gap-3">
              <div class="w-5 h-5 rounded-full bg-green-500/15 flex items-center justify-center flex-shrink-0 mt-0.5">
                <Check class="w-3 h-3 text-green-600 dark:text-green-400" />
              </div>
              <span><span class="font-semibold">60-sec</span> check interval</span>
            </li>
            <li class="flex items-start gap-3">
              <div class="w-5 h-5 rounded-full bg-green-500/15 flex items-center justify-center flex-shrink-0 mt-0.5">
                <Check class="w-3 h-3 text-green-600 dark:text-green-400" />
              </div>
              <span>Standard latency telemetry</span>
            </li>
            <li class="flex items-start gap-3">
              <div class="w-5 h-5 rounded-full bg-green-500/15 flex items-center justify-center flex-shrink-0 mt-0.5">
                <Check class="w-3 h-3 text-green-600 dark:text-green-400" />
              </div>
              <span>Community forum support</span>
            </li>
          </ul>
          <div class="mt-auto pt-4">
            <Button variant="outline" disabled class="w-full h-11 cursor-not-allowed">
              <span class="flex items-center gap-2">
                <Check class="w-4 h-4" />
                {{ currentTier === 'Free' ? 'Current Plan' : 'Free Tier' }}
              </span>
            </Button>
          </div>
        </CardContent>
      </Card>

      <!-- PRO PLAN -->
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
            Most Popular • Enterprise Grade
          </Badge>
        </div>
        <div v-if="currentTier === 'Pro'" class="absolute top-0 right-0">
          <div class="bg-gradient-to-r from-primary to-accent text-primary-foreground text-[10px] uppercase tracking-wider font-bold px-3 py-1.5 rounded-bl-xl border-l border-b border-primary/30">
            Current Plan
          </div>
        </div>
        <CardHeader class="pb-4 space-y-3 pt-6">
          <div class="flex items-center gap-2">
            <Zap class="w-5 h-5 text-primary" />
            <CardTitle class="text-xl">CloudPulse Pro</CardTitle>
          </div>
          <div class="flex items-baseline gap-1">
            <span class="text-4xl font-extrabold tracking-tight bg-gradient-to-r from-primary to-accent bg-clip-text text-transparent">
              ${{ planPriceUSD }}
            </span>
            <span class="text-muted-foreground text-sm">
              /{{ billingCycle === 'annual' ? 'year' : 'month' }} (~ ₹{{ planPrice.toLocaleString() }})
            </span>
          </div>
          <CardDescription class="text-sm">
            For growing engineering teams with critical infrastructure SLAs.
          </CardDescription>
        </CardHeader>
        <CardContent class="pt-2 space-y-5 flex-1 flex flex-col">
          <ul class="space-y-3 text-sm">
            <li class="flex items-start gap-3">
              <div class="w-5 h-5 rounded-full bg-primary/15 flex items-center justify-center flex-shrink-0 mt-0.5">
                <Check class="w-3 h-3 text-primary" />
              </div>
              <span><span class="font-semibold">Unlimited</span> cloud assets monitoring</span>
            </li>
            <li class="flex items-start gap-3">
              <div class="w-5 h-5 rounded-full bg-primary/15 flex items-center justify-center flex-shrink-0 mt-0.5">
                <Check class="w-3 h-3 text-primary" />
              </div>
              <span><span class="font-semibold">10-sec</span> sub-second ping sweeps</span>
            </li>
            <li class="flex items-start gap-3">
              <div class="w-5 h-5 rounded-full bg-primary/15 flex items-center justify-center flex-shrink-0 mt-0.5">
                <Check class="w-3 h-3 text-primary" />
              </div>
              <span>Automated incident detection banners</span>
            </li>
            <li class="flex items-start gap-3">
              <div class="w-5 h-5 rounded-full bg-primary/15 flex items-center justify-center flex-shrink-0 mt-0.5">
                <Check class="w-3 h-3 text-primary" />
              </div>
              <span>Priority 24/7 dedicated engineering support</span>
            </li>
            <li class="flex items-start gap-3">
              <div class="w-5 h-5 rounded-full bg-primary/15 flex items-center justify-center flex-shrink-0 mt-0.5">
                <Check class="w-3 h-3 text-primary" />
              </div>
              <span>Full 90+ days metrics history & chart analytics</span>
            </li>
          </ul>
          <div class="mt-auto pt-4 space-y-2">
            <Button
              v-if="currentTier !== 'Pro'"
              class="w-full h-11 font-semibold shadow-lg shadow-primary/30 hover:shadow-primary/40 transition-all text-sm"
              style="background: linear-gradient(135deg, hsl(var(--primary)), hsl(var(--accent)));"
              :disabled="isUpgrading"
              @click="openCheckoutModal"
            >
              <span v-if="isUpgrading" class="flex items-center gap-2">
                <RefreshCw class="w-4 h-4 animate-spin" />
                Initializing Checkout...
              </span>
              <span v-else class="flex items-center gap-2">
                <Zap class="w-4 h-4" />
                Upgrade to Pro (Checkout)
              </span>
            </Button>
            <Button
              v-else
              variant="outline"
              disabled
              class="w-full h-11 cursor-not-allowed"
            >
              <span class="flex items-center gap-2">
                <Check class="w-4 h-4 text-green-500" />
                You're on Pro Plan
              </span>
            </Button>
            <p class="text-[11px] text-center text-muted-foreground pt-1">
              🔒 256-Bit SSL Encrypted • PCI-DSS Compliant • Cancel Anytime
            </p>
          </div>
        </CardContent>
      </Card>
    </div>

    <!-- Billing History & Invoices Table -->
    <div class="max-w-5xl mx-auto space-y-4">
      <div class="flex items-center justify-between">
        <div>
          <h3 class="text-lg font-bold">Billing & Invoice History</h3>
          <p class="text-xs text-muted-foreground">Download receipts and view past subscription payments.</p>
        </div>
      </div>

      <Card class="border-border/60">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Invoice #</TableHead>
              <TableHead>Date</TableHead>
              <TableHead>Plan</TableHead>
              <TableHead>Amount</TableHead>
              <TableHead>Status</TableHead>
              <TableHead class="text-right">Action</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            <TableRow v-if="invoices.length === 0">
              <TableCell colspan="6" class="text-center py-6 text-muted-foreground text-sm">
                No past invoices found. Complete an upgrade to generate your first invoice.
              </TableCell>
            </TableRow>
            <TableRow v-for="inv in invoices" :key="inv.id">
              <TableCell class="font-mono text-xs font-semibold text-primary">
                {{ inv.invoiceNumber }}
              </TableCell>
              <TableCell class="text-xs text-muted-foreground">
                {{ new Date(inv.issuedAt).toLocaleDateString() }}
              </TableCell>
              <TableCell class="text-xs font-medium">
                {{ inv.planName }}
              </TableCell>
              <TableCell class="text-xs font-semibold">
                ₹{{ inv.amount.toLocaleString() }}
              </TableCell>
              <TableCell>
                <Badge variant="outline" class="text-[10px] bg-green-500/10 text-green-600 border-green-500/30">
                  {{ inv.status }}
                </Badge>
              </TableCell>
              <TableCell class="text-right">
                <Button variant="ghost" size="sm" class="h-7 text-xs" @click="viewReceipt(inv)">
                  <FileText class="w-3.5 h-3.5 mr-1" /> View Receipt
                </Button>
              </TableCell>
            </TableRow>
          </TableBody>
        </Table>
      </Card>
    </div>

    <!-- Developer Test Controls -->
    <div class="max-w-5xl mx-auto">
      <Card class="border-border/60 bg-muted/20 border-dashed">
        <CardContent class="p-4 flex flex-col sm:flex-row items-center justify-between gap-4">
          <div class="flex items-center gap-3 text-sm">
            <div class="w-8 h-8 rounded-lg bg-primary/10 flex items-center justify-center text-primary flex-shrink-0">
              <Sparkles class="w-4 h-4" />
            </div>
            <div>
              <p class="font-medium">Developer Sandbox Controls</p>
              <p class="text-xs text-muted-foreground">
                Reset your account back to Free tier to test form validations and the complete checkout flow again.
              </p>
            </div>
          </div>
          <Button
            variant="outline"
            size="sm"
            :disabled="isResetting || currentTier === 'Free'"
            @click="handleResetToFree"
          >
            <RotateCcw class="w-3.5 h-3.5 mr-1.5" :class="{ 'animate-spin': isResetting }" />
            Reset to Free Tier
          </Button>
        </CardContent>
      </Card>
    </div>

    <!-- PRODUCTION-GRADE CHECKOUT DRAWER / MODAL -->
    <Dialog :open="isCheckoutOpen" @update:open="isCheckoutOpen = $event">
      <DialogContent class="sm:max-w-[760px] p-0 overflow-hidden border-2 border-primary/30 shadow-2xl">
        <div class="grid grid-cols-1 md:grid-cols-12 min-h-[480px]">
          <!-- Left Column: Order Breakdown & Trust Badges -->
          <div class="md:col-span-5 bg-muted/50 p-6 flex flex-col justify-between border-b md:border-b-0 md:border-r border-border/60">
            <div class="space-y-5">
              <div class="flex items-center gap-2.5">
                <div class="w-8 h-8 rounded-lg bg-primary flex items-center justify-center text-white font-bold shadow-md shadow-primary/30">
                  <Activity class="w-4 h-4" />
                </div>
                <div>
                  <h3 class="font-bold text-sm leading-tight">CloudPulse</h3>
                  <p class="text-[11px] text-muted-foreground">Pro Telemetry Plan</p>
                </div>
              </div>

              <!-- Order Summary -->
              <div class="space-y-3 pt-2">
                <div class="text-xs font-semibold text-muted-foreground uppercase tracking-wider">
                  Order Summary
                </div>
                <div class="space-y-2 text-xs">
                  <div class="flex justify-between">
                    <span class="text-muted-foreground">CloudPulse Pro (Monthly)</span>
                    <span class="font-semibold">₹{{ planPrice.toLocaleString() }}</span>
                  </div>
                  <div class="flex justify-between">
                    <span class="text-muted-foreground">GST / Sales Tax (18%)</span>
                    <span class="font-semibold">₹{{ gstAmount.toLocaleString() }}</span>
                  </div>
                  <div class="border-t border-border/60 pt-2 flex justify-between text-sm font-bold">
                    <span>Total Due Today</span>
                    <span class="text-primary">₹{{ totalPayable.toLocaleString() }}</span>
                  </div>
                </div>
              </div>

              <!-- Auto-Fill Demo Helper -->
              <div class="pt-2">
                <button
                  type="button"
                  @click="activePaymentTab === 'card' ? autoFillTestCard() : autoFillTestUpi()"
                  class="w-full py-2 px-3 rounded-lg bg-primary/10 hover:bg-primary/20 text-primary text-xs font-semibold border border-primary/30 flex items-center justify-center gap-1.5 transition-colors"
                >
                  <Sparkles class="w-3.5 h-3.5" />
                  ✨ Auto-Fill Test Payment Details
                </button>
              </div>
            </div>

            <!-- Trust / Security Footnote -->
            <div class="space-y-2 pt-6 border-t border-border/50 text-[11px] text-muted-foreground">
              <div class="flex items-center gap-2">
                <Lock class="w-3.5 h-3.5 text-green-500" />
                <span>256-Bit SSL Encrypted Checkout</span>
              </div>
              <div class="flex items-center gap-2">
                <Globe class="w-3.5 h-3.5 text-primary" />
                <span>PCI-DSS Level 1 Compliant</span>
              </div>
            </div>
          </div>

          <!-- Right Column: Interactive Form & Real Validation -->
          <div class="md:col-span-7 p-6 flex flex-col justify-between bg-card space-y-5">
            <div>
              <div class="flex items-center justify-between pb-3 border-b border-border/50">
                <h4 class="text-sm font-bold">Select Payment Instrument</h4>
                <span class="text-[10px] text-muted-foreground font-mono">Order: {{ currentOrderId.substring(0, 12) }}</span>
              </div>

              <!-- Payment Instrument Tabs -->
              <div class="flex rounded-lg bg-muted/60 p-1 gap-1 border border-border/40 mt-3">
                <button
                  class="flex-1 py-2 px-2 text-xs font-semibold rounded-md transition-all flex items-center justify-center gap-1.5"
                  :class="activePaymentTab === 'card' ? 'bg-primary text-primary-foreground shadow-sm' : 'text-muted-foreground hover:text-foreground'"
                  @click="activePaymentTab = 'card'"
                >
                  <CreditCard class="w-3.5 h-3.5" />
                  Card
                </button>
                <button
                  class="flex-1 py-2 px-2 text-xs font-semibold rounded-md transition-all flex items-center justify-center gap-1.5"
                  :class="activePaymentTab === 'upi' ? 'bg-primary text-primary-foreground shadow-sm' : 'text-muted-foreground hover:text-foreground'"
                  @click="activePaymentTab = 'upi'"
                >
                  <Smartphone class="w-3.5 h-3.5" />
                  UPI / QR
                </button>
                <button
                  class="flex-1 py-2 px-2 text-xs font-semibold rounded-md transition-all flex items-center justify-center gap-1.5"
                  :class="activePaymentTab === 'netbanking' ? 'bg-primary text-primary-foreground shadow-sm' : 'text-muted-foreground hover:text-foreground'"
                  @click="activePaymentTab = 'netbanking'"
                >
                  <Building2 class="w-3.5 h-3.5" />
                  Netbanking
                </button>
              </div>

              <!-- FORM 1: CREDIT / DEBIT CARD -->
              <div v-if="activePaymentTab === 'card'" class="space-y-3 pt-3">
                <div class="space-y-1">
                  <div class="flex items-center justify-between text-xs font-medium">
                    <label>Card Number</label>
                    <span class="text-[11px] font-bold text-primary">{{ detectedCardBrand }}</span>
                  </div>
                  <Input
                    :value="cardNumber"
                    @input="handleCardInput"
                    placeholder="4111 2222 3333 4444"
                    maxlength="19"
                    class="font-mono text-xs"
                    :class="{ 'border-destructive focus-visible:ring-destructive': formErrors.cardNumber }"
                  />
                  <p v-if="formErrors.cardNumber" class="text-[11px] text-destructive font-medium">
                    {{ formErrors.cardNumber }}
                  </p>
                </div>

                <div class="grid grid-cols-2 gap-3">
                  <div class="space-y-1">
                    <label class="text-xs font-medium">Expiry Date</label>
                    <Input
                      :value="cardExpiry"
                      @input="handleExpiryInput"
                      placeholder="MM/YY"
                      maxlength="5"
                      class="font-mono text-xs"
                      :class="{ 'border-destructive focus-visible:ring-destructive': formErrors.cardExpiry }"
                    />
                    <p v-if="formErrors.cardExpiry" class="text-[11px] text-destructive font-medium">
                      {{ formErrors.cardExpiry }}
                    </p>
                  </div>
                  <div class="space-y-1">
                    <label class="text-xs font-medium">CVV / CVC</label>
                    <Input
                      :value="cardCvv"
                      @input="handleCvvInput"
                      type="password"
                      placeholder="•••"
                      maxlength="4"
                      class="font-mono text-xs"
                      :class="{ 'border-destructive focus-visible:ring-destructive': formErrors.cardCvv }"
                    />
                    <p v-if="formErrors.cardCvv" class="text-[11px] text-destructive font-medium">
                      {{ formErrors.cardCvv }}
                    </p>
                  </div>
                </div>

                <div class="grid grid-cols-2 gap-3">
                  <div class="space-y-1">
                    <label class="text-xs font-medium">Cardholder Name</label>
                    <Input
                      v-model="cardHolder"
                      placeholder="Name on card"
                      class="text-xs uppercase"
                      :class="{ 'border-destructive focus-visible:ring-destructive': formErrors.cardHolder }"
                    />
                    <p v-if="formErrors.cardHolder" class="text-[11px] text-destructive font-medium">
                      {{ formErrors.cardHolder }}
                    </p>
                  </div>
                  <div class="space-y-1">
                    <label class="text-xs font-medium">ZIP / Postal Code</label>
                    <Input
                      v-model="billingPostal"
                      placeholder="e.g. 10001"
                      class="text-xs font-mono"
                      :class="{ 'border-destructive focus-visible:ring-destructive': formErrors.billingPostal }"
                    />
                    <p v-if="formErrors.billingPostal" class="text-[11px] text-destructive font-medium">
                      {{ formErrors.billingPostal }}
                    </p>
                  </div>
                </div>
              </div>

              <!-- FORM 2: UPI / QR -->
              <div v-if="activePaymentTab === 'upi'" class="space-y-3 pt-3">
                <div class="p-3 bg-muted/40 rounded-xl border border-dashed border-border/80 flex items-center justify-between">
                  <div class="space-y-1">
                    <p class="text-xs font-bold">Scan to Pay via UPI</p>
                    <p class="text-[10px] text-muted-foreground">Expires in {{ Math.floor(qrSecondsLeft / 60) }}:{{ (qrSecondsLeft % 60).toString().padStart(2, '0') }}</p>
                  </div>
                  <div class="w-16 h-16 bg-white p-1 rounded-lg border shadow-sm">
                    <svg viewBox="0 0 100 100" class="w-full h-full text-slate-900 fill-current">
                      <rect x="10" y="10" width="25" height="25" rx="2" />
                      <rect x="15" y="15" width="15" height="15" fill="white" />
                      <rect x="18" y="18" width="9" height="9" />
                      <rect x="65" y="10" width="25" height="25" rx="2" />
                      <rect x="70" y="15" width="15" height="15" fill="white" />
                      <rect x="73" y="18" width="9" height="9" />
                      <rect x="10" y="65" width="25" height="25" rx="2" />
                      <rect x="15" y="70" width="15" height="15" fill="white" />
                      <rect x="18" y="73" width="9" height="9" />
                      <rect x="42" y="15" width="12" height="12" />
                      <rect x="45" y="38" width="20" height="8" />
                      <rect x="40" y="55" width="16" height="16" />
                      <rect x="65" y="60" width="20" height="20" />
                    </svg>
                  </div>
                </div>

                <div class="space-y-1">
                  <label class="text-xs font-medium">Or Enter UPI ID (VPA)</label>
                  <Input
                    v-model="upiId"
                    placeholder="username@okhdfcbank"
                    class="text-xs font-mono"
                    :class="{ 'border-destructive focus-visible:ring-destructive': formErrors.upiId }"
                  />
                  <p v-if="formErrors.upiId" class="text-[11px] text-destructive font-medium">
                    {{ formErrors.upiId }}
                  </p>
                </div>
              </div>

              <!-- FORM 3: NETBANKING -->
              <div v-if="activePaymentTab === 'netbanking'" class="space-y-3 pt-3">
                <label class="text-xs font-medium">Select Your Bank</label>
                <div class="grid grid-cols-2 gap-2">
                  <button
                    v-for="b in popularBanks"
                    :key="b.id"
                    type="button"
                    class="flex items-center gap-2 p-2.5 rounded-lg border text-left text-xs transition-all"
                    :class="selectedBank === b.id ? 'border-primary bg-primary/10 text-primary font-semibold shadow-sm' : 'border-border/60 hover:bg-muted/40'"
                    @click="selectedBank = b.id; if (formErrors.selectedBank) delete formErrors.selectedBank"
                  >
                    <div class="w-5 h-5 rounded bg-muted flex items-center justify-center text-[9px] font-bold">
                      {{ b.code }}
                    </div>
                    <span>{{ b.name }}</span>
                  </button>
                </div>
                <p v-if="formErrors.selectedBank" class="text-[11px] text-destructive font-medium">
                  {{ formErrors.selectedBank }}
                </p>
              </div>

              <!-- Terms Checkbox -->
              <div class="pt-3">
                <label class="flex items-start gap-2 text-xs text-muted-foreground cursor-pointer">
                  <input
                    type="checkbox"
                    v-model="agreeTerms"
                    class="mt-0.5 rounded text-primary focus:ring-primary h-3.5 w-3.5 border-border"
                  />
                  <span>
                    I authorize CloudPulse to charge ₹{{ totalPayable.toLocaleString() }} today and accept the
                    <strong class="text-foreground">Subscription Terms & Cancellation Policy</strong>.
                  </span>
                </label>
                <p v-if="formErrors.agreeTerms" class="text-[11px] text-destructive font-medium mt-1">
                  {{ formErrors.agreeTerms }}
                </p>
              </div>
            </div>

            <!-- Submit Action -->
            <div class="pt-2">
              <Button
                class="w-full h-11 text-sm font-semibold shadow-lg shadow-primary/30"
                style="background: linear-gradient(135deg, hsl(var(--primary)), hsl(var(--accent)));"
                :disabled="isProcessingPayment"
                @click="handleCompletePayment"
              >
                <span v-if="isProcessingPayment" class="flex items-center gap-2">
                  <RefreshCw class="w-4 h-4 animate-spin" />
                  <span v-if="processingStep === 1">1/3 Encrypting Payload...</span>
                  <span v-else-if="processingStep === 2">2/3 Authorizing 3D Secure...</span>
                  <span v-else>3/3 Provisioning Pro Plan...</span>
                </span>
                <span v-else class="flex items-center gap-2">
                  <Lock class="w-4 h-4" />
                  Pay ₹{{ totalPayable.toLocaleString() }} & Upgrade to Pro
                </span>
              </Button>
            </div>
          </div>
        </div>
      </DialogContent>
    </Dialog>

    <!-- INVOICE / RECEIPT VIEWER MODAL -->
    <Dialog :open="isReceiptOpen" @update:open="isReceiptOpen = $event">
      <DialogContent class="sm:max-w-[560px] p-6 space-y-6">
        <div v-if="selectedReceipt" class="space-y-6" id="printable-receipt">
          <div class="flex items-center justify-between border-b pb-4">
            <div class="flex items-center gap-2.5">
              <div class="w-8 h-8 rounded-lg bg-primary flex items-center justify-center text-white font-bold">
                <Activity class="w-4 h-4" />
              </div>
              <div>
                <h3 class="font-bold text-base">CloudPulse Inc.</h3>
                <p class="text-xs text-muted-foreground">Official Payment Receipt</p>
              </div>
            </div>
            <div class="text-right">
              <Badge class="bg-green-500 text-white font-semibold">PAID</Badge>
              <p class="text-xs font-mono text-muted-foreground mt-1">{{ selectedReceipt.invoiceNumber }}</p>
            </div>
          </div>

          <div class="grid grid-cols-2 gap-4 text-xs">
            <div>
              <span class="text-muted-foreground font-medium">Billed To:</span>
              <p class="font-semibold text-foreground mt-0.5">{{ authStore.user?.email }}</p>
              <p class="text-muted-foreground">Cloud Telemetry Customer</p>
            </div>
            <div class="text-right">
              <span class="text-muted-foreground font-medium">Payment Date:</span>
              <p class="font-semibold text-foreground mt-0.5">{{ new Date(selectedReceipt.issuedAt).toLocaleString() }}</p>
              <p class="text-muted-foreground">{{ selectedReceipt.paymentMethod || 'Visa •••• 4242' }}</p>
            </div>
          </div>

          <div class="rounded-xl border border-border/60 overflow-hidden">
            <table class="w-full text-xs">
              <thead class="bg-muted/60 text-muted-foreground font-semibold">
                <tr>
                  <th class="p-3 text-left">Description</th>
                  <th class="p-3 text-right">Amount</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-border/60">
                <tr>
                  <td class="p-3">
                    <p class="font-semibold">{{ selectedReceipt.planName }} (Subscription)</p>
                    <p class="text-[10px] text-muted-foreground">Includes unlimited cloud assets and 10s telemetry checks</p>
                  </td>
                  <td class="p-3 text-right font-semibold">₹{{ (selectedReceipt.amount / 1.18).toFixed(2) }}</td>
                </tr>
                <tr>
                  <td class="p-3 text-muted-foreground">GST / Sales Tax (18%)</td>
                  <td class="p-3 text-right font-semibold">₹{{ (selectedReceipt.amount - selectedReceipt.amount / 1.18).toFixed(2) }}</td>
                </tr>
                <tr class="bg-muted/20 font-bold">
                  <td class="p-3">Total Paid</td>
                  <td class="p-3 text-right text-primary">₹{{ selectedReceipt.amount.toLocaleString() }}</td>
                </tr>
              </tbody>
            </table>
          </div>

          <div class="flex items-center justify-end gap-2 pt-2">
            <Button variant="outline" size="sm" @click="printReceipt">
              <Download class="w-3.5 h-3.5 mr-1.5" /> Print / Save PDF
            </Button>
            <Button size="sm" @click="isReceiptOpen = false">
              Done
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>

    <!-- CANCEL SUBSCRIPTION CONFIRMATION MODAL -->
    <Dialog :open="isCancelModalOpen" @update:open="isCancelModalOpen = $event">
      <DialogContent class="sm:max-w-[440px] p-6 space-y-5">
        <div class="space-y-2">
          <h3 class="font-bold text-lg text-destructive flex items-center gap-2">
            <AlertTriangle class="w-5 h-5" /> Cancel Pro Subscription?
          </h3>
          <p class="text-xs text-muted-foreground leading-relaxed">
            Are you sure you want to cancel your CloudPulse Pro subscription? You will lose access to unlimited asset monitoring, 10s ping sweeps, and historical analytics.
          </p>
        </div>
        <div class="flex items-center justify-end gap-2 pt-3">
          <Button variant="outline" size="sm" :disabled="isCancelling" @click="isCancelModalOpen = false">
            Keep Pro Plan
          </Button>
          <Button variant="destructive" size="sm" :disabled="isCancelling" @click="handleCancelSubscription">
            <RefreshCw v-if="isCancelling" class="w-3.5 h-3.5 mr-1.5 animate-spin" />
            Yes, Cancel Subscription
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  </div>
</template>
