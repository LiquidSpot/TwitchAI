let memoryAccessToken: string | null = null
let memoryRefreshToken: string | null = null

// Инициализация из localStorage при загрузке модуля
try {
  memoryAccessToken = localStorage.getItem('access_token')
  memoryRefreshToken = localStorage.getItem('refresh_token')
} catch {}

export function getAccessToken(): string | null {
  // Вернём только валидно выглядящий JWT (3 части через точки)
  if (typeof memoryAccessToken === 'string' && memoryAccessToken.split('.').length === 3) {
    return memoryAccessToken
  }
  return null
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
    // Сохраняем только корректный JWT в localStorage, иначе держим в памяти
    if (access && access.split('.').length === 3) localStorage.setItem('access_token', access)
    else localStorage.removeItem('access_token')
  }
  if (typeof refresh !== 'undefined') {
    memoryRefreshToken = refresh
    if (refresh) localStorage.setItem('refresh_token', refresh)
    else localStorage.removeItem('refresh_token')
  }
  try { window.dispatchEvent(new Event('auth-changed')) } catch {}
}

export function clearTokens() {
  memoryAccessToken = null
  memoryRefreshToken = null
  localStorage.removeItem('access_token')
  localStorage.removeItem('refresh_token')
  try { window.dispatchEvent(new Event('auth-changed')) } catch {}
}


