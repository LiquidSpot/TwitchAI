import { createSignal } from 'solid-js'
import api from '../lib/api'
import { saveTokens } from '../lib/auth'

export default function Register() {
  const [email, setEmail] = createSignal('')
  const [password, setPassword] = createSignal('')
  const [loading, setLoading] = createSignal(false)
  const [error, setError] = createSignal<string | null>(null)

  const onSubmit = async (e: Event) => {
    e.preventDefault()
    setError(null)
    setLoading(true)
    try {
      const { data } = await api.post('/v1/auth/register', { email: email(), password: password() })
      // Регистрация возвращает LSResponse<AppUser>, можно сразу отправить на /login или выполнить авто‑логин
      const login = await api.post('/v1/auth/login', { email: email(), password: password() })
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
      setError(err?.message ?? 'Ошибка регистрации')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div class="max-w-md mx-auto p-6 glass">
      <h1 class="text-2xl mb-4">Регистрация</h1>
      <form onSubmit={onSubmit} class="space-y-3">
        <input class="input" type="email" placeholder="Email" value={email()} onInput={e => setEmail(e.currentTarget.value)} />
        <input class="input" type="password" placeholder="Пароль" value={password()} onInput={e => setPassword(e.currentTarget.value)} />
        {error() && <div class="text-red-400 text-sm">{error()}</div>}
        <button class="btn w-full" disabled={loading()}>{loading() ? '...' : 'Создать аккаунт'}</button>
      </form>
    </div>
  )
}


