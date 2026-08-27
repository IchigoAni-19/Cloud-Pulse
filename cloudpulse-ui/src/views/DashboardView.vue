<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import {
  Activity,
  AlertTriangle,
  Check,
  X,
  RefreshCw,
  Plus,
  Trash2,
  Eye,
  Server,
  Database,
  Cpu,
  Workflow,
  Cloud,
  Zap,
  Clock,
} from '@lucide/vue'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { useAssetsStore } from '@/stores/assets'
import AddAssetModal from '@/components/dashboard/AddAssetModal.vue'
import { cn } from '@/lib/utils'
import type { AssetStatus, Environment } from '@/stores/assets'

const router = useRouter()
const assetsStore = useAssetsStore()

const addAssetOpen = ref(false)
const activeFilter = ref<Environment | 'All'>('All')
const deleteConfirmId = ref<string | null>(null)

onMounted(async () => {
  try {
    await Promise.all([
      assetsStore.fetchAssets(),
      assetsStore.fetchDashboardMetrics(),
    ])
  } catch (e) {
    // silently ignore - backend may not be running
  }
})

const downAssetsCount = computed(() =>
  assetsStore.assets.filter(a => a.currentStatus === 'Down').length
)

async function handleFilterChange(env: Environment | 'All') {
  activeFilter.value = env
  try {
    if (env === 'All') {
      await assetsStore.fetchAssets()
    } else {
      await assetsStore.fetchAssets({ env })
    }
  } catch (e) {
    // ignore
  }
}

function statusBadge(status: AssetStatus) {
  switch (status) {
    case 'Healthy':
      return { variant: 'default' as const, cls: 'bg-green-500 hover:bg-green-500', label: 'Healthy', dot: 'bg-green-500' }
    case 'Degraded':
      return { variant: 'secondary' as const, cls: 'bg-yellow-500/20 text-yellow-600 dark:text-yellow-400 border-yellow-500/30', label: 'Degraded', dot: 'bg-yellow-500' }
    case 'Down':
      return { variant: 'destructive' as const, cls: '', label: 'Down', dot: 'bg-red-500 animate-pulse' }
    default:
      return { variant: 'outline' as const, cls: '', label: 'Unknown', dot: 'bg-gray-400' }
  }
}

const resourceIcon = (type: string) => {
  switch (type) {
    case 'API': return Server
    case 'Database': return Database
    case 'VM': return Cpu
    case 'Worker': return Workflow
    default: return Cloud
  }
}

function formatAgo(timestamp?: string): string {
  if (!timestamp) return 'Never'
  const diff = Date.now() - new Date(timestamp).getTime()
  const sec = Math.floor(diff / 1000)
  if (sec < 60) return `${sec}s ago`
  const min = Math.floor(sec / 60)
  if (min < 60) return `${min}m ago`
  const hr = Math.floor(min / 60)
  if (hr < 24) return `${hr}h ago`
  const day = Math.floor(hr / 24)
  return `${day}d ago`
}

function truncateUrl(url: string, max = 50) {
  if (url.length <= max) return url
  return url.slice(0, max - 3) + '...'
}

async function handlePing(id: string) {
  try {
    await assetsStore.pingAsset(id)
  } catch (e) {
    // ignore
  }
}

async function handleDelete(id: string) {
  try {
    await assetsStore.deleteAsset(id)
    deleteConfirmId.value = null
  } catch (e) {
    // ignore
  }
}

const filterBtn = (val: Environment | 'All') =>
  cn(
    'px-4 py-2 rounded-lg text-sm font-medium transition-all duration-150 border',
    activeFilter.value === val
      ? 'border-primary bg-primary/10 text-primary shadow-sm'
      : 'border-border hover:border-primary/40 hover:bg-muted/50'
  )
</script>

<template>
  <div class="container mx-auto px-4 py-8 space-y-6 max-w-7xl">
    <div class="flex items-end justify-between flex-wrap gap-4">
      <div>
        <h1 class="text-2xl md:text-3xl font-bold tracking-tight flex items-center gap-2">
          <Activity class="w-7 h-7 text-primary" />
          Dashboard
        </h1>
        <p class="text-muted-foreground mt-1 text-sm">
          Monitor the health and uptime of all your cloud assets.
        </p>
      </div>
      <Button class="gap-2 h-11" @click="addAssetOpen = true">
        <Plus class="w-4 h-4" />
        Register Cloud Asset
      </Button>
    </div>

    <div
      v-if="downAssetsCount > 0"
      class="rounded-xl overflow-hidden animate-pulse relative"
      style="box-shadow: 0 0 0 2px hsl(var(--destructive) / 0.4), 0 0 30px hsl(var(--destructive) / 0.2);"
    >
      <div
        class="px-5 py-4 flex items-center gap-4"
        style="background: linear-gradient(135deg, hsl(var(--destructive) / 0.15), hsl(var(--destructive) / 0.05));"
      >
        <div class="w-11 h-11 rounded-xl bg-destructive/20 flex items-center justify-center flex-shrink-0">
          <AlertTriangle class="w-6 h-6 text-destructive" />
        </div>
        <div class="flex-1">
          <h3 class="font-semibold text-destructive flex items-center gap-2">
            Active Incident Detected
            <Badge variant="destructive" class="text-[10px] h-4">{{ downAssetsCount }}</Badge>
          </h3>
          <p class="text-sm text-destructive/80 mt-0.5">
            {{ downAssetsCount }} service{{ downAssetsCount > 1 ? 's' : '' }} currently reporting Down status.
          </p>
        </div>
      </div>
    </div>

    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <Card class="border-border/50 shadow-sm hover:shadow-md transition-shadow duration-200">
        <CardContent class="p-5">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm text-muted-foreground">Total Assets</p>
              <p class="text-3xl font-bold mt-1">
                {{ assetsStore.summaryMetrics.totalAssets || assetsStore.assets.length || 0 }}
              </p>
            </div>
            <div class="w-11 h-11 rounded-xl bg-primary/15 flex items-center justify-center">
              <Cloud class="w-6 h-6 text-primary" />
            </div>
          </div>
          <div class="mt-3 h-1 w-full bg-muted rounded-full overflow-hidden">
            <div class="h-full bg-primary/70 rounded-full" style="width: 100%;" />
          </div>
        </CardContent>
      </Card>

      <Card class="border-border/50 shadow-sm hover:shadow-md transition-shadow duration-200">
        <CardContent class="p-5">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm text-muted-foreground">Global Uptime</p>
              <p class="text-3xl font-bold mt-1">
                {{ (assetsStore.summaryMetrics.overallUptimePercentage || 0).toFixed(2) }}%
              </p>
            </div>
            <div class="w-11 h-11 rounded-xl bg-green-500/15 flex items-center justify-center">
              <Zap class="w-6 h-6 text-green-600 dark:text-green-400" />
            </div>
          </div>
          <div class="mt-3 h-1 w-full bg-muted rounded-full overflow-hidden">
            <div
              class="h-full bg-green-500 rounded-full transition-all duration-500"
              :style="{ width: `${assetsStore.summaryMetrics.overallUptimePercentage || 0}%` }"
            />
          </div>
        </CardContent>
      </Card>

      <Card class="border-border/50 shadow-sm hover:shadow-md transition-shadow duration-200">
        <CardContent class="p-5">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm text-muted-foreground">Healthy</p>
              <p class="text-3xl font-bold mt-1 text-green-600 dark:text-green-400">
                {{ assetsStore.summaryMetrics.healthyCount || assetsStore.assets.filter(a => a.currentStatus === 'Healthy').length || 0 }}
              </p>
            </div>
            <div class="w-11 h-11 rounded-xl bg-green-500/15 flex items-center justify-center">
              <Check class="w-6 h-6 text-green-600 dark:text-green-400" />
            </div>
          </div>
          <div class="mt-3 h-1 w-full bg-muted rounded-full overflow-hidden">
            <div
              class="h-full bg-green-500 rounded-full transition-all duration-500"
              :style="{ width: `${Math.min(100, (assetsStore.summaryMetrics.healthyCount / Math.max(1, assetsStore.summaryMetrics.totalAssets || assetsStore.assets.length)) * 100)}%` }"
            />
          </div>
        </CardContent>
      </Card>

      <Card class="border-border/50 shadow-sm hover:shadow-md transition-shadow duration-200">
        <CardContent class="p-5">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm text-muted-foreground">Critical / Down</p>
              <p class="text-3xl font-bold mt-1 text-destructive flex items-center gap-2">
                <span
                  :class="downAssetsCount > 0 ? 'animate-pulse' : ''"
                >
                  {{ assetsStore.summaryMetrics.criticalCount || downAssetsCount || 0 }}
                </span>
              </p>
            </div>
            <div class="w-11 h-11 rounded-xl bg-destructive/15 flex items-center justify-center">
              <X class="w-6 h-6 text-destructive" />
            </div>
          </div>
          <div class="mt-3 h-1 w-full bg-muted rounded-full overflow-hidden">
            <div
              class="h-full bg-destructive rounded-full transition-all duration-500"
              :style="{ width: `${Math.min(100, ((assetsStore.summaryMetrics.criticalCount || downAssetsCount) / Math.max(1, assetsStore.summaryMetrics.totalAssets || assetsStore.assets.length)) * 100)}%` }"
            />
          </div>
        </CardContent>
      </Card>
    </div>

    <div class="flex flex-wrap items-center gap-2 justify-between">
      <div class="flex items-center gap-2 flex-wrap">
        <button :class="filterBtn('All')" @click="handleFilterChange('All')">
          All
        </button>
        <button :class="filterBtn('Production')" @click="handleFilterChange('Production')">
          Production
        </button>
        <button :class="filterBtn('Staging')" @click="handleFilterChange('Staging')">
          Staging
        </button>
        <button :class="filterBtn('Development')" @click="handleFilterChange('Development')">
          Development
        </button>
      </div>
      <Button variant="ghost" size="sm" class="gap-1.5 text-muted-foreground" :disabled="assetsStore.isLoading" @click="() => { handleFilterChange(activeFilter) }">
        <RefreshCw :class="['w-4 h-4', assetsStore.isLoading && 'animate-spin']" />
        Refresh
      </Button>
    </div>

    <Card class="border-border/50 shadow-sm overflow-hidden">
      <div class="overflow-x-auto">
        <Table>
          <TableHeader class="bg-muted/40">
            <TableRow>
              <TableHead class="font-semibold">Name</TableHead>
              <TableHead class="font-semibold">URL</TableHead>
              <TableHead class="font-semibold">Status</TableHead>
              <TableHead class="font-semibold">Latency</TableHead>
              <TableHead class="font-semibold">Last Checked</TableHead>
              <TableHead class="text-right font-semibold w-[80px]">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            <TableRow v-if="assetsStore.isLoading && assetsStore.assets.length === 0">
              <TableCell :colspan="6" class="text-center py-12 text-muted-foreground">
                <div class="flex flex-col items-center gap-3">
                  <RefreshCw class="w-8 h-8 animate-spin text-primary" />
                  <p>Loading assets...</p>
                </div>
              </TableCell>
            </TableRow>
            <TableRow v-else-if="assetsStore.assets.length === 0">
              <TableCell :colspan="6" class="text-center py-16">
                <div class="flex flex-col items-center gap-3">
                  <div class="w-16 h-16 rounded-2xl bg-muted flex items-center justify-center">
                    <Cloud class="w-9 h-9 text-muted-foreground" />
                  </div>
                  <div>
                    <p class="font-medium">No assets registered yet</p>
                    <p class="text-sm text-muted-foreground mt-1">
                      Register your first cloud asset to start monitoring.
                    </p>
                  </div>
                  <Button class="mt-2 gap-2" @click="addAssetOpen = true">
                    <Plus class="w-4 h-4" />
                    Register Asset
                  </Button>
                </div>
              </TableCell>
            </TableRow>
            <TableRow v-for="asset in assetsStore.assets" :key="asset.id" class="group transition-colors hover:bg-muted/30">
              <TableCell>
                <div class="flex items-center gap-3">
                  <div class="w-9 h-9 rounded-lg bg-primary/10 flex items-center justify-center flex-shrink-0">
                    <component :is="resourceIcon(asset.resourceType)" class="w-4.5 h-4.5 text-primary" />
                  </div>
                  <div class="min-w-0">
                    <div class="font-medium truncate max-w-[200px]">{{ asset.name }}</div>
                    <div class="flex items-center gap-1.5 mt-0.5">
                      <Badge variant="outline" class="text-[10px] h-4 px-1.5">{{ asset.resourceType }}</Badge>
                      <Badge variant="secondary" class="text-[10px] h-4 px-1.5">{{ asset.environment }}</Badge>
                    </div>
                  </div>
                </div>
              </TableCell>
              <TableCell>
                <span class="font-mono text-xs text-muted-foreground truncate max-w-[260px] inline-block align-middle" :title="asset.targetUrl">
                  {{ truncateUrl(asset.targetUrl) }}
                </span>
              </TableCell>
              <TableCell>
                <Badge
                  :variant="statusBadge(asset.currentStatus).variant"
                  :class="['gap-1.5 pl-2 pr-2.5', statusBadge(asset.currentStatus).cls]"
                >
                  <span :class="['w-1.5 h-1.5 rounded-full inline-block', statusBadge(asset.currentStatus).dot]" />
                  {{ statusBadge(asset.currentStatus).label }}
                </Badge>
              </TableCell>
              <TableCell>
                <span
                  :class="[
                    'font-mono text-sm font-medium',
                    (asset.lastLatencyMs || 0) > 500 ? 'text-yellow-600 dark:text-yellow-400' :
                    (asset.lastLatencyMs || 0) > 200 ? 'text-yellow-500' :
                    'text-green-600 dark:text-green-400'
                  ]"
                >
                  {{ asset.lastLatencyMs != null ? `${asset.lastLatencyMs} ms` : '—' }}
                </span>
              </TableCell>
              <TableCell>
                <span class="text-sm text-muted-foreground inline-flex items-center gap-1.5">
                  <Clock class="w-3.5 h-3.5" />
                  {{ formatAgo(asset.lastCheckedAt) }}
                </span>
              </TableCell>
              <TableCell class="text-right">
                <DropdownMenu>
                  <DropdownMenuTrigger as-child>
                    <Button variant="ghost" size="icon" class="h-9 w-9">
                      <svg class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <circle cx="12" cy="5" r="1" />
                        <circle cx="12" cy="12" r="1" />
                        <circle cx="12" cy="19" r="1" />
                      </svg>
                    </Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end" class="w-44">
                    <DropdownMenuLabel>Actions</DropdownMenuLabel>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem @click="handlePing(asset.id)" class="cursor-pointer gap-2">
                      <RefreshCw class="w-4 h-4" />
                      Ping Now
                    </DropdownMenuItem>
                    <DropdownMenuItem @click="router.push(`/assets/${asset.id}`)" class="cursor-pointer gap-2">
                      <Eye class="w-4 h-4" />
                      View Details
                    </DropdownMenuItem>
                    <DropdownMenuSeparator />
                    <div v-if="deleteConfirmId !== asset.id">
                      <DropdownMenuItem @click="deleteConfirmId = asset.id" class="text-destructive cursor-pointer gap-2">
                        <Trash2 class="w-4 h-4" />
                        Delete
                      </DropdownMenuItem>
                    </div>
                    <div v-else class="p-2 bg-destructive/5 rounded-md my-1 space-y-2">
                      <p class="text-xs text-destructive px-1">Confirm delete?</p>
                      <div class="flex gap-2">
                        <Button size="sm" variant="destructive" class="flex-1 h-8 text-xs" @click="handleDelete(asset.id)">
                          Yes
                        </Button>
                        <Button size="sm" variant="ghost" class="flex-1 h-8 text-xs" @click="deleteConfirmId = null">
                          No
                        </Button>
                      </div>
                    </div>
                  </DropdownMenuContent>
                </DropdownMenu>
              </TableCell>
            </TableRow>
          </TableBody>
        </Table>
      </div>
    </Card>

    <AddAssetModal v-model:open="addAssetOpen" />
  </div>
</template>
