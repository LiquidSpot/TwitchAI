import { jwtDecode } from 'jwt-decode'
import { useNavigate } from '@solidjs/router'
import { createStore } from 'solid-js/store'
import api from '../lib/api'
import { getCurrentUserId } from '../lib/user'
import { reveal } from '../utils/reveal'


type JwtPayload = { email?: string; sub?: string }

export default function Dashboard() {
  const navigate = useNavigate()
  const token = localStorage.getItem('access_token')
  let userEmail: string | undefined
  try { userEmail = token ? (jwtDecode(token) as JwtPayload).email : undefined } catch {}

  const [state, setState] = createStore({
    apiStatus: '—',
    twitchStatus: '—',
    openaiStatus: '—',
    engine: '—',
    replyLimit: '—',
    botStatus: localStorage.getItem('ui_bot_last_reload') ? 'Перезапущен' : '—',
    activityLabels: [] as string[],
    activitySamples: [] as number[],
    activityCommands: [] as string[],
    activityCommandCounts: [] as number[],
  })

  const onlineLast = () => (state.activitySamples.length ? state.activitySamples[state.activitySamples.length - 1] : 0)
  const onlineAvg = () => {
    const a = state.activitySamples
    if (!a.length) return 0
    return Math.round(a.reduce((s, n) => s + n, 0) / a.length)
  }
  const cmdTotal = () => (state.activityCommandCounts.length ? state.activityCommandCounts.reduce((s, n) => s + n, 0) : 0)
  const topCmdIndex = () => {
    const arr = state.activityCommandCounts
    if (!arr.length) return -1
    let idx = 0; let max = -1
    for (let i = 0; i < arr.length; i++) { if (arr[i] > max) { max = arr[i]; idx = i } }
    return max >= 0 ? idx : -1
  }

  ;(async () => {
    const userId = getCurrentUserId()
    try {
      // простая проверка API
      setState('apiStatus', 'OK')
      // конфиг бота
      if (userId) {
        const cfg = await api.get('/v1.0/bot/config', { params: { userId } })
        const r = cfg?.data?.result ?? cfg?.data?.data
        if (r) {
          setState('engine', r.defaultRole || '—')
          setState('replyLimit', String(r.replyLimit ?? '—'))
        }
      }
      // статусы интеграций (как заглушка — наличие clientId / engines)
      try {
        const us = userId ? await api.get('/v1.0/user-settings', { params: { userId } }) : null
        const s = us?.data?.result ?? us?.data?.data
        setState('twitchStatus', s?.twitchClientId ? 'OK' : '—')
      } catch {}
      try {
        const eng = await api.get('/v1.0/ai/engines')
        const list: string[] = eng?.data?.result ?? eng?.data ?? []
        setState('openaiStatus', Array.isArray(list) && list.length > 0 ? 'OK' : '—')
      } catch {}

      // активность для графиков
      if (userId) {
        try {
          const act = await api.get('/v1.0/viewers/activity', { params: { userId } })
          const a = act?.data?.result ?? act?.data?.data
          if (a) {
            setState('activityLabels', Array.isArray(a.labels) ? a.labels : [])
            setState('activitySamples', Array.isArray(a.onlineSamples) ? a.onlineSamples : [])
            setState('activityCommands', Array.isArray(a.aiCommands) ? a.aiCommands : [])
            setState('activityCommandCounts', Array.isArray(a.aiCommandCounts) ? a.aiCommandCounts : [])
          }
        } catch {}
      }
    } catch {}
  })()

  const cards = [
    { title: 'Статус API', value: state.apiStatus },
    { title: 'Twitch интеграция', value: state.twitchStatus },
    { title: 'OpenAI интеграция', value: state.openaiStatus },
    { title: 'Движок', value: state.engine },
    { title: 'Reply limit', value: state.replyLimit },
    { title: 'Статус бота', value: state.botStatus },
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
            <div class="h-32">
              {state.activitySamples.length === 0 ? (
                <svg viewBox="0 0 400 130" class="w-full h-full">
                  <defs>
                    <linearGradient id="dash_grad" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="0%" stop-color="rgba(142,227,245,0.8)" />
                      <stop offset="100%" stop-color="rgba(142,227,245,0.05)" />
                    </linearGradient>
                  </defs>
                  <path d="M0,95 C50,75 100,100 150,85 C200,70 250,92 300,65 C350,45 380,80 400,60 L400,130 L0,130 Z" fill="url(#dash_grad)" />
                  <path d="M0,95 C50,75 100,100 150,85 C200,70 250,92 300,65 C350,45 380,80 400,60" fill="none" stroke="rgba(142,227,245,0.8)" stroke-width="2" />
                </svg>
              ) : (
                <svg viewBox="0 0 400 130" class="w-full h-full">
                  <defs>
                    <linearGradient id="dash_grad2" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="0%" stop-color="rgba(142,227,245,0.8)" />
                      <stop offset="100%" stop-color="rgba(142,227,245,0.05)" />
                    </linearGradient>
                  </defs>
                  {(() => {
                    const pts = state.activitySamples
                    const max = Math.max(...pts, 1)
                    const step = 400 / Math.max(pts.length - 1, 1)
                    const toY = (v: number) => 130 - (v / max) * 100
                    let d = ''
                    pts.forEach((v, i) => {
                      const x = i * step
                      const y = toY(v)
                      d += i === 0 ? `M${x},${y}` : ` L${x},${y}`
                    })
                    const fill = `${d} L400,130 L0,130 Z`
                    return [
                      <path d={fill} fill="url(#dash_grad2)" />,
                      <path d={d} fill="none" stroke="rgba(142,227,245,0.8)" stroke-width="2" />,
                    ]
                  })()}
                </svg>
              )}
            </div>
            <div class="mt-2 flex flex-wrap gap-2 text-xs text-slate-300">
              <span class="inline-flex items-center gap-1 rounded bg-white/10 px-2 py-1">
                <span class="h-2 w-2 rounded-full" style={{ background: 'rgba(142,227,245,0.8)' }} /> Онлайн
              </span>
              <span class="rounded bg-white/5 px-2 py-1">Период: 1 ч</span>
              <span class="rounded bg-white/5 px-2 py-1">Точек: {state.activitySamples.length || 12}</span>
              <span class="rounded bg-white/5 px-2 py-1">Сейчас: {onlineLast()}</span>
              <span class="rounded bg-white/5 px-2 py-1">Среднее: {onlineAvg()}</span>
            </div>
          </div>
          <div class="card-secondary p-4">
            <div class="text-slate-300 text-sm mb-2">Команды/ИИ</div>
            {(state.activityCommands.length === 0 || state.activityCommandCounts.length === 0) ? (
              <svg viewBox="0 0 400 130" class="w-full h-32">
                <rect x="20" y="40" width="40" height="70" fill="rgba(142,227,245,0.4)" />
                <rect x="80" y="20" width="40" height="90" fill="rgba(142,227,245,0.6)" />
                <rect x="140" y="60" width="40" height="50" fill="rgba(142,227,245,0.4)" />
                <rect x="200" y="35" width="40" height="75" fill="rgba(142,227,245,0.7)" />
                <rect x="260" y="80" width="40" height="30" fill="rgba(142,227,245,0.3)" />
                <rect x="320" y="55" width="40" height="55" fill="rgba(142,227,245,0.5)" />
              </svg>
            ) : (
              <svg viewBox="0 0 400 130" class="w-full h-32">
                {(() => {
                  const max = Math.max(...state.activityCommandCounts, 1)
                  const barW = 400 / Math.max(state.activityCommandCounts.length * 1.5, 1)
                  return state.activityCommandCounts.map((cnt, i) => {
                    const x = 10 + i * (barW * 1.5)
                    const h = (cnt / max) * 100
                    const y = 120 - h
                    return <rect x={x} y={y} width={barW} height={h} fill="rgba(142,227,245,0.6)" />
                  })
                })()}
              </svg>
            )}
            <div class="mt-2 flex flex-wrap gap-2 text-xs text-slate-300">
              <span class="inline-flex items-center gap-1 rounded bg-white/10 px-2 py-1">
                <span class="h-2 w-2 rounded-sm" style={{ background: 'rgba(142,227,245,0.6)' }} /> Использования
              </span>
              <span class="rounded bg-white/5 px-2 py-1">Период: сегодня</span>
              <span class="rounded bg-white/5 px-2 py-1">Команд: {state.activityCommands.length || 0}</span>
              <span class="rounded bg-white/5 px-2 py-1">Всего: {cmdTotal()}</span>
              {topCmdIndex() >= 0 && (
                <span class="rounded bg-white/5 px-2 py-1">Топ: {state.activityCommands[topCmdIndex()] ?? ''} — {state.activityCommandCounts[topCmdIndex()] ?? 0}</span>
              )}
            </div>
            <div class="mt-2 text-xs text-slate-300">
              {state.activityCommands.length && state.activityCommandCounts.length ? (
                <ul class="space-y-1 max-h-24 overflow-auto">
                  {state.activityCommands.map((c, i) => (
                    <li class="flex items-center justify-between gap-2">
                      <span class="inline-flex items-center gap-2">
                        <span class="h-2 w-2 rounded-sm" style={{ background: 'rgba(142,227,245,0.6)' }} />
                        <code>{c}</code>
                      </span>
                      <span class="rounded bg-white/5 px-2 py-0.5">{state.activityCommandCounts[i] ?? 0}</span>
                    </li>
                  ))}
                </ul>
              ) : (
                <div class="text-slate-400">Популярные: !ai, !engine, !reply-limit, !sound…</div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}


