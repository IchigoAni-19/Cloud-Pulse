<script setup lang="ts">
import { computed } from 'vue'
import { RouterView, useRoute } from 'vue-router'
import Navbar from '@/components/layout/Navbar.vue'

const route = useRoute()
const isAuthPage = computed(() => route.name === 'Login' || route.name === 'Register')
</script>

<template>
  <main class="min-h-screen bg-background text-foreground">
    <Navbar v-if="!isAuthPage" />
    <RouterView />
  </main>
</template>
