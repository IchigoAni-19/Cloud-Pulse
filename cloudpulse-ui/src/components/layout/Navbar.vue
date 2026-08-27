<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import {
  Sun,
  Moon,
  ChevronDown,
  User,
  LogOut,
  Activity,
  CreditCard,
} from '@lucide/vue'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { useAuthStore } from '@/stores/auth'

const route = useRoute()
const authStore = useAuthStore()
const isDark = ref(false)

function toggleTheme() {
  isDark.value = !isDark.value
  if (isDark.value) {
    document.documentElement.classList.add('dark')
    localStorage.setItem('theme', 'dark')
  } else {
    document.documentElement.classList.remove('dark')
    localStorage.setItem('theme', 'light')
  }
}

onMounted(() => {
  const storedTheme = localStorage.getItem('theme')
  const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches
  if (storedTheme === 'dark' || (!storedTheme && prefersDark)) {
    isDark.value = true
    document.documentElement.classList.add('dark')
  }
})

function handleLogout() {
  authStore.logout()
  window.location.href = '/login'
}

const tierBadgeVariant = computed(() => {
  return authStore.user?.subscriptionTier === 'Pro' ? 'default' : 'secondary'
})
</script>

<template>
  <header
    class="sticky top-0 z-50 backdrop-blur-md border-b border-border"
    style="background: linear-gradient(135deg, rgba(15, 23, 42, 0.95), rgba(30, 27, 75, 0.95));"
  >
    <div class="container mx-auto px-4 h-16 flex items-center justify-between">
      <div class="flex items-center gap-10">
        <RouterLink to="/" class="flex items-center gap-2 text-white">
          <div class="w-8 h-8 rounded-lg bg-gradient-to-br from-blue-500 to-purple-600 flex items-center justify-center">
            <Activity class="w-5 h-5 text-white" />
          </div>
          <span class="font-bold text-lg tracking-tight">CloudPulse</span>
        </RouterLink>

        <nav class="hidden md:flex items-center gap-1">
          <RouterLink
            to="/"
            :class="[
              'px-4 py-2 rounded-lg text-sm font-medium transition-all duration-200',
              route.path === '/'
                ? 'bg-white/10 text-white shadow-inner'
                : 'text-white/70 hover:text-white hover:bg-white/5'
            ]"
          >
            Dashboard
          </RouterLink>
          <RouterLink
            to="/billing"
            :class="[
              'px-4 py-2 rounded-lg text-sm font-medium transition-all duration-200 flex items-center gap-2',
              route.path === '/billing'
                ? 'bg-white/10 text-white shadow-inner'
                : 'text-white/70 hover:text-white hover:bg-white/5'
            ]"
          >
            <CreditCard class="w-4 h-4" />
            Billing
          </RouterLink>
        </nav>
      </div>

      <div class="flex items-center gap-2">
        <Button
          variant="ghost"
          size="icon"
          class="text-white/80 hover:text-white hover:bg-white/10"
          @click="toggleTheme"
        >
          <Sun v-if="isDark" class="w-5 h-5" />
          <Moon v-else class="w-5 h-5" />
        </Button>

        <DropdownMenu>
          <DropdownMenuTrigger as-child>
            <Button variant="ghost" class="text-white hover:bg-white/10 gap-2 px-3">
              <div class="w-8 h-8 rounded-full bg-gradient-to-br from-blue-500 to-purple-600 flex items-center justify-center">
                <User class="w-4 h-4 text-white" />
              </div>
              <span class="hidden md:block text-sm text-white/90 max-w-[140px] truncate">
                {{ authStore.user?.email || 'Account' }}
              </span>
              <ChevronDown class="w-4 h-4 text-white/60" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" class="w-56">
            <DropdownMenuLabel>
              <div class="flex flex-col gap-1">
                <span class="font-medium text-sm">{{ authStore.user?.email || 'User' }}</span>
                <div class="flex items-center gap-2">
                  <span class="text-xs text-muted-foreground">
                    {{ authStore.user?.role || 'User' }}
                  </span>
                  <Badge :variant="tierBadgeVariant" class="text-[10px] h-4 px-1.5">
                    {{ authStore.user?.subscriptionTier || 'Free' }}
                  </Badge>
                </div>
              </div>
            </DropdownMenuLabel>
            <DropdownMenuSeparator />
            <DropdownMenuItem @click="handleLogout" class="text-destructive cursor-pointer">
              <LogOut class="w-4 h-4 mr-2" />
              Logout
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
    </div>
  </header>
</template>
