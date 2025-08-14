export function getAccessToken(): string | null {
  return localStorage.getItem('access_token')
}

export function getRefreshToken(): string | null {
  return localStorage.getItem('refresh_token')
}

export function isAuthenticated(): boolean {
  const t = getAccessToken()
  return !!t && t.length > 0
}

export function saveTokens(access?: string | null, refresh?: string | null) {
  if (access) localStorage.setItem('access_token', access)
  if (refresh) localStorage.setItem('refresh_token', refresh)
}

export function clearTokens() {
  localStorage.removeItem('access_token')
  localStorage.removeItem('refresh_token')
}


