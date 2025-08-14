import axios, { AxiosError, AxiosRequestConfig } from 'axios'
import { getAccessToken, getRefreshToken, saveTokens, clearTokens } from './auth'

const api = axios.create({ baseURL: '/api' })

// Проставляем access токен в каждый запрос
api.interceptors.request.use((config) => {
  const t = getAccessToken()
  if (t) {
    const headers: Record<string, string> = (config.headers as any) ?? {}
    headers.Authorization = `Bearer ${t}`
    config.headers = headers as any
  }
  return config
})

let isRefreshing = false
let pendingQueue: Array<(token: string | null) => void> = []

async function refreshToken(): Promise<string | null> {
  if (isRefreshing) {
    return new Promise(resolve => pendingQueue.push(resolve))
  }
  isRefreshing = true
  try {
    const rt = getRefreshToken()
    if (!rt) throw new Error('No refresh token')
    const resp = await axios.post('/api/v1/auth/refresh', { refreshToken: rt })
    const newAccess = resp.data?.data?.access ?? resp.data?.access ?? resp.data?.token ?? null
    const newRefresh = resp.data?.data?.refresh ?? resp.data?.refresh ?? null
    saveTokens(newAccess, newRefresh ?? undefined)
    pendingQueue.forEach(cb => cb(newAccess))
    pendingQueue = []
    return newAccess
  } catch (e) {
    saveTokens(null, null)
    pendingQueue.forEach(cb => cb(null))
    pendingQueue = []
    throw e
  } finally {
    isRefreshing = false
  }
}

api.interceptors.response.use(
  (r) => r,
  async (error: AxiosError) => {
    const original = error.config as (AxiosRequestConfig & { _retry?: boolean })
    const status = error.response?.status
    if (status === 401 && !original?._retry) {
      original._retry = true
      try {
        const newToken = await refreshToken()
        if (newToken) {
          original.headers = original.headers ?? {}
          original.headers.Authorization = `Bearer ${newToken}`
          return api.request(original)
        }
      } catch {}
      clearTokens()
      try { window.location.href = '/login' } catch {}
      // отдадим исходную ошибку
    }
    return Promise.reject(error)
  }
)

export default api


