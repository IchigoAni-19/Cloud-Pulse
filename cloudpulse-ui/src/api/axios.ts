import axios, { type AxiosError } from 'axios'
import router from '@/router'

export interface ApiErrorShape {
  message?: string
  title?: string
  errors?: Record<string, string[]>
}

export function extractErrorMessage(error: unknown): string {
  if (!error) return 'An unknown error occurred'

  const axiosErr = error as AxiosError<ApiErrorShape>

  if (!axiosErr.response) {
    if (axiosErr.code === 'ERR_NETWORK' || axiosErr.message?.toLowerCase().includes('network')) {
      return `Network error: could not reach the CloudPulse API at ${import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api/v1'}. Please ensure the backend server is running.`
    }
    return axiosErr.message || 'A connection error occurred'
  }

  const status = axiosErr.response.status
  const data = axiosErr.response.data

  const msg = data?.message || data?.title || axiosErr.message

  if (status === 400) return msg || 'Invalid request. Please check your input and try again.'
  if (status === 401) return msg || 'You are not signed in. Please log in again.'
  if (status === 403) return msg || 'You do not have permission to perform this action.'
  if (status === 404) return msg || 'The requested resource was not found.'
  if (status === 409) return msg || 'A conflict occurred with the current state.'
  if (status >= 500) return msg || `Server error (${status}). Please try again later.`

  return msg || `Request failed with status ${status}`
}

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api/v1',
  headers: {
    'Content-Type': 'application/json'
  },
  timeout: 15000
})

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('access_token')
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    if (error.response && error.response.status === 401) {
      localStorage.clear()
      const currentPath = window.location.pathname
      if (!['/login', '/register'].includes(currentPath)) {
        router.push('/login')
      }
    }
    return Promise.reject(error)
  }
)

