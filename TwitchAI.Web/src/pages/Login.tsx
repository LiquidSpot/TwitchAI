import { createStore } from 'solid-js/store'
import api from '../lib/api'
import { saveTokens } from '../lib/auth'

export default function Login() {
  const [state, setState] = createStore<{ email: string; password: string; loading: boolean; error: string | null }>({
    email: '',
    password: '',
    loading: false,
    error: null,
  })

  const onSubmit = async (e: Event) => {
    e.preventDefault()
    setState('error', null)
    setState('loading', true)
    try {
      const { data } = await api.post('/v1.0/auth/login', { email: state.email, password: state.password })
      if (data?.result?.access || data?.result?.refresh) {
        // ожидаем формат LSResponse<(access, refresh)>
        const access = data.result.access ?? data.result?.item1
        const refresh = data.result.refresh ?? data.result?.item2
        saveTokens(access ?? null, refresh ?? undefined)
        location.href = '/settings'
      } else if (data?.result) {
        // совместимость: строковый токен
        saveTokens(data.result ?? null, undefined)
        location.href = '/settings'
      } else {
        setState('error', 'Неверный логин или пароль')
      }
    } catch (err: any) {
      setState('error', err?.message ?? 'Ошибка входа')
    } finally {
      setState('loading', false)
    }
  }

  return (
    <div class="max-w-md mx-auto p-6 glass">
      <h1 class="text-2xl mb-4">Вход</h1>
      <form onSubmit={onSubmit} class="space-y-3">
        <input class="input" type="email" placeholder="Email" value={state.email} onInput={e => setState('email', e.currentTarget.value)} />
        <input class="input" type="password" placeholder="Пароль" value={state.password} onInput={e => setState('password', e.currentTarget.value)} />
        {state.error && <div class="text-red-400 text-sm">{state.error}</div>}
        <button class="btn btn-primary w-full" disabled={state.loading}>{state.loading ? '...' : 'Войти'}</button>
      </form>
    </div>
  )
}


