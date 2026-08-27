import { defineStore } from 'pinia'
import { ref } from 'vue'
import { apiClient } from '@/api/axios'

export interface User {
  id: string
  email: string
  phoneNumber?: string
  role: string
  subscriptionTier: string
}

export const useAuthStore = defineStore('auth', () => {
  const user = ref<User | null>(null)
  const token = ref<string>('')
  const isAuthenticated = ref<boolean>(false)
  const isLoading = ref<boolean>(false)

  function loadFromStorage() {
    const storedToken = localStorage.getItem('access_token')
    const storedUser = localStorage.getItem('user')
    if (storedToken) {
      token.value = storedToken
      isAuthenticated.value = true
    }
    if (storedUser) {
      try {
        user.value = JSON.parse(storedUser)
      } catch (e) {
        console.error('Failed to parse stored user:', e)
      }
    }
  }

  async function login({ email, password }: { email: string; password: string }) {
    isLoading.value = true
    try {
      const response = await apiClient.post('/auth/login', { email, password })
      const data = response.data
      token.value = data.accessToken || data.token || ''
      user.value = data.user || { id: data.userId, email, role: 'User', subscriptionTier: 'Free' }
      isAuthenticated.value = true
      localStorage.setItem('access_token', token.value)
      localStorage.setItem('user', JSON.stringify(user.value))
      return data
    } finally {
      isLoading.value = false
    }
  }

  async function register({ email, password, phoneNumber }: { email: string; password: string; phoneNumber?: string }) {
    isLoading.value = true
    try {
      const response = await apiClient.post('/auth/register', { email, password, phoneNumber })
      const data = response.data
      token.value = data.accessToken || data.token || ''
      user.value = data.user || { id: data.userId, email, phoneNumber, role: 'User', subscriptionTier: 'Free' }
      isAuthenticated.value = true
      localStorage.setItem('access_token', token.value)
      localStorage.setItem('user', JSON.stringify(user.value))
      return data
    } finally {
      isLoading.value = false
    }
  }

  async function loginWithGoogle(idToken: string) {
    isLoading.value = true
    try {
      const response = await apiClient.post('/auth/google', { idToken })
      const data = response.data
      token.value = data.accessToken || data.token || ''
      user.value = data.user || { id: data.userId, email: data.email || '', role: 'User', subscriptionTier: 'Free' }
      isAuthenticated.value = true
      localStorage.setItem('access_token', token.value)
      localStorage.setItem('user', JSON.stringify(user.value))
      return data
    } finally {
      isLoading.value = false
    }
  }

  async function sendPhoneOtp(phone: string) {
    isLoading.value = true
    try {
      const response = await apiClient.post('/auth/phone/send-otp', { phone })
      return response.data
    } finally {
      isLoading.value = false
    }
  }

  async function verifyPhoneOtp({ phone, otpCode }: { phone: string; otpCode: string }) {
    isLoading.value = true
    try {
      const response = await apiClient.post('/auth/phone/verify-otp', { phone, otpCode })
      const data = response.data
      token.value = data.accessToken || data.token || ''
      user.value = data.user || { id: data.userId, email: '', phoneNumber: phone, role: 'User', subscriptionTier: 'Free' }
      isAuthenticated.value = true
      localStorage.setItem('access_token', token.value)
      localStorage.setItem('user', JSON.stringify(user.value))
      return data
    } finally {
      isLoading.value = false
    }
  }

  function logout() {
    user.value = null
    token.value = ''
    isAuthenticated.value = false
    localStorage.clear()
  }

  loadFromStorage()

  return {
    user,
    token,
    isAuthenticated,
    isLoading,
    login,
    register,
    loginWithGoogle,
    sendPhoneOtp,
    verifyPhoneOtp,
    logout,
    loadFromStorage,
  }
})
