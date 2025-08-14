let memoryAccessToken: string | null = null
let memoryRefreshToken: string | null = null

// Инициализация из localStorage при загрузке модуля
try {
  memoryAccessToken = localStorage.getItem('access_token')
  memoryRefreshToken = localStorage.getItem('refresh_token')
} catch {}

export function getAccessToken(): string | null {
  return memoryAccessToken
}

export function getRefreshToken(): string | null {
  return memoryRefreshToken
}

export function isAuthenticated(): boolean {
  const t = getAccessToken()
  return !!t && t.length > 0
}

export function saveTokens(access?: string | null, refresh?: string | null) {
  if (typeof access !== 'undefined') {
    memoryAccessToken = access
    if (access) localStorage.setItem('access_token', access)
    else localStorage.removeItem('access_token')
  }
  if (typeof refresh !== 'undefined') {
    memoryRefreshToken = refresh
    if (refresh) localStorage.setItem('refresh_token', refresh)
    else localStorage.removeItem('refresh_token')
  }
}

export function clearTokens() {
  memoryAccessToken = null
  memoryRefreshToken = null
  localStorage.removeItem('access_token')
  localStorage.removeItem('refresh_token')
}


