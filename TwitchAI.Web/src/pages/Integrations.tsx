import { createStore } from 'solid-js/store'
import api from '../lib/api'
import { reveal } from '../utils/reveal'

export default function Integrations() {
  const [state, setState] = createStore({
    twitchToken: '',
    openaiToken: '',
    status: null as string | null,
    saving: false,
    checkingTwitch: false,
    checkingOpenAI: false,
    twitchOk: null as null | boolean,
    openaiOk: null as null | boolean,
    maskTwitch: true,
    maskOpenAI: true,
  })

  const save = async (e: Event) => {
    e.preventDefault()
    setState('saving', true)
    setState('status', null)
    try {
      await api.post('/v1.0/user-settings', { twitchToken: state.twitchToken, openaiToken: state.openaiToken })
      setState('status', 'Сохранено')
    } catch (e: any) {
      setState('status', e?.message ?? 'Ошибка')
    } finally {
      setState('saving', false)
    }
  }

  const checkTwitch = async () => {
    setState('checkingTwitch', true)
    try {
      const { data } = await api.post('/v1.0/user-settings/check/twitch', { token: state.twitchToken })
      const ok = !!(data?.result ?? data?.data ?? data?.ok)
      setState('twitchOk', ok)
    } catch {
      setState('twitchOk', false)
    } finally {
      setState('checkingTwitch', false)
    }
  }

  const checkOpenAI = async () => {
    setState('checkingOpenAI', true)
    try {
      const { data } = await api.post('/v1.0/user-settings/check/openai', { token: state.openaiToken })
      const ok = !!(data?.result ?? data?.data ?? data?.ok)
      setState('openaiOk', ok)
    } catch {
      setState('openaiOk', false)
    } finally {
      setState('checkingOpenAI', false)
    }
  }

  return (
    <div class="max-w-3xl mx-auto p-6 glass space-y-6">
      <h1 class="text-2xl" ref={el => reveal(el, () => ({}))}>Интеграции</h1>
      <form class="space-y-4" onSubmit={save}>
        <div class="grid md:grid-cols-2 gap-4">
          <div class="space-y-2 card-secondary p-4" ref={el => reveal(el, () => ({}))}>
            <label class="block text-slate-300">Twitch Token</label>
            <div class="flex gap-2">
              <input class="input flex-1" type={state.maskTwitch ? 'password' : 'text'} placeholder="••••••" value={state.twitchToken} onInput={e => setState('twitchToken', e.currentTarget.value)} />
              <button type="button" class="btn" onClick={() => setState('maskTwitch', !state.maskTwitch)}>{state.maskTwitch ? 'Показать' : 'Скрыть'}</button>
            </div>
            <div class="flex items-center gap-2">
              <button type="button" class="btn btn-secondary" onClick={checkTwitch} disabled={state.checkingTwitch}>{state.checkingTwitch ? '...' : 'Проверить'}</button>
              {state.twitchOk !== null && (
                <span class={state.twitchOk ? 'text-green-400' : 'text-red-400'}>{state.twitchOk ? 'OK' : 'Ошибка'}</span>
              )}
            </div>
          </div>
          <div class="space-y-2 card-secondary p-4" ref={el => reveal(el, () => ({ delayMs: 120 }))}>
            <label class="block text-slate-300">OpenAI Token</label>
            <div class="flex gap-2">
              <input class="input flex-1" type={state.maskOpenAI ? 'password' : 'text'} placeholder="••••••" value={state.openaiToken} onInput={e => setState('openaiToken', e.currentTarget.value)} />
              <button type="button" class="btn" onClick={() => setState('maskOpenAI', !state.maskOpenAI)}>{state.maskOpenAI ? 'Показать' : 'Скрыть'}</button>
            </div>
            <div class="flex items-center gap-2">
              <button type="button" class="btn btn-secondary" onClick={checkOpenAI} disabled={state.checkingOpenAI}>{state.checkingOpenAI ? '...' : 'Проверить'}</button>
              {state.openaiOk !== null && (
                <span class={state.openaiOk ? 'text-green-400' : 'text-red-400'}>{state.openaiOk ? 'OK' : 'Ошибка'}</span>
              )}
            </div>
          </div>
        </div>
        {state.status && <div class="text-sm text-slate-400">{state.status}</div>}
        <button class="btn btn-primary" disabled={state.saving}>{state.saving ? '...' : 'Сохранить'}</button>
      </form>
    </div>
  )
}


