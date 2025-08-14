import { createSignal } from 'solid-js'
import api from '../lib/api'

export default function Login() {
  const [email, setEmail] = createSignal('')
  const [password, setPassword] = createSignal('')
  const [loading, setLoading] = createSignal(false)
  const [error, setError] = createSignal<string | null>(null)

  const onSubmit = async (e: Event) => {
    e.preventDefault()
    setError(null)
    setLoading(true)
    try {
      const { data } = await api.post('/v1/auth/login', { email: email(), password: password() })
      if (data?.result?.access || data?.result?.refresh) {
        // ожидаем формат LSResponse<(access, refresh)>
        const access = data.result.access ?? data.result?.item1
        const refresh = data.result.refresh ?? data.result?.item2
        if (access) localStorage.setItem('access_token', access)
        if (refresh) localStorage.setItem('refresh_token', refresh)
        location.href = '/settings'
      } else if (data?.result) {
        // совместимость: строковый токен
        localStorage.setItem('access_token', data.result)
        location.href = '/settings'
      } else {
        setError('Неверный логин или пароль')
      }
    } catch (err: any) {
      setError(err?.message ?? 'Ошибка входа')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div class="max-w-md mx-auto p-6 glass">
      <h1 class="text-2xl mb-4">Вход</h1>
      <form onSubmit={onSubmit} class="space-y-3">
        <input class="input" type="email" placeholder="Email" value={email()} onInput={e => setEmail(e.currentTarget.value)} />
        <input class="input" type="password" placeholder="Пароль" value={password()} onInput={e => setPassword(e.currentTarget.value)} />
        {error() && <div class="text-red-400 text-sm">{error()}</div>}
        <button class="btn w-full" disabled={loading()}>{loading() ? '...' : 'Войти'}</button>
      </form>
    </div>
  )
}


