import { createStore } from 'solid-js/store'
import { jwtDecode } from 'jwt-decode'
import { getCurrentUserId } from '../lib/user'
import api from '../lib/api'
import { reveal } from '../utils/reveal'
import { toastError, toastInfo, toastSuccess } from '../lib/toast'
import twitchHelp1 from '../img/twitch_help_1.jpg'
import twitchHelp2 from '../img/twitch_help_2.jpg'
import twitchHelp3 from '../img/twitch_help_3.jpg'
import twitchHelpProd1 from '../img/twitch_help_prod_1.jpg'
import twitchHelpProd2 from '../img/twitch_help_prod_2.jpg'
import twitchHelpProd3 from '../img/twitch_help_prod_3.jpg'
import openaiHelp3 from '../img/openai_help_3.jpg'
import openaiHelp4 from '../img/openai_help_4.jpg'
import openaiHelp5 from '../img/openai_help_5.jpg'

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
    twitchChecking: false as boolean,
    openaiChecking: false as boolean,
    twitchPulse: false as boolean,
    openaiPulse: false as boolean,
    twitchSuccess: false as boolean,
    openaiSuccess: false as boolean,
    saving: false as boolean,
    saveOk: null as null | boolean,
    activityLoading: false as boolean,
    activityLabels: [] as string[],
    activitySamples: [] as number[],
    activityCommands: [] as string[],
  })

  const saveDraft = () => {
    try { localStorage.setItem('onboarding_draft', JSON.stringify(state)) } catch {}
  }

  // инициализация: загрузить черновик и userId
  ;(() => {
    try {
      const saved = localStorage.getItem('onboarding_draft')
      if (saved) {
        try { setState(JSON.parse(saved)) } catch {}
      }
      const uid0 = getCurrentUserId()
      if (uid0) {
        setState('userId', uid0)
        // пропускаем шаг 0, т.к. пользователь уже авторизован
        setState('step', 1 as Step)
      }
      const uid = (state as any).userId as string
      if (uid) {
        api.get('/v1.0/user-settings', { params: { userId: uid } })
          .then(({ data }) => {
            const s = data?.result ?? data?.data ?? null
            if (s) {
              setState('twitchClientId', s.twitchClientId ?? '')
              setState('openAiOrganizationId', s.openAiOrganizationId ?? '')
              setState('openAiProjectId', s.openAiProjectId ?? '')
            }
          })
          .catch(() => {})

        // подтянуть текущие настройки бота
        api.get('/v1.0/bot/config', { params: { userId: uid } })
          .then(({ data }) => {
            const b = data?.result ?? data?.data ?? null
            if (b) {
              if (typeof b.defaultRole === 'string') setState('botRole', b.defaultRole)
              if (typeof b.cooldownSeconds === 'number') setState('cooldownSeconds', b.cooldownSeconds)
            }
          })
          .catch(() => {})

        // подтянуть заглушечную активность
        setState('activityLoading', true)
        api.get('/v1.0/viewers/activity', { params: { userId: uid } })
          .then(({ data }) => {
            const a = data?.result ?? data?.data ?? null
            if (a) {
              setState('activityLabels', Array.isArray(a.labels) ? a.labels : [])
              setState('activitySamples', Array.isArray(a.onlineSamples) ? a.onlineSamples : [])
              setState('activityCommands', Array.isArray(a.aiCommands) ? a.aiCommands : [])
            }
          })
          .catch(() => {})
          .finally(() => setState('activityLoading', false))
      }
      saveDraft()
    } catch {}
  })()

  const total = 6
  const next = () => { setState('step', Math.min(state.step + 1, total - 1) as Step); saveDraft() }
  const prev = () => { setState('step', Math.max(state.step - 1, 1) as Step); saveDraft() }

  const doCheckTwitch = async () => {
    setState('twitchChecking', true)
    try {
      const token = (state.twitchToken || '').trim()
      const client = (state.twitchClientId || '').trim()
      const payload: any = {
        AccessToken: token,
        ClientId: client,
        accessToken: token,
        clientId: client,
      }
      const { data } = await api.post('/v1.0/user-settings/check/twitch', payload, { headers: { 'Content-Type': 'application/json' }})
      const val = (data?.result ?? data?.data ?? data) as any
      const ok = val && ((typeof val === 'string' && val.toLowerCase() === 'ok') || (typeof val === 'object' && val?.ok === true))
      setState('checkTwitch', !!ok)
      if (ok) {
        toastInfo({ title: 'Twitch', text: 'Токен принят', position: 'top-right' })
        setState('twitchSuccess', true)
        setTimeout(() => setState('twitchSuccess', false), 1000)
      } else {
        toastError({ title: 'Twitch', text: 'Не удалось проверить токен', position: 'top-right' })
        setState('twitchPulse', true)
        setTimeout(() => setState('twitchPulse', false), 1000)
      }
    } catch (err: any) {
      setState('checkTwitch', false)
      // не редиректим; только уведомление
      const title = err?.response?.data?.title as string | undefined
      const hasValidation = err?.response?.status === 400 && (!!err?.response?.data?.errors || title?.includes('validat'))
      const msg = hasValidation ? 'Ошибка валидации полей' : (err?.message || 'Не удалось проверить токен')
      toastError({ title: 'Twitch', text: msg, position: 'top-right' })
      setState('twitchPulse', true)
      setTimeout(() => setState('twitchPulse', false), 1000)
    } finally {
      setState('twitchChecking', false)
    }
  }
  const doCheckOpenAI = async () => {
    setState('openaiChecking', true)
    try {
      const key = (state.openaiToken || '').trim()
      const org = (state.openAiOrganizationId || '').trim() || null
      const proj = (state.openAiProjectId || '').trim() || null
      const payload: any = {
        ApiKey: key,
        apiKey: key,
        OrganizationId: org,
        organizationId: org,
        ProjectId: proj,
        projectId: proj,
      }
      const { data } = await api.post('/v1.0/user-settings/check/openai', payload, { headers: { 'Content-Type': 'application/json' }})
      const val = (data?.result ?? data?.data ?? data) as any
      const ok = val && ((typeof val === 'string' && val.toLowerCase() === 'ok') || (typeof val === 'object' && val?.ok === true))
      setState('checkOpenAI', !!ok)
      if (ok) {
        toastInfo({ title: 'OpenAI', text: 'Ключ принят', position: 'top-right' })
        setState('openaiSuccess', true)
        setTimeout(() => setState('openaiSuccess', false), 1000)
      } else {
        toastError({ title: 'OpenAI', text: 'Не удалось проверить ключ', position: 'top-right' })
        setState('openaiPulse', true)
        setTimeout(() => setState('openaiPulse', false), 1000)
      }
    } catch (err: any) {
      setState('checkOpenAI', false)
      const title = err?.response?.data?.title as string | undefined
      const hasValidation = err?.response?.status === 400 && (!!err?.response?.data?.errors || title?.includes('One or more validation errors occurred.'))
      const msg = hasValidation ? 'Ошибка валидации полей' : (err?.response?.data?.errors?.ApiKey?.[0] || err?.message || 'Не удалось проверить ключ')
      toastError({ title: 'OpenAI', text: msg, position: 'top-right' })
      setState('openaiPulse', true)
      setTimeout(() => setState('openaiPulse', false), 1000)
    } finally {
      setState('openaiChecking', false)
    }
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
      try {
        await api.put('/v1.0/bot/config', {
          userId: state.userId,
          defaultRole: state.botRole,
          cooldownSeconds: state.cooldownSeconds,
          replyLimit: 3,
          enableAi: true,
          enableCompliment: true,
          enableFact: true,
          enableHoliday: true,
          enableTranslation: true,
          enableSoundAlerts: true,
          enableViewersStats: true,
        })
      } catch {}
      toastSuccess({ title: 'Онбординг', text: 'Настройки сохранены', position: 'top-right' })
    } catch {
      setState('saveOk', false)
      toastError({ title: 'Онбординг', text: 'Не удалось сохранить настройки', position: 'top-right' })
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
        <div class="glass p-4 space-y-3 relative" ref={el => reveal(el, () => ({}))}>
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
          <input class="input" type="text" placeholder="Twitch Client ID" value={state.twitchClientId} onInput={e => { setState('twitchClientId', e.currentTarget.value); setState('checkTwitch', null); saveDraft() }} />
          <input class="input" type="password" placeholder="Twitch Access Token ••••••" value={state.twitchToken} onInput={e => { setState('twitchToken', e.currentTarget.value); setState('checkTwitch', null); saveDraft() }} />
          <div class="flex items-center gap-2">
            <button disabled={state.twitchChecking} class={`btn btn-secondary ${state.twitchPulse ? '!bg-red-600/60 !border-red-400/60 ring-2 ring-red-400/60' : ''} ${state.twitchSuccess ? '!bg-green-600/60 !border-green-400/60 ring-2 ring-green-400/60' : ''}`} onClick={doCheckTwitch}>
              {state.twitchChecking && (<span class="mr-2 h-4 w-4 rounded-full border-2 border-white/30 border-t-white animate-spin" />)}
              {state.twitchChecking ? 'Проверяем...' : 'Проверить'}
            </button>
            {state.checkTwitch === true && <span class="text-green-400">OK</span>}
          </div>
          <div class="group relative mt-2 text-xs text-slate-200 space-y-2 bg-slate-800/70 rounded-lg p-3 border border-white/20">
            <div class="font-medium">Как получить Twitch токен (для тестирования)</div>
            <ol class="list-decimal pl-4 space-y-1">
              <li>
                Перейдите на
                {' '}<a class="underline text-accent" href="https://twitchtokengenerator.com/" target="_blank" rel="noreferrer">TwitchTokenGenerator</a>
                {' '}и выберите scopes: <code>chat:read</code>, <code>chat:edit</code>, <code>channel:moderate</code> (опц.).
              </li>
              <li>Нажмите «Generate Token», авторизуйтесь через Twitch.</li>
              <li>Скопируйте Access Token и Client ID и вставьте в поля выше.</li>
            </ol>
            {/* tooltips on hover over help block */}
            <div class="pointer-events-none hidden md:block absolute -top-[330px] left-full ml-5 w-[500px] space-y-4 opacity-0 translate-y-1 transition-all duration-[1400ms] ease-[cubic-bezier(0.22,1,0.36,1)] group-hover:opacity-100 group-hover:translate-y-0">
              <div class="glass p-2">
                <div class="text-sm md:text-base mb-1 text-slate-200">Заходим на сайт</div>
                <img src={twitchHelp1} alt="Twitch help 1" class="rounded-lg w-full h-auto max-h-[50vh] object-contain" />
              </div>
              <div class="glass p-2" style={{ 'transition-delay': '120ms' }}>
                <div class="text-sm md:text-base mb-1 text-slate-200">Выбираем права и генерируем токен</div>
                <img src={twitchHelp2} alt="Twitch help 2" class="rounded-lg w-full h-auto max-h-[50vh] object-contain" />
              </div>
              <div class="glass p-2" style={{ 'transition-delay': '240ms' }}>
                <div class="text-sm md:text-base mb-1 text-slate-200">Получаем все необходимые токены</div>
                <img src={twitchHelp3} alt="Twitch help 3" class="rounded-lg w-full h-auto max-h-[50vh] object-contain" />
              </div>
            </div>
          </div>
          <div class="group relative mt-2 text-xs text-slate-200 space-y-2 bg-slate-800/70 rounded-lg p-3 border border-white/20">
            <div class="font-medium">Как получить Twitch токен (для реального проекта)</div>
            <div>
              Используйте {' '}
              <a class="underline text-accent" href="https://dev.twitch.tv/console/apps" target="_blank" rel="noreferrer">Twitch Developer Console</a>
              {' '}и стандартный OAuth‑flow для получения токенов.
            </div>
            {/* prod tooltips */}
            <div class="pointer-events-none hidden md:block absolute -top-[447px] left-full ml-5 w-[360px] space-y-4 opacity-0 translate-y-1 transition-all duration-[1400ms] ease-[cubic-bezier(0.22,1,0.36,1)] group-hover:opacity-100 group-hover:translate-y-0">
              <div class="glass p-2">
                <div class="text-sm md:text-base mb-1 text-slate-200">Создание нового приложения</div>
                <img src={twitchHelpProd1} alt="Twitch prod help 1" class="rounded-lg w-full h-auto max-h-[50vh] object-contain" />
              </div>
              <div class="glass p-2" style={{ 'transition-delay': '120ms' }}>
                <div class="text-sm md:text-base mb-1 text-slate-200">Заполнение данных для приложения</div>
                <img src={twitchHelpProd2} alt="Twitch prod help 2" class="rounded-lg w-full h-auto max-h-[50vh] object-contain" />
              </div>
              <div class="glass p-2" style={{ 'transition-delay': '240ms' }}>
                <div class="text-sm md:text-base mb-1 text-slate-200">Генерация ключа и токена для доступа</div>
                <img src={twitchHelpProd3} alt="Twitch prod help 3" class="rounded-lg w-full h-auto max-h-[50vh] object-contain" />
              </div>
            </div>
          </div>
          <div class="flex gap-2">
            <button class="btn" onClick={prev}>Назад</button>
            <button class="btn btn-primary" onClick={next} disabled={!state.checkTwitch}>Далее</button>
          </div>
        </div>
      )}

      {state.step === 2 && (
        <div class="glass p-4 space-y-3" ref={el => reveal(el, () => ({}))}>
          <div class="text-lg">OpenAI токен</div>
          <input class="input" type="text" placeholder="OpenAI Organization Id (необязательно)" value={state.openAiOrganizationId} onInput={e => { setState('openAiOrganizationId', e.currentTarget.value); setState('checkOpenAI', null); saveDraft() }} />
          <input class="input" type="text" placeholder="OpenAI Project Id (необязательно)" value={state.openAiProjectId} onInput={e => { setState('openAiProjectId', e.currentTarget.value); setState('checkOpenAI', null); saveDraft() }} />
          <input class="input" type="password" placeholder="OpenAI API Key ••••••" value={state.openaiToken} onInput={e => { setState('openaiToken', e.currentTarget.value); setState('checkOpenAI', null); saveDraft() }} />
          <div class="flex items-center gap-2">
            <button disabled={state.openaiChecking} class={`btn btn-secondary ${state.openaiPulse ? '!bg-red-600/60 !border-red-400/60 ring-2 ring-red-400/60' : ''} ${state.openaiSuccess ? '!bg-green-600/60 !border-green-400/60 ring-2 ring-green-400/60' : ''}`} onClick={doCheckOpenAI}>
              {state.openaiChecking && (<span class="mr-2 h-4 w-4 rounded-full border-2 border-white/30 border-t-white animate-spin" />)}
              {state.openaiChecking ? 'Проверяем...' : 'Проверить'}
            </button>
            {state.checkOpenAI !== null && <span class={state.checkOpenAI ? 'text-green-400' : 'text-red-400'}>{state.checkOpenAI ? 'OK' : 'Ошибка'}</span>}
          </div>
          <div class="group relative mt-2 text-xs text-slate-200 space-y-2 bg-slate-800/70 rounded-lg p-3 border border-white/20">
            <div class="font-medium">Как получить OpenAI API ключ и идентификаторы</div>
            <ol class="list-decimal pl-4 space-y-1">
              <li>
                Перейдите на <a class="underline text-accent" href="https://platform.openai.com/" target="_blank" rel="noreferrer">OpenAI Platform</a> и во вкладке <b>Project</b> создайте свой проект.
              </li>
              <li>
                На вкладке <b>General</b> найдите и скопируйте <b>Organization ID</b>.
              </li>
              <li>
                В разделе <b>API Keys</b> создайте новый API‑ключ и вставьте его в поле выше.
              </li>
            </ol>
            {/* OpenAI tooltips */}
            <div class="pointer-events-none hidden md:block absolute -top-[385px] left-full ml-5 w-[360px] space-y-4 opacity-0 translate-y-1 transition-all duration-[1400ms] ease-[cubic-bezier(0.22,1,0.36,1)] group-hover:opacity-100 group-hover:translate-y-0">
              <div class="glass p-2">
                <div class="text-sm md:text-base mb-1 text-slate-200">Создайте свой проект вкладка "Project"</div>
                <img src={openaiHelp3} alt="OpenAI help 3" class="rounded-lg w-full h-auto max-h-[50vh] object-contain" />
              </div>
              <div class="glass p-2" style={{ 'transition-delay': '120ms' }}>
                <div class="text-sm md:text-base mb-1 text-slate-200">Код организации можете найти на основной вкладке "General"</div>
                <img src={openaiHelp4} alt="OpenAI help 4" class="rounded-lg w-full h-auto max-h-[50vh] object-contain" />
              </div>
              <div class="glass p-2" style={{ 'transition-delay': '240ms' }}>
                <div class="text-sm md:text-base mb-1 text-slate-200">Создайте АПИ ключ в разделе Api Keys</div>
                <img src={openaiHelp5} alt="OpenAI help 5" class="rounded-lg w-full h-auto max-h-[50vh] object-contain" />
              </div>
            </div>
          </div>
          <div class="flex gap-2">
            <button class="btn" onClick={prev}>Назад</button>
            <button class="btn btn-primary" onClick={next} disabled={!state.checkOpenAI}>Далее</button>
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
          {state.saveOk === true && (
            <div class="pt-2">
              <a class="btn" href="/dashboard">Перейти в дашборд</a>
            </div>
          )}
        </div>
      )}
    </div>
  )
}


