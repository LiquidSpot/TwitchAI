import { createStore } from 'solid-js/store'
import api from '../lib/api'
import { toastError, toastSuccess } from '../lib/toast'
import { getCurrentUserId } from '../lib/user'

type BotSettings = {
  appUserId: string
  defaultRole: string
  cooldownSeconds: number
  replyLimit: number
  enableAi: boolean
  enableCompliment: boolean
  enableFact: boolean
  enableHoliday: boolean
  enableTranslation: boolean
  enableSoundAlerts: boolean
  enableViewersStats: boolean
}

type FlagKey = 'enableAi' | 'enableCompliment' | 'enableFact' | 'enableHoliday' | 'enableTranslation' | 'enableSoundAlerts' | 'enableViewersStats'

export default function Settings() {
  const [state, setState] = createStore<{ userId: string; settings: BotSettings | null; loading: boolean; saving: boolean }>(
    { userId: '', settings: null, loading: false, saving: false }
  )

  // авто-подстановка userId из JWT
  ;(() => {
    const uid = getCurrentUserId()
    if (uid) setState('userId', uid)
  })()

  const load = async () => {
    if (!state.userId) return
    setState('loading', true)
    try {
      const { data } = await api.get('/v1/bot/config', { params: { userId: state.userId } })
      setState('settings', data?.result as BotSettings)
    } finally {
      setState('loading', false)
    }
  }

  const save = async () => {
    if (!state.userId || !state.settings) return
    setState('saving', true)
    try {
      const { data } = await api.put('/v1/bot/config', {
        userId: state.userId,
        ...state.settings,
      })
      setState('settings', data?.result as BotSettings)
      toastSuccess('Настройки сохранены')
    } finally {
      setState('saving', false)
    }
  }

  return (
    <div class="max-w-5xl mx-auto p-6 space-y-6">
      <div class="glass p-4">
        <h2 class="text-xl mb-3">Идентификатор пользователя</h2>
        <input class="input" placeholder="UserId (GUID)" value={state.userId} onInput={e => setState('userId', e.currentTarget.value)} />
        <button class="btn btn-secondary mt-3" onClick={load} disabled={state.loading}>{state.loading ? 'Загрузка...' : 'Загрузить'}</button>
      </div>

      {state.settings && (
        <div class="glass p-4 space-y-3">
          <h2 class="text-xl">Настройки бота</h2>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
            <label class="space-y-1">
              <div class="text-sm">Роль по умолчанию</div>
              <input class="input" value={state.settings.defaultRole} onInput={e => setState('settings', 'defaultRole', e.currentTarget.value)} />
            </label>
            <label class="space-y-1">
              <div class="text-sm">Cooldown (сек)</div>
              <input class="input" type="number" value={state.settings.cooldownSeconds} onInput={e => setState('settings', 'cooldownSeconds', +e.currentTarget.value)} />
            </label>
            <label class="space-y-1">
              <div class="text-sm">Reply limit</div>
              <input class="input" type="number" value={state.settings.replyLimit} onInput={e => setState('settings', 'replyLimit', +e.currentTarget.value)} />
            </label>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-3 gap-3">
            {([
              ['enableAi','AI'],
              ['enableCompliment','Compliment'],
              ['enableFact','Fact'],
              ['enableHoliday','Holiday'],
              ['enableTranslation','Translation'],
              ['enableSoundAlerts','Sound Alerts'],
              ['enableViewersStats','Viewers Stats']
            ] as Array<[FlagKey, string]>).map(([key, label]) => (
              <label class="flex items-center gap-2">
                <input type="checkbox" checked={Boolean(state.settings && (state.settings as BotSettings)[key])} onChange={e => setState('settings', key as any, e.currentTarget.checked)} />
                <span>{label}</span>
              </label>
            ))}
          </div>

          <div class="pt-2">
            <button class="btn btn-primary" onClick={save} disabled={state.saving}>{state.saving ? 'Сохранение...' : 'Сохранить'}</button>
          </div>
        </div>
      )}
    </div>
  )
}


