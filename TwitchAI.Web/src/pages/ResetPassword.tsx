import { createSignal } from 'solid-js'
import api from '../lib/api'

export default function ResetPassword() {
  const [email, setEmail] = createSignal('')
  const [info, setInfo] = createSignal<string | null>(null)
  const [loading, setLoading] = createSignal(false)

  const onSubmit = async (e: Event) => {
    e.preventDefault()
    setLoading(true)
    try {
      // заглушка, пока нет backend‑эндпойнта
      await new Promise(r => setTimeout(r, 600))
      setInfo('Если email существует, мы отправим инструкции по сбросу пароля')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div class="max-w-md mx-auto p-6 glass">
      <h1 class="text-2xl mb-4">Восстановление пароля</h1>
      <form onSubmit={onSubmit} class="space-y-3">
        <input class="input" type="email" placeholder="Email" value={email()} onInput={e => setEmail(e.currentTarget.value)} />
        {info() && <div class="text-accent text-sm">{info()}</div>}
        <button class="btn w-full" disabled={loading()}>{loading() ? '...' : 'Отправить'}</button>
      </form>
    </div>
  )
}


