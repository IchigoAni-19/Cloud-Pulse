<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { Server, Database, Cpu, Workflow, Plus, Check, AlertTriangle } from '@lucide/vue'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { useAssetsStore } from '@/stores/assets'
import { useAuthStore } from '@/stores/auth'
import type { ResourceType, Environment } from '@/stores/assets'
import { cn } from '@/lib/utils'

const props = defineProps<{
  open: boolean
}>()

const emit = defineEmits<{
  (e: 'update:open', value: boolean): void
}>()

const assetsStore = useAssetsStore()
const authStore = useAuthStore()

const name = ref('')
const targetUrl = ref('')
const resourceType = ref<ResourceType>('API')
const environment = ref<Environment>('Production')
const checkIntervalSeconds = ref<number>(60)

const errorMsg = ref('')
const urlError = ref('')

const isFreeTier = computed(() => authStore.user?.subscriptionTier !== 'Pro')
const quotaExceeded = computed(() => isFreeTier.value && assetsStore.assets.length >= 3)

watch(
  () => props.open,
  (v) => {
    if (v) {
      name.value = ''
      targetUrl.value = ''
      resourceType.value = 'API'
      environment.value = 'Production'
      checkIntervalSeconds.value = 60
      errorMsg.value = ''
      urlError.value = ''
    }
  }
)

function validateUrl(url: string): boolean {
  if (!url) return true
  try {
    new URL(url)
    return true
  } catch {
    return false
  }
}

const resourceOptions: { value: ResourceType; icon: any; label: string }[] = [
  { value: 'API', icon: Server, label: 'API' },
  { value: 'Database', icon: Database, label: 'Database' },
  { value: 'VM', icon: Cpu, label: 'VM' },
  { value: 'Worker', icon: Workflow, label: 'Worker' },
]

const envOptions: Environment[] = ['Production', 'Staging', 'Development']

async function handleSubmit() {
  errorMsg.value = ''
  urlError.value = ''

  if (!name.value.trim()) {
    errorMsg.value = 'Asset name is required'
    return
  }
  if (!validateUrl(targetUrl.value)) {
    urlError.value = 'Please enter a valid URL (e.g. https://api.example.com)'
    return
  }
  if (!targetUrl.value.trim()) {
    errorMsg.value = 'Target URL is required'
    return
  }
  if (quotaExceeded.value) {
    errorMsg.value = 'Free tier maximum of 3 assets exceeded. Upgrade to Pro for unlimited assets.'
    return
  }
  if (!checkIntervalSeconds.value || checkIntervalSeconds.value < 10) {
    checkIntervalSeconds.value = 60
  }

  try {
    await assetsStore.createAsset({
      name: name.value.trim(),
      targetUrl: targetUrl.value.trim(),
      resourceType: resourceType.value,
      environment: environment.value,
      checkIntervalSeconds: checkIntervalSeconds.value,
    })
    emit('update:open', false)
  } catch (e: any) {
    errorMsg.value = e.message || 'Failed to create asset'
  }
}

function optBtn(selected: boolean) {
  return cn(
    'flex-1 px-3 py-2.5 rounded-lg border text-sm font-medium transition-all duration-150 flex items-center justify-center gap-2',
    selected
      ? 'border-primary bg-primary/10 text-primary shadow-sm'
      : 'border-border hover:border-primary/50 hover:bg-muted/50'
  )
}
</script>

<template>
  <Dialog :open="open" @update:open="(v) => emit('update:open', v)">
    <DialogContent class="sm:max-w-lg">
      <DialogHeader>
        <DialogTitle class="flex items-center gap-2">
          <Plus class="w-5 h-5 text-primary" />
          Register Cloud Asset
        </DialogTitle>
        <DialogDescription>
          Add a new cloud asset to start monitoring its health and uptime.
        </DialogDescription>
      </DialogHeader>

      <div class="space-y-5 py-2">
        <div v-if="quotaExceeded" class="rounded-xl border border-destructive/30 bg-destructive/5 p-3 flex items-start gap-3 text-sm">
          <AlertTriangle class="w-5 h-5 text-destructive flex-shrink-0 mt-0.5" />
          <div>
            <p class="font-medium text-destructive">Free Tier Quota Exceeded</p>
            <p class="text-muted-foreground text-xs mt-0.5">
              You have reached the maximum of 3 assets on the Free plan. Upgrade to Pro for unlimited assets.
            </p>
          </div>
        </div>

        <div v-if="errorMsg" class="text-sm bg-destructive/10 text-destructive border border-destructive/20 rounded-lg p-3">
          {{ errorMsg }}
        </div>

        <div class="space-y-2">
          <label class="text-sm font-medium">Asset Name</label>
          <Input
            v-model="name"
            placeholder="e.g. Production API Gateway"
            class="h-11"
          />
        </div>

        <div class="space-y-2">
          <label class="text-sm font-medium">Target URL</label>
          <Input
            v-model="targetUrl"
            :class="{ 'border-destructive focus-visible:ring-destructive': urlError }"
            type="url"
            placeholder="https://api.example.com/health"
            class="h-11"
          />
          <p v-if="urlError" class="text-xs text-destructive">{{ urlError }}</p>
        </div>

        <div class="space-y-2">
          <label class="text-sm font-medium">Resource Type</label>
          <div class="grid grid-cols-4 gap-2">
            <button
              v-for="opt in resourceOptions"
              :key="opt.value"
              type="button"
              :class="optBtn(resourceType === opt.value)"
              @click="resourceType = opt.value"
            >
              <component :is="opt.icon" class="w-4 h-4" />
              <span class="hidden sm:inline">{{ opt.label }}</span>
            </button>
          </div>
        </div>

        <div class="space-y-2">
          <label class="text-sm font-medium">Environment</label>
          <div class="grid grid-cols-3 gap-2">
            <button
              v-for="env in envOptions"
              :key="env"
              type="button"
              :class="optBtn(environment === env)"
              @click="environment = env"
            >
              {{ env }}
            </button>
          </div>
        </div>

        <div class="space-y-2">
          <div class="flex items-center justify-between">
            <label class="text-sm font-medium">Check Interval (seconds)</label>
            <Badge variant="outline" class="text-xs">
              {{ isFreeTier ? 'Min 60s (Free)' : 'Min 10s (Pro)' }}
            </Badge>
          </div>
          <Input
            v-model.number="checkIntervalSeconds"
            :min="isFreeTier ? 60 : 10"
            type="number"
            class="h-11"
          />
        </div>
      </div>

      <DialogFooter>
        <Button
          variant="ghost"
          @click="emit('update:open', false)"
        >
          Cancel
        </Button>
        <Button
          :disabled="quotaExceeded || assetsStore.isLoading"
          @click="handleSubmit"
          class="gap-2"
        >
          <span v-if="assetsStore.isLoading" class="flex items-center gap-2">
            <svg class="w-4 h-4 animate-spin" viewBox="0 0 24 24" fill="none">
              <circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="3" class="opacity-25" />
              <path d="M12 2a10 10 0 0110 10" stroke="currentColor" stroke-width="3" stroke-linecap="round" />
            </svg>
            Creating...
          </span>
          <span v-else class="flex items-center gap-2">
            <Check class="w-4 h-4" />
            Create Asset
          </span>
        </Button>
      </DialogFooter>
    </DialogContent>
  </Dialog>
</template>
