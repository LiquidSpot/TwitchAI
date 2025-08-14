import { createResource, createSignal, Show } from 'solid-js'
import api from '../lib/api'

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
  const [userId, setUserId] = createSignal('')
  const [settings, { mutate, refetch }] = createResource(async () => {
    if (!userId()) return null
    const { data } = await api.get('/v1/bot/config', { params: { userId: userId() } })
    return data?.result as BotSettings
  })

  const save = async () => {
    if (!userId()) return
    const { data } = await api.put('/v1/bot/config', {
      userId: userId(),
      ...settings()
    })
    mutate(data?.result)
  }

  return (
    <div class="max-w-5xl mx-auto p-6 space-y-6">
      <div class="glass p-4">
        <h2 class="text-xl mb-3">Идентификатор пользователя</h2>
        <input class="input" placeholder="UserId (GUID)" value={userId()} onInput={e => setUserId(e.currentTarget.value)} />
        <button class="btn mt-3" onClick={() => refetch()}>Загрузить</button>
      </div>

      <Show when={settings()}>
        <div class="glass p-4 space-y-3">
          <h2 class="text-xl">Настройки бота</h2>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
            <label class="space-y-1">
              <div class="text-sm">Роль по умолчанию</div>
              <input class="input" value={settings()!.defaultRole} onInput={e => mutate({ ...settings()!, defaultRole: e.currentTarget.value })} />
            </label>
            <label class="space-y-1">
              <div class="text-sm">Cooldown (сек)</div>
              <input class="input" type="number" value={settings()!.cooldownSeconds} onInput={e => mutate({ ...settings()!, cooldownSeconds: +e.currentTarget.value })} />
            </label>
            <label class="space-y-1">
              <div class="text-sm">Reply limit</div>
              <input class="input" type="number" value={settings()!.replyLimit} onInput={e => mutate({ ...settings()!, replyLimit: +e.currentTarget.value })} />
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
                <input type="checkbox" checked={Boolean(settings() && (settings() as BotSettings)[key])} onChange={e => mutate((prev) => ({ ...(prev as BotSettings), [key]: e.currentTarget.checked } as BotSettings))} />
                <span>{label}</span>
              </label>
            ))}
          </div>

          <div class="pt-2">
            <button class="btn" onClick={save}>Сохранить</button>
          </div>
        </div>
      </Show>
    </div>
  )
}


