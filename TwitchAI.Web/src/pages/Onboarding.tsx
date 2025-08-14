import { createStore } from 'solid-js/store'
import { onMount } from 'solid-js'
import { jwtDecode } from 'jwt-decode'
import api from '../lib/api'
import { reveal } from '../utils/reveal'

type Step = 0 | 1 | 2 | 3 | 4 | 5

export default function Onboarding() {
  const [state, setState] = createStore({
    step: 0 as Step,
    userId: '' as string,
    twitchToken: '',
    twitchClientId: '',
    openaiToken: '',
    openAiOrganizationId: '',
    openAiProjectId: '',
    botRole: 'bot',
    cooldownSeconds: 25,
    uploading: false,
    uploadProgress: 0,
    checkTwitch: null as null | boolean,
    checkOpenAI: null as null | boolean,
    saving: false as boolean,
    saveOk: null as null | boolean,
  })

  const saveDraft = () => {
    try { localStorage.setItem('onboarding_draft', JSON.stringify(state)) } catch {}
  }

  // черновик в localStorage
  onMount(() => {
    const saved = localStorage.getItem('onboarding_draft')
    if (saved) {
      try { setState(JSON.parse(saved)) } catch {}
    }
    // получить userId из access_token
    try {
      const t = localStorage.getItem('access_token')
      if (t) {
        const payload = jwtDecode<{ sub?: string }>(t)
        if (payload?.sub) setState('userId', payload.sub)
      }
    } catch {}
    saveDraft()
  })

  const total = 6
  const next = () => { setState('step', Math.min(state.step + 1, total - 1) as Step); saveDraft() }
  const prev = () => { setState('step', Math.max(state.step - 1, 0) as Step); saveDraft() }

  const doCheckTwitch = async () => {
    try {
      const { data } = await api.post('/v1.0/user-settings/check/twitch', {
        accessToken: state.twitchToken,
        clientId: state.twitchClientId,
      })
      setState('checkTwitch', !!(data?.result ?? data?.data ?? data?.ok))
    } catch { setState('checkTwitch', false) }
  }
  const doCheckOpenAI = async () => {
    try {
      const { data } = await api.post('/v1.0/user-settings/check/openai', {
        apiKey: state.openaiToken,
        organizationId: state.openAiOrganizationId || null,
        projectId: state.openAiProjectId || null,
      })
      setState('checkOpenAI', !!(data?.result ?? data?.data ?? data?.ok))
    } catch { setState('checkOpenAI', false) }
  }

  const saveAll = async () => {
    if (!state.userId) return
    setState('saving', true)
    setState('saveOk', null)
    try {
      const body = {
        userId: state.userId,
        twitchChannelName: null,
        twitchBotUsername: null,
        twitchAccessToken: state.twitchToken || null,
        twitchRefreshToken: null,
        twitchClientId: state.twitchClientId || null,
        openAiOrganizationId: state.openAiOrganizationId || null,
        openAiProjectId: state.openAiProjectId || null,
        openAiApiKey: state.openaiToken || null,
      }
      const { data } = await api.post('/v1.0/user-settings', body)
      setState('saveOk', !!(data?.success ?? data?.ok ?? data?.result))
    } catch {
      setState('saveOk', false)
    } finally {
      setState('saving', false)
    }
  }

  return (
    <div class="max-w-3xl mx-auto p-6 space-y-6">
      <div class="glass p-4" ref={el => reveal(el, () => ({}))}>
        <div class="mb-2 text-xl">Онбординг</div>
        <div class="h-2 bg-slate-800 rounded">
          <div class="h-2 bg-accent rounded" style={{ width: `${((state.step + 1) / total) * 100}%` }} />
        </div>
      </div>

      {state.step === 0 && (
        <div class="glass p-4 space-y-3" ref={el => reveal(el, () => ({}))}>
          <div class="text-lg">Профиль</div>
          <div class="text-slate-300">Войдите в аккаунт и продолжайте настройку интеграций.</div>
          <div class="flex gap-2">
            <button class="btn" onClick={next}>Далее</button>
          </div>
        </div>
      )}

      {state.step === 1 && (
        <div class="glass p-4 space-y-3" ref={el => reveal(el, () => ({}))}>
          <div class="text-lg">Twitch токен</div>
          <input class="input" type="text" placeholder="Twitch Client ID" value={state.twitchClientId} onInput={e => { setState('twitchClientId', e.currentTarget.value); saveDraft() }} />
          <input class="input" type="password" placeholder="Twitch Access Token ••••••" value={state.twitchToken} onInput={e => { setState('twitchToken', e.currentTarget.value); saveDraft() }} />
          <div class="flex items-center gap-2">
            <button class="btn btn-secondary" onClick={doCheckTwitch}>Проверить</button>
            {state.checkTwitch !== null && <span class={state.checkTwitch ? 'text-green-400' : 'text-red-400'}>{state.checkTwitch ? 'OK' : 'Ошибка'}</span>}
          </div>
          <div class="flex gap-2">
            <button class="btn" onClick={prev}>Назад</button>
            <button class="btn btn-primary" onClick={next} disabled={!state.twitchToken || !state.twitchClientId}>Далее</button>
          </div>
        </div>
      )}

      {state.step === 2 && (
        <div class="glass p-4 space-y-3" ref={el => reveal(el, () => ({}))}>
          <div class="text-lg">OpenAI токен</div>
          <input class="input" type="text" placeholder="OpenAI Organization Id (необязательно)" value={state.openAiOrganizationId} onInput={e => { setState('openAiOrganizationId', e.currentTarget.value); saveDraft() }} />
          <input class="input" type="text" placeholder="OpenAI Project Id (необязательно)" value={state.openAiProjectId} onInput={e => { setState('openAiProjectId', e.currentTarget.value); saveDraft() }} />
          <input class="input" type="password" placeholder="OpenAI API Key ••••••" value={state.openaiToken} onInput={e => { setState('openaiToken', e.currentTarget.value); saveDraft() }} />
          <div class="flex items-center gap-2">
            <button class="btn btn-secondary" onClick={doCheckOpenAI}>Проверить</button>
            {state.checkOpenAI !== null && <span class={state.checkOpenAI ? 'text-green-400' : 'text-red-400'}>{state.checkOpenAI ? 'OK' : 'Ошибка'}</span>}
          </div>
          <div class="flex gap-2">
            <button class="btn" onClick={prev}>Назад</button>
            <button class="btn btn-primary" onClick={next} disabled={!state.openaiToken}>Далее</button>
          </div>
        </div>
      )}

      {state.step === 3 && (
        <div class="glass p-4 space-y-3" ref={el => reveal(el, () => ({}))}>
          <div class="text-lg">Настройки бота</div>
          <div class="grid md:grid-cols-2 gap-3">
            <label class="space-y-1">
              <div class="text-sm">Роль</div>
              <input class="input" value={state.botRole} onInput={e => { setState('botRole', e.currentTarget.value); saveDraft() }} />
            </label>
            <label class="space-y-1">
              <div class="text-sm">Cooldown (сек)</div>
              <input class="input" type="number" value={state.cooldownSeconds} onInput={e => { setState('cooldownSeconds', +e.currentTarget.value); saveDraft() }} />
            </label>
          </div>
          <div class="flex gap-2">
            <button class="btn" onClick={prev}>Назад</button>
            <button class="btn btn-primary" onClick={next}>Далее</button>
          </div>
        </div>
      )}

      {state.step === 4 && (
        <div class="glass p-4 space-y-3" ref={el => reveal(el, () => ({}))}>
          <div class="text-lg">Загрузка звуков</div>
          <div class="h-28 bg-slate-800/60 rounded flex items-center justify-center text-slate-500">Drag & Drop (скоро)</div>
          <div class="flex gap-2">
            <button class="btn" onClick={prev}>Назад</button>
            <button class="btn btn-primary" onClick={next}>Далее</button>
          </div>
        </div>
      )}

      {state.step === 5 && (
        <div class="glass p-4 space-y-3" ref={el => reveal(el, () => ({}))}>
          <div class="text-lg">Финиш</div>
          <div class="text-slate-300">Проверьте подключения и запустите бота. Настройки можно менять в любое время.</div>
          <div class="flex items-center gap-2">
            <button class="btn" onClick={prev} disabled={state.saving}>Назад</button>
            <button class="btn btn-primary" onClick={saveAll} disabled={state.saving || !state.userId}>Сохранить</button>
            {state.saveOk === true && <span class="text-green-400">Сохранено</span>}
            {state.saveOk === false && <span class="text-red-400">Ошибка сохранения</span>}
          </div>
        </div>
      )}
    </div>
  )
}


