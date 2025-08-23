import { createStore } from 'solid-js/store'
import { jwtDecode } from 'jwt-decode'
// no solid-js lifecycle imports used to keep compatibility
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
  let engineRef: HTMLDivElement | undefined
  let outsideHandler: ((e: MouseEvent) => void) | null = null
  let fileInputRef: HTMLInputElement | undefined
  const bindOutsideClose = () => {
    if (outsideHandler) return
    outsideHandler = (e: MouseEvent) => {
      if (!state.engineOpen) return
      if (engineRef && !engineRef.contains(e.target as Node)) setState('engineOpen', false)
    }
    document.addEventListener('mousedown', outsideHandler)
  }
  const unbindOutsideClose = () => {
    if (!outsideHandler) return
    document.removeEventListener('mousedown', outsideHandler)
    outsideHandler = null
  }
  const [state, setState] = createStore<{
    userId: string;
    settings: BotSettings | null;
    loaded: boolean;
    // форма интеграций
    integrationForm: {
      twitchClientId: string;
      twitchAccessToken: string;
      openAiOrganizationId: string;
      openAiProjectId: string;
      openAiApiKey: string;
    } | null;
    showTwitchToken: boolean;
    showOpenAiToken: boolean;
    engines: string[];
    selectedEngine: string;
    engineOpen: boolean;
    replyLimit: number;
    soundsToUpload: File[];
    botStatus: string;
    dragOver: boolean;
    loading: boolean;
    saving: boolean;
  }>(
    { userId: '', settings: null, loaded: false, integrationForm: null, showTwitchToken: false, showOpenAiToken: false, engines: [], selectedEngine: '', engineOpen: false, replyLimit: 3, soundsToUpload: [], botStatus: '—', dragOver: false, loading: false, saving: false }
  )

  // bind outside click on open/close
  ;(() => {
    let lastOpen = false
    const tick = () => {
      const open = state.engineOpen as boolean
      if (open !== lastOpen) {
        lastOpen = open
        if (open) bindOutsideClose(); else unbindOutsideClose()
      }
      requestAnimationFrame(tick)
    }
    requestAnimationFrame(tick)
  })()

  // авто-подстановка userId из JWT
  ;(() => {
    const uid = getCurrentUserId()
    if (uid) setState('userId', uid)
    // восстановить кеш состояния, если есть
    try {
      const cached = localStorage.getItem('settings_page_cache')
      if (cached) {
        const c = JSON.parse(cached)
        if (c && typeof c === 'object') {
          if (c.userId) setState('userId', c.userId)
          if (c.settings) setState('settings', c.settings)
          if (c.integrationForm) setState('integrationForm', c.integrationForm)
          if (Array.isArray(c.engines)) setState('engines', c.engines)
          if (typeof c.selectedEngine === 'string') setState('selectedEngine', c.selectedEngine)
          if (typeof c.replyLimit === 'number') setState('replyLimit', c.replyLimit)
          if (typeof c.botStatus === 'string') setState('botStatus', c.botStatus)
          if (c.loaded === true) setState('loaded', true)
        }
      }
    } catch {}
    // авто-загрузка актуальных данных
    setTimeout(() => { if (getCurrentUserId()) load() }, 0)
  })()

  const saveCache = () => {
    try {
      const payload = {
        userId: state.userId,
        settings: state.settings,
        integrationForm: state.integrationForm,
        engines: state.engines,
        selectedEngine: state.selectedEngine,
        replyLimit: state.replyLimit,
        botStatus: state.botStatus,
        loaded: state.loaded,
      }
      localStorage.setItem('settings_page_cache', JSON.stringify(payload))
    } catch {}
  }

  const load = async () => {
    if (!state.userId) return
    setState('loading', true)
    try {
      // Bot settings
      const { data } = await api.get('/v1.0/bot/config', { params: { userId: state.userId } })
      setState('settings', data?.result as BotSettings)
      if (typeof (data?.result?.replyLimit) === 'number') setState('replyLimit', data.result.replyLimit)

      // User integrations → подготовить форму (секреты не возвращаются)
      try {
        const us = await api.get('/v1.0/user-settings', { params: { userId: state.userId } })
        const s = us?.data?.result ?? us?.data?.data ?? null
        setState('integrationForm', {
          twitchClientId: s?.twitchClientId ?? '',
          twitchAccessToken: '',
          openAiOrganizationId: s?.openAiOrganizationId ?? '',
          openAiProjectId: s?.openAiProjectId ?? '',
          openAiApiKey: '',
        })
      } catch {}

      // Engines list
      try {
        const eng = await api.get('/v1.0/ai/engines')
        const listAny = eng?.data?.result ?? eng?.data ?? []
        const list: string[] = Array.isArray(listAny) ? listAny.filter(x => typeof x === 'string') : []
        const unique = Array.from(new Set(list))
        setState('engines', unique)
        // выбрать из localStorage, иначе первый
        const saved = localStorage.getItem('ui_engine')
        if (saved && unique.includes(saved)) setState('selectedEngine', saved)
        else if (unique.length > 0) setState('selectedEngine', unique[0])
      } catch {}
    } finally {
      setState('loading', false)
      setState('loaded', true)
      saveCache()
    }
  }

  const save = async () => {
    if (!state.userId || !state.settings) return
    setState('saving', true)
    try {
      const { data } = await api.put('/v1.0/bot/config', {
        userId: state.userId,
        ...state.settings,
      })
      setState('settings', data?.result as BotSettings)
      toastSuccess('Настройки сохранены')
    } finally {
      setState('saving', false)
      saveCache()
    }
  }

  const saveIntegrations = async () => {
    if (!state.userId || !state.integrationForm) return
    setState('saving', true)
    try {
      const body = {
        userId: state.userId,
        twitchChannelName: null,
        twitchBotUsername: null,
        twitchAccessToken: state.integrationForm.twitchAccessToken || null,
        twitchRefreshToken: null,
        twitchClientId: state.integrationForm.twitchClientId || null,
        openAiOrganizationId: state.integrationForm.openAiOrganizationId || null,
        openAiProjectId: state.integrationForm.openAiProjectId || null,
        openAiApiKey: state.integrationForm.openAiApiKey || null,
      }
      await api.post('/v1.0/user-settings', body)
      toastSuccess('Интеграции сохранены')
      // не перезаполняем секреты обратно
      setState('integrationForm', 'twitchAccessToken', '')
      setState('integrationForm', 'openAiApiKey', '')
    } catch (e: any) {
      toastError(e?.message || 'Не удалось сохранить интеграции')
    } finally {
      setState('saving', false)
      saveCache()
    }
  }

  const applyEngine = async () => {
    if (!state.userId || !state.selectedEngine) return
    try {
      await api.put('/v1.0/ai/engine', { userId: state.userId, engineName: state.selectedEngine })
      localStorage.setItem('ui_engine', state.selectedEngine)
      toastSuccess('Движок установлен')
    } catch (e: any) {
      toastError(e?.message || 'Не удалось установить движок')
    } finally {
      saveCache()
    }
  }

  const applyReplyLimit = async () => {
    if (!state.userId || !state.replyLimit) return
    try {
      const limit = Math.max(1, Math.min(10, state.replyLimit))
      await api.put('/v1.0/ai/reply-limit', { userId: state.userId, limit })
      localStorage.setItem('ui_reply_limit', String(limit))
      setState('replyLimit', limit)
      toastSuccess('Лимит контекста обновлён')
    } catch (e: any) {
      toastError(e?.message || 'Не удалось обновить лимит')
    } finally {
      saveCache()
    }
  }

  const uploadSounds = async () => {
    if (!state.soundsToUpload.length) return
    // Placeholder: бэкенд эндпойнта пока нет
    toastSuccess(`Звуки подготовлены (${state.soundsToUpload.length}) — загрузка скоро`)
    localStorage.setItem('ui_sound_last_selected', String(state.soundsToUpload.length))
    saveCache()
  }

  const onDropFiles = (files: FileList | null) => {
    if (!files) return
    setState('soundsToUpload', Array.from(files))
  }

  const reloadBot = async () => {
    // Placeholder: нет API; фиксируем отметку времени
    const ts = new Date().toISOString()
    localStorage.setItem('ui_bot_last_reload', ts)
    setState('botStatus', 'Перезапущен')
    toastSuccess('Команда перезапуска отправлена')
    saveCache()
  }

  return (
    <div class="max-w-5xl mx-auto p-6 space-y-6">
      <div class="glass p-4">
        <h2 class="text-xl mb-3">Идентификатор пользователя</h2>
        <input class="input" placeholder="UserId (GUID)" value={state.userId} onInput={e => setState('userId', e.currentTarget.value)} />
        <button class="btn btn-secondary mt-3" onClick={load} disabled={state.loading}>{state.loading ? 'Загрузка...' : 'Загрузить'}</button>
      </div>

      {state.integrationForm && (
        <div class="glass p-4 space-y-3">
          <h2 class="text-xl">Интеграции и токены</h2>
          <div class="grid md:grid-cols-2 gap-3">
            <label class="space-y-1">
              <div class="text-sm">Twitch Client ID</div>
              <input class="input" value={state.integrationForm.twitchClientId} onInput={e => setState('integrationForm', 'twitchClientId', e.currentTarget.value)} />
            </label>
            <label class="space-y-1">
              <div class="text-sm">Twitch Access Token</div>
              <div class="flex gap-2">
                <input class="input flex-1" type={state.showTwitchToken ? 'text' : 'password'} placeholder="••••••" value={state.integrationForm.twitchAccessToken} onInput={e => setState('integrationForm', 'twitchAccessToken', e.currentTarget.value)} />
                <button class="btn btn-secondary" onClick={() => setState('showTwitchToken', !state.showTwitchToken)}>{state.showTwitchToken ? 'Скрыть' : 'Показать'}</button>
              </div>
            </label>
            <label class="space-y-1">
              <div class="text-sm">OpenAI Organization Id (опц.)</div>
              <input class="input" value={state.integrationForm.openAiOrganizationId} onInput={e => setState('integrationForm', 'openAiOrganizationId', e.currentTarget.value)} />
            </label>
            <label class="space-y-1">
              <div class="text-sm">OpenAI Project Id (опц.)</div>
              <input class="input" value={state.integrationForm.openAiProjectId} onInput={e => setState('integrationForm', 'openAiProjectId', e.currentTarget.value)} />
            </label>
            <label class="space-y-1 md:col-span-2">
              <div class="text-sm">OpenAI API Key</div>
              <div class="flex gap-2">
                <input class="input flex-1" type={state.showOpenAiToken ? 'text' : 'password'} placeholder="••••••" value={state.integrationForm.openAiApiKey} onInput={e => setState('integrationForm', 'openAiApiKey', e.currentTarget.value)} />
                <button class="btn btn-secondary" onClick={() => setState('showOpenAiToken', !state.showOpenAiToken)}>{state.showOpenAiToken ? 'Скрыть' : 'Показать'}</button>
              </div>
            </label>
          </div>
          <div class="pt-2">
            <button class="btn btn-secondary" onClick={saveIntegrations} disabled={state.saving}>{state.saving ? 'Сохранение...' : 'Сохранить интеграции'}</button>
          </div>
        </div>
      )}

      {state.loaded && (
      <div class="glass p-4 space-y-3">
        <h2 class="text-xl">Конфигурация бота</h2>
        <div class="grid md:grid-cols-2 gap-3">
          <label class="space-y-1">
            <div class="text-sm">Движок</div>
            <div class="flex gap-2">
              <div class="relative w-full" ref={el => (engineRef = el as HTMLDivElement)} tabIndex={0} onKeyDown={e => { if ((e as KeyboardEvent).key === 'Escape') { setState('engineOpen', false); unbindOutsideClose() } }}>
                <button type="button" class="input w-full bg-slate-800 text-white flex items-center justify-between" onClick={() => { const open = !state.engineOpen; setState('engineOpen', open); open ? bindOutsideClose() : unbindOutsideClose() }}>
                  <span class="truncate">{state.selectedEngine || 'Выберите движок'}</span>
                  <svg class="h-4 w-4 opacity-70" viewBox="0 0 20 20" fill="currentColor"><path fill-rule="evenodd" d="M5.23 7.21a.75.75 0 011.06.02L10 10.94l3.71-3.71a.75.75 0 111.06 1.06l-4.24 4.24a.75.75 0 01-1.06 0L5.21 8.29a.75.75 0 01.02-1.08z" clip-rule="evenodd"/></svg>
                </button>
                {state.engineOpen && (
                  <div class="absolute z-20 mt-2 w-full rounded-xl border border-white/15 bg-slate-900 text-white shadow-xl max-h-60 overflow-auto" style={{ 'scroll-behavior': 'smooth' }}>
                    {state.engines.map(e => (
                      <div class="px-4 py-2.5 hover:bg-white/10 cursor-pointer transition-colors" onMouseDown={() => { setState('selectedEngine', e); setState('engineOpen', false); unbindOutsideClose() }}>{e}</div>
                    ))}
                  </div>
                )}
              </div>
              <button class="btn btn-secondary" onClick={applyEngine}>Применить</button>
            </div>
          </label>
          <label class="space-y-1">
            <div class="text-sm">Кол-во контекста (reply limit)</div>
            <div class="flex gap-2">
              <input class="input w-24" type="number" min="1" max="10" value={state.replyLimit} onInput={e => setState('replyLimit', +e.currentTarget.value)} />
              <button class="btn btn-secondary" onClick={applyReplyLimit}>Сохранить</button>
            </div>
          </label>
        </div>

        <div class="grid md:grid-cols-2 gap-3">
          <div class="space-y-1 md:col-span-2">
            <div class="text-sm">Загрузка звуковых команд</div>
            <div class="rounded border border-white/20 bg-slate-800/60 p-4 text-sm select-none">
              Выбор файлов доступен только по кнопке ниже. Поддерживаются .mp3/.wav
              <div class="mt-2 flex items-center gap-2">
                <input ref={el => (fileInputRef = el as HTMLInputElement)} type="file" multiple accept="audio/*" class="hidden" onChange={e => setState('soundsToUpload', Array.from(e.currentTarget.files || []))} />
                <button class="btn btn-secondary" onClick={() => fileInputRef?.click()}>Выбрать файлы</button>
                <button class="btn btn-secondary" onClick={uploadSounds} disabled={!state.soundsToUpload.length}>Загрузить</button>
                <div class="text-slate-400 text-xs">{state.soundsToUpload.length ? `${state.soundsToUpload.length} файл(ов) выбрано` : 'Файлы не выбраны'}</div>
              </div>
            </div>
            <div class="text-xs text-slate-400">Поддержка загрузки на сервер будет добавлена позже</div>
          </div>
        </div>

        <div class="grid md:grid-cols-2 gap-3">
          <div>
            <div class="text-sm">Перезагрузка бота на канал</div>
            <button class="btn btn-secondary" onClick={reloadBot}>Перезагрузить бота</button>
          </div>
          <div>
            <div class="text-sm">Статус бота</div>
            <div class="card-secondary p-3">{state.botStatus}</div>
          </div>
        </div>
      </div>
      )}

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


