import { jwtDecode } from 'jwt-decode'
import { useNavigate } from '@solidjs/router'
import { reveal } from '../utils/reveal'

type JwtPayload = { email?: string; sub?: string }

export default function Dashboard() {
  const navigate = useNavigate()
  const token = localStorage.getItem('access_token')
  let userEmail: string | undefined
  try { userEmail = token ? (jwtDecode(token) as JwtPayload).email : undefined } catch {}

  const cards = [
    { title: 'Статус API', value: 'OK' },
    { title: 'Интеграции', value: 'Twitch / OpenAI' },
    { title: 'Зрители (активные/всего)', value: '—' },
  ]

  return (
    <div class="max-w-6xl mx-auto px-4 space-y-8">
      <div class="glass p-5" ref={el => reveal(el, () => ({}))}>
        <div class="text-slate-300 text-sm">Добро пожаловать</div>
        <div class="text-2xl">{userEmail ?? 'Личный кабинет'}</div>
      </div>

      <div class="grid md:grid-cols-3 gap-4">
        {cards.map((c, i) => (
          <div class="card-secondary p-5" ref={el => reveal(el, () => ({ delayMs: i * 120 }))}>
            <div class="text-slate-300 text-sm">{c.title}</div>
            <div class="text-2xl">{c.value}</div>
          </div>
        ))}
      </div>

      <div class="grid md:grid-cols-3 gap-4">
        <button class="btn btn-primary" ref={el => reveal(el, () => ({}))} onClick={() => navigate('/onboarding')}>Онбординг</button>
        <button class="btn" ref={el => reveal(el, () => ({ delayMs: 100 }))} onClick={() => navigate('/integrations')}>Интеграции</button>
        <button class="btn" ref={el => reveal(el, () => ({ delayMs: 200 }))} onClick={() => navigate('/settings')}>Настройки бота</button>
      </div>

      <div class="glass p-5" ref={el => reveal(el, () => ({}))}>
        <div class="mb-3 text-xl">Активность</div>
        <div class="grid md:grid-cols-2 gap-4">
          <div class="card-secondary p-4">
            <div class="text-slate-300 text-sm mb-2">Онлайн</div>
            <div class="h-32 bg-slate-800/60 rounded flex items-center justify-center text-slate-500">График (скоро)</div>
          </div>
          <div class="card-secondary p-4">
            <div class="text-slate-300 text-sm mb-2">Команды/ИИ</div>
            <div class="h-32 bg-slate-800/60 rounded flex items-center justify-center text-slate-500">Статистика (скоро)</div>
          </div>
        </div>
      </div>
    </div>
  )
}


