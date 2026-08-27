import { defineStore } from 'pinia'
import { ref } from 'vue'
import { apiClient } from '@/api/axios'

export type AssetStatus = 'Healthy' | 'Degraded' | 'Down' | 'Unknown'
export type ResourceType = 'API' | 'Database' | 'VM' | 'Worker'
export type Environment = 'Production' | 'Staging' | 'Development'

export interface AssetResponseDto {
  id: string
  name: string
  targetUrl: string
  resourceType: ResourceType
  environment: Environment
  checkIntervalSeconds: number
  currentStatus: AssetStatus
  lastLatencyMs?: number
  lastCheckedAt?: string
  isActive: boolean
  createdAt: string
}

export interface DashboardSummaryDto {
  totalAssets: number
  overallUptimePercentage: number
  healthyCount: number
  criticalCount: number
  degradedCount: number
  unknownCount: number
}

export interface HealthDataPointDto {
  timestamp: string
  latencyMs: number
  statusCode: number
  success: boolean
  status: AssetStatus
}

export interface AssetMetricsHistoryDto {
  assetId: string
  assetName: string
  uptimePercentage: number
  averageLatencyMs: number
  history: HealthDataPointDto[]
}

export const useAssetsStore = defineStore('assets', () => {
  const assets = ref<AssetResponseDto[]>([])
  const activeAsset = ref<AssetResponseDto | null>(null)
  const summaryMetrics = ref<DashboardSummaryDto>({
    totalAssets: 0,
    overallUptimePercentage: 0,
    healthyCount: 0,
    criticalCount: 0,
    degradedCount: 0,
    unknownCount: 0,
  })
  const assetHistory = ref<AssetMetricsHistoryDto | null>(null)
  const isLoading = ref<boolean>(false)
  const error = ref<string | null>(null)

  async function fetchAssets({ env, type }: { env?: Environment; type?: ResourceType } = {}) {
    isLoading.value = true
    error.value = null
    try {
      const params: Record<string, string> = {}
      if (env) params.environment = env
      if (type) params.resourceType = type
      const response = await apiClient.get('/assets', { params })
      assets.value = response.data || []
      return assets.value
    } catch (e: any) {
      error.value = e.message || 'Failed to fetch assets'
      throw e
    } finally {
      isLoading.value = false
    }
  }

  async function createAsset(payload: Omit<AssetResponseDto, 'id' | 'currentStatus' | 'lastCheckedAt' | 'createdAt' | 'isActive' | 'lastLatencyMs'>) {
    isLoading.value = true
    error.value = null
    try {
      const response = await apiClient.post('/assets', payload)
      const newAsset: AssetResponseDto = response.data
      assets.value.push(newAsset)
      return newAsset
    } catch (e: any) {
      error.value = e.response?.data?.message || e.message || 'Failed to create asset'
      throw e
    } finally {
      isLoading.value = false
    }
  }

  async function deleteAsset(id: string) {
    isLoading.value = true
    error.value = null
    try {
      await apiClient.delete(`/assets/${id}`)
      assets.value = assets.value.filter(a => a.id !== id)
      if (activeAsset.value?.id === id) {
        activeAsset.value = null
      }
    } catch (e: any) {
      error.value = e.message || 'Failed to delete asset'
      throw e
    } finally {
      isLoading.value = false
    }
  }

  async function pingAsset(id: string) {
    isLoading.value = true
    error.value = null
    try {
      const response = await apiClient.post(`/assets/${id}/ping`)
      const updatedAsset: AssetResponseDto = response.data
      const idx = assets.value.findIndex(a => a.id === id)
      if (idx !== -1) {
        assets.value[idx] = updatedAsset
      }
      if (activeAsset.value?.id === id) {
        activeAsset.value = updatedAsset
      }
      return updatedAsset
    } catch (e: any) {
      error.value = e.message || 'Failed to ping asset'
      throw e
    } finally {
      isLoading.value = false
    }
  }

  async function fetchDashboardMetrics() {
    isLoading.value = true
    error.value = null
    try {
      const response = await apiClient.get('/metrics/dashboard')
      summaryMetrics.value = response.data || summaryMetrics.value
      return summaryMetrics.value
    } catch (e: any) {
      error.value = e.message || 'Failed to fetch dashboard metrics'
      throw e
    } finally {
      isLoading.value = false
    }
  }

  async function fetchAssetHistory(id: string) {
    isLoading.value = true
    error.value = null
    try {
      const response = await apiClient.get(`/metrics/${id}/history`)
      assetHistory.value = response.data
      return assetHistory.value
    } catch (e: any) {
      error.value = e.message || 'Failed to fetch asset history'
      throw e
    } finally {
      isLoading.value = false
    }
  }

  return {
    assets,
    activeAsset,
    summaryMetrics,
    assetHistory,
    isLoading,
    error,
    fetchAssets,
    createAsset,
    deleteAsset,
    pingAsset,
    fetchDashboardMetrics,
    fetchAssetHistory,
  }
})
