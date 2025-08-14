import { createStore } from 'solid-js/store'
import api from '../lib/api'
import { saveTokens } from '../lib/auth'

export default function Register() {
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
      const { data } = await api.post('/v1.0/auth/register', { email: state.email, password: state.password })
      // Регистрация возвращает LSResponse<AppUser>, можно сразу отправить на /login или выполнить авто‑логин
      const login = await api.post('/v1.0/auth/login', { email: state.email, password: state.password })
      const res = login.data
      if (res?.result?.access || res?.result?.refresh) {
        saveTokens(res.result.access ?? res.result?.item1, res.result.refresh ?? res.result?.item2)
        location.href = '/settings'
      } else if (res?.result) {
        saveTokens(res.result, null)
        location.href = '/settings'
      } else {
        location.href = '/login'
      }
    } catch (err: any) {
      setState('error', err?.message ?? 'Ошибка регистрации')
    } finally {
      setState('loading', false)
    }
  }

  return (
    <div class="max-w-md mx-auto p-6 glass">
      <h1 class="text-2xl mb-4">Регистрация</h1>
      <form onSubmit={onSubmit} class="space-y-3">
        <input class="input" type="email" placeholder="Email" value={state.email} onInput={e => setState('email', e.currentTarget.value)} />
        <input class="input" type="password" placeholder="Пароль" value={state.password} onInput={e => setState('password', e.currentTarget.value)} />
        {state.error && <div class="text-red-400 text-sm">{state.error}</div>}
        <button class="btn w-full" disabled={state.loading}>{state.loading ? '...' : 'Создать аккаунт'}</button>
      </form>
    </div>
  )
}


