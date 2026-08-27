<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import {
  Activity,
  RefreshCw,
  Check,
  X,
  Clock,
  Zap,
  BarChart3,
  Server,
  Database,
  Cpu,
  Workflow,
  Cloud,
  ArrowLeft,
  Eye,
} from '@lucide/vue'
import { useRouter } from 'vue-router'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
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
import { useAssetsStore } from '@/stores/assets'
import type { AssetResponseDto, AssetStatus, HealthDataPointDto } from '@/stores/assets'

const route = useRoute()
const router = useRouter()
const assetsStore = useAssetsStore()

const assetId = computed(() => route.params.id as string)
const asset = computed<AssetResponseDto | undefined>(
  () => assetsStore.assets.find(a => a.id === assetId.value) || assetsStore.activeAsset || undefined
)

const hoveredPoint = ref<{ idx: number; x: number; y: number } | null>(null)
const isActive = ref(true)

onMounted(async () => {
  try {
    if (assetsStore.assets.length === 0) {
      await assetsStore.fetchAssets()
    }
    const found = assetsStore.assets.find(a => a.id === assetId.value)
    if (found) {
      assetsStore.activeAsset = found
      isActive.value = found.isActive
    }
    await assetsStore.fetchAssetHistory(assetId.value)
  } catch (e) {
    // ignore
  }
})

const history = computed<HealthDataPointDto[]>(() => assetsStore.assetHistory?.history || [])

const uptimePct = computed(() => assetsStore.assetHistory?.uptimePercentage ?? 0)
const avgLatency = computed(() => assetsStore.assetHistory?.averageLatencyMs ?? 0)

function statusBadge(status?: AssetStatus) {
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

function formatTime(timestamp: string): string {
  const d = new Date(timestamp)
  return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' })
}

function formatFullTime(timestamp: string): string {
  const d = new Date(timestamp)
  return d.toLocaleString()
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

const chartWidth = 800
const chartHeight = 260
const paddingTop = 20
const paddingBottom = 40
const paddingLeft = 50
const paddingRight = 20

const chartInnerWidth = chartWidth - paddingLeft - paddingRight
const chartInnerHeight = chartHeight - paddingTop - paddingBottom

const chartPoints = computed(() => {
  if (history.value.length === 0) return []
  const maxLatency = Math.max(...history.value.map(p => p.latencyMs), 100)
  const minLatency = 0
  const n = history.value.length
  return history.value.map((p, i) => {
    const x = n === 1 ? paddingLeft + chartInnerWidth / 2 : paddingLeft + (i / (n - 1)) * chartInnerWidth
    const latencyRange = Math.max(1, maxLatency - minLatency)
    const y = paddingTop + chartInnerHeight - ((p.latencyMs - minLatency) / latencyRange) * chartInnerHeight
    return { point: p, x, y, latencyRange, maxLatency, minLatency }
  })
})

const polylinePath = computed(() => {
  if (chartPoints.value.length === 0) return ''
  return chartPoints.value.map((cp, i) => `${i === 0 ? 'M' : 'L'} ${cp.x} ${cp.y}`).join(' ')
})

const yAxisTicks = computed(() => {
  const maxLatency = Math.max(...history.value.map(p => p.latencyMs), 100)
  const ticks = 4
  const result: { y: number; label: string }[] = []
  for (let i = 0; i <= ticks; i++) {
    const val = (maxLatency / ticks) * i
    const y = paddingTop + chartInnerHeight - (val / Math.max(1, maxLatency)) * chartInnerHeight
    result.push({ y, label: `${Math.round(val)} ms` })
  }
  return result
})

function colorForStatus(status?: AssetStatus) {
  switch (status) {
    case 'Healthy': return '#22c55e'
    case 'Degraded': return '#eab308'
    case 'Down': return '#ef4444'
    default: return '#94a3b8'
  }
}

async function handlePing() {
  await assetsStore.pingAsset(assetId.value)
  await assetsStore.fetchAssetHistory(assetId.value)
}

const recentLogs = computed(() => history.value.slice(-10).reverse())
</script>

<template>
  <div class="container mx-auto px-4 py-8 space-y-6 max-w-7xl">
    <div class="flex items-center justify-between flex-wrap gap-4">
      <div>
        <button
          class="text-sm text-muted-foreground hover:text-foreground flex items-center gap-1 mb-2 transition-colors"
          @click="router.push('/')"
        >
          <ArrowLeft class="w-4 h-4" />
          Back to Dashboard
        </button>
        <div class="flex items-center gap-3 flex-wrap">
          <h1 class="text-2xl md:text-3xl font-bold tracking-tight flex items-center gap-2">
            <component
              v-if="asset"
              :is="resourceIcon(asset.resourceType)"
              class="w-7 h-7 text-primary"
            />
            {{ asset?.name || assetsStore.assetHistory?.assetName || 'Asset Detail' }}
          </h1>
          <Badge
            v-if="asset"
            :variant="statusBadge(asset.currentStatus).variant"
            :class="['gap-1.5 pl-2 pr-2.5 py-1', statusBadge(asset.currentStatus).cls]"
          >
            <span :class="['w-1.5 h-1.5 rounded-full inline-block', statusBadge(asset.currentStatus).dot]" />
            {{ statusBadge(asset.currentStatus).label }}
          </Badge>
        </div>
      </div>
      <div class="flex items-center gap-2">
        <Button class="gap-2" @click="handlePing" :disabled="assetsStore.isLoading">
          <RefreshCw :class="['w-4 h-4', assetsStore.isLoading && 'animate-spin']" />
          Ping Now
        </Button>
      </div>
    </div>

    <div v-if="asset" class="grid grid-cols-1 md:grid-cols-4 gap-4">
      <Card class="col-span-full md:col-span-2 border-border/50">
        <CardContent class="p-5 space-y-3">
          <div class="flex items-start gap-4 flex-wrap">
            <div class="space-y-1.5">
              <p class="text-xs text-muted-foreground uppercase tracking-wide font-medium">Target URL</p>
              <a :href="asset.targetUrl" target="_blank" rel="noopener" class="font-mono text-sm text-primary hover:underline break-all">
                {{ asset.targetUrl }}
              </a>
            </div>
            <div class="flex items-center gap-2 flex-wrap">
              <Badge variant="outline">{{ asset.resourceType }}</Badge>
              <Badge
                :variant="asset.environment === 'Production' ? 'default' : asset.environment === 'Staging' ? 'secondary' : 'outline'"
                :class="asset.environment === 'Production' ? 'bg-green-500 hover:bg-green-500' : ''"
              >
                {{ asset.environment }}
              </Badge>
              <Badge variant="outline">
                {{ asset.checkIntervalSeconds }}s interval
              </Badge>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card class="border-border/50">
        <CardContent class="p-5">
          <div class="flex items-center gap-3">
            <div class="w-11 h-11 rounded-xl bg-green-500/15 flex items-center justify-center">
              <Check class="w-6 h-6 text-green-600 dark:text-green-400" />
            </div>
            <div>
              <p class="text-sm text-muted-foreground">Uptime</p>
              <p class="text-2xl font-bold">{{ uptimePct.toFixed(2) }}%</p>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card class="border-border/50">
        <CardContent class="p-5">
          <div class="flex items-center gap-3">
            <div class="w-11 h-11 rounded-xl bg-primary/15 flex items-center justify-center">
              <Zap class="w-6 h-6 text-primary" />
            </div>
            <div>
              <p class="text-sm text-muted-foreground">Avg Latency</p>
              <p class="text-2xl font-bold">{{ avgLatency.toFixed(0) }} ms</p>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card class="border-border/50">
        <CardContent class="p-5">
          <div class="flex items-center gap-3">
            <div class="w-11 h-11 rounded-xl bg-muted flex items-center justify-center">
              <Clock class="w-6 h-6 text-muted-foreground" />
            </div>
            <div>
              <p class="text-sm text-muted-foreground">Last Checked</p>
              <p class="text-sm font-semibold mt-1">{{ formatAgo(asset.lastCheckedAt) }}</p>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card class="border-border/50">
        <CardContent class="p-5">
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-3">
              <div :class="['w-11 h-11 rounded-xl flex items-center justify-center transition-colors', isActive ? 'bg-green-500/15' : 'bg-muted']">
                <Eye :class="['w-6 h-6', isActive ? 'text-green-600 dark:text-green-400' : 'text-muted-foreground']" />
              </div>
              <div>
                <p class="text-sm text-muted-foreground">Monitoring</p>
                <p class="text-sm font-semibold mt-1">{{ isActive ? 'Active' : 'Paused' }}</p>
              </div>
            </div>
            <button
              :class="[
                'relative w-11 h-6 rounded-full transition-colors duration-200',
                isActive ? 'bg-primary' : 'bg-muted-foreground/30'
              ]"
              @click="isActive = !isActive"
            >
              <span
                :class="[
                  'absolute top-1 w-4 h-4 rounded-full bg-white shadow-sm transition-transform duration-200',
                  isActive ? 'translate-x-6' : 'translate-x-1'
                ]"
              />
            </button>
          </div>
        </CardContent>
      </Card>
    </div>

    <Card class="border-border/50 shadow-sm">
      <CardHeader class="pb-3">
        <CardTitle class="flex items-center gap-2">
          <BarChart3 class="w-5 h-5 text-primary" />
          Latency Timeline
        </CardTitle>
        <CardDescription>
          {{ history.length }} health checks recorded
        </CardDescription>
      </CardHeader>
      <CardContent>
        <div v-if="assetsStore.isLoading && history.length === 0" class="py-16 text-center text-muted-foreground">
          <RefreshCw class="w-8 h-8 animate-spin text-primary mx-auto mb-3" />
          Loading metrics history...
        </div>
        <div v-else-if="history.length === 0" class="py-16 text-center text-muted-foreground">
          <div class="w-16 h-16 rounded-2xl bg-muted flex items-center justify-center mx-auto mb-3">
            <BarChart3 class="w-9 h-9" />
          </div>
          <p class="font-medium text-foreground mb-1">No history available</p>
          <p class="text-sm">Ping this asset to begin collecting metrics.</p>
        </div>
        <div v-else class="relative w-full overflow-x-auto">
          <svg
            :width="chartWidth"
            :height="chartHeight"
            class="min-w-full"
            @mouseleave="hoveredPoint = null"
          >
            <defs>
              <linearGradient id="chartGradient" x1="0%" y1="0%" x2="0%" y2="100%">
                <stop offset="0%" style="stop-color: hsl(var(--primary)); stop-opacity: 0.2" />
                <stop offset="100%" style="stop-color: hsl(var(--primary)); stop-opacity: 0" />
              </linearGradient>
            </defs>

            <g v-for="t in yAxisTicks" :key="t.label" class="text-xs">
              <line
                :x1="paddingLeft"
                :x2="chartWidth - paddingRight"
                :y1="t.y"
                :y2="t.y"
                stroke="hsl(var(--border))"
                stroke-width="1"
                stroke-dasharray="4 4"
              />
              <text
                :x="paddingLeft - 8"
                :y="t.y + 4"
                fill="hsl(var(--muted-foreground))"
                font-size="11"
                text-anchor="end"
                font-family="Inter, sans-serif"
              >
                {{ t.label }}
              </text>
            </g>

            <path
              v-if="chartPoints.length > 1"
              :d="`${polylinePath} L ${chartPoints[chartPoints.length - 1].x} ${paddingTop + chartInnerHeight} L ${chartPoints[0].x} ${paddingTop + chartInnerHeight} Z`"
              fill="url(#chartGradient)"
            />
            <path
              v-if="chartPoints.length > 1"
              :d="polylinePath"
              fill="none"
              stroke="hsl(var(--primary))"
              stroke-width="2.5"
              stroke-linecap="round"
              stroke-linejoin="round"
            />

            <g v-for="(cp, idx) in chartPoints" :key="idx">
              <circle
                :cx="cp.x"
                :cy="cp.y"
                r="5.5"
                fill="hsl(var(--card))"
                :stroke="colorForStatus(cp.point.status)"
                stroke-width="2.5"
                class="cursor-pointer transition-transform hover:scale-125 origin-center"
                style="transform-box: fill-box;"
                @mouseenter="() => hoveredPoint = { idx, x: cp.x, y: cp.y }"
                @mousemove="() => { if (hoveredPoint) { hoveredPoint.x = cp.x; hoveredPoint.y = cp.y } }"
              />
            </g>

            <g v-if="chartPoints.length > 0">
              <text
                :x="chartPoints[0].x"
                :y="chartHeight - 14"
                fill="hsl(var(--muted-foreground))"
                font-size="10"
                text-anchor="start"
                font-family="Inter, sans-serif"
              >
                {{ formatTime(chartPoints[0].point.timestamp) }}
              </text>
              <text
                :x="chartPoints[chartPoints.length - 1].x"
                :y="chartHeight - 14"
                fill="hsl(var(--muted-foreground))"
                font-size="10"
                text-anchor="end"
                font-family="Inter, sans-serif"
              >
                {{ formatTime(chartPoints[chartPoints.length - 1].point.timestamp) }}
              </text>
              <text
                v-if="chartPoints.length >= 3"
                :x="chartPoints[Math.floor(chartPoints.length / 2)].x"
                :y="chartHeight - 14"
                fill="hsl(var(--muted-foreground))"
                font-size="10"
                text-anchor="middle"
                font-family="Inter, sans-serif"
              >
                {{ formatTime(chartPoints[Math.floor(chartPoints.length / 2)].point.timestamp) }}
              </text>
            </g>

            <rect
              :x="paddingLeft"
              :y="paddingTop"
              :width="chartInnerWidth"
              :height="chartInnerHeight"
              fill="transparent"
            />
          </svg>

          <div
            v-if="hoveredPoint != null && history[hoveredPoint.idx]"
            class="absolute pointer-events-none z-10 bg-card border border-border rounded-lg shadow-xl p-3 text-xs min-w-[180px] animate-zoom-in"
            :style="{
              left: `calc(${Math.min(Math.max(hoveredPoint.x / chartWidth * 100, 10), 80)}% + ${hoveredPoint.x < chartWidth * 0.2 ? '16px' : '-200px'})`,
              top: `calc(${Math.max(hoveredPoint.y / chartHeight * 100, 5)}% - 20px)`,
              transform: hoveredPoint.x > chartWidth * 0.8 ? 'translateX(-100%)' : undefined,
            }"
          >
            <div class="flex items-center gap-2 mb-1.5">
              <span
                class="w-2 h-2 rounded-full"
                :style="{ background: colorForStatus(history[hoveredPoint.idx].status) }"
              />
              <span class="font-semibold text-xs">
                {{ history[hoveredPoint.idx].status }}
              </span>
              <span class="ml-auto font-mono text-[11px] text-muted-foreground">
                {{ history[hoveredPoint.idx].statusCode }}
              </span>
            </div>
            <div class="text-[11px] text-muted-foreground mb-1">
              {{ formatFullTime(history[hoveredPoint.idx].timestamp) }}
            </div>
            <div class="font-mono text-sm font-bold flex items-center gap-1">
              <Zap class="w-3 h-3 text-primary" />
              {{ history[hoveredPoint.idx].latencyMs }} ms
            </div>
          </div>
        </div>
      </CardContent>
    </Card>

    <Card class="border-border/50 shadow-sm">
      <CardHeader class="pb-3">
        <CardTitle class="flex items-center gap-2">
          <Activity class="w-5 h-5 text-primary" />
          Recent Health Logs
        </CardTitle>
        <CardDescription>
          Last {{ recentLogs.length }} health check results
        </CardDescription>
      </CardHeader>
      <CardContent class="p-0">
        <div class="overflow-x-auto">
          <Table>
            <TableHeader class="bg-muted/40">
              <TableRow>
                <TableHead class="font-semibold">Time</TableHead>
                <TableHead class="font-semibold">Status</TableHead>
                <TableHead class="font-semibold">HTTP Code</TableHead>
                <TableHead class="font-semibold">Latency</TableHead>
                <TableHead class="font-semibold text-right">Result</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              <TableRow v-if="recentLogs.length === 0">
                <TableCell :colspan="5" class="text-center py-12 text-muted-foreground">
                  No recent health logs.
                </TableCell>
              </TableRow>
              <TableRow v-for="(log, i) in recentLogs" :key="i" class="transition-colors hover:bg-muted/30">
                <TableCell>
                  <span class="text-sm flex items-center gap-1.5">
                    <Clock class="w-3.5 h-3.5 text-muted-foreground" />
                    {{ formatFullTime(log.timestamp) }}
                  </span>
                </TableCell>
                <TableCell>
                  <Badge
                    :variant="statusBadge(log.status).variant"
                    :class="['gap-1.5 pl-2 pr-2.5', statusBadge(log.status).cls]"
                  >
                    <span :class="['w-1.5 h-1.5 rounded-full inline-block', statusBadge(log.status).dot]" />
                    {{ statusBadge(log.status).label }}
                  </Badge>
                </TableCell>
                <TableCell>
                  <span class="font-mono text-sm">{{ log.statusCode }}</span>
                </TableCell>
                <TableCell>
                  <span
                    :class="[
                      'font-mono text-sm font-medium',
                      log.latencyMs > 500 ? 'text-yellow-600 dark:text-yellow-400' :
                      log.latencyMs > 200 ? 'text-yellow-500' :
                      'text-green-600 dark:text-green-400'
                    ]"
                  >
                    {{ log.latencyMs }} ms
                  </span>
                </TableCell>
                <TableCell class="text-right">
                  <Badge v-if="log.success" variant="default" class="gap-1 bg-green-500 hover:bg-green-500">
                    <Check class="w-3 h-3" /> Success
                  </Badge>
                  <Badge v-else variant="destructive" class="gap-1">
                    <X class="w-3 h-3" /> Failed
                  </Badge>
                </TableCell>
              </TableRow>
            </TableBody>
          </Table>
        </div>
      </CardContent>
    </Card>
  </div>
</template>
