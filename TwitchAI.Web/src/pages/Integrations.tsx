import { createStore } from 'solid-js/store'
import api from '../lib/api'
import { reveal } from '../utils/reveal'
import { jwtDecode } from 'jwt-decode'
import { getCurrentUserId } from '../lib/user'
import { toastError, toastInfo, toastSuccess } from '../lib/toast'

export default function Integrations() {
	const [state, setState] = createStore({
		userId: '' as string,
		twitchToken: '',
		twitchClientId: '',
		openaiToken: '',
		openAiOrganizationId: '',
		openAiProjectId: '',
		status: null as string | null,
		saving: false,
		checkingTwitch: false,
		checkingOpenAI: false,
		twitchOk: null as null | boolean,
		openaiOk: null as null | boolean,
		pulseTwitch: false,
		pulseOpenAI: false,
		successTwitch: false,
		successOpenAI: false,
		maskTwitch: true,
		maskOpenAI: true,
	})

	// init: взять userId из JWT и подгрузить существующие настройки (без секретов)
	;(() => {
    try {
      const uid0 = getCurrentUserId()
      if (uid0) setState('userId', uid0)
      const uid = uid0 || ((state as any).userId as string)
			if (uid) {
				api
					.get(`/v1.0/user-settings`, { params: { userId: uid } })
					.then(({ data }) => {
						const s = data?.result ?? data?.data ?? null
						if (s) {
							setState('twitchClientId', s.twitchClientId ?? '')
							setState('openAiOrganizationId', s.openAiOrganizationId ?? '')
							setState('openAiProjectId', s.openAiProjectId ?? '')
						}
					})
					.catch(() => {})
			}
		} catch {}
	})()

	const save = async (e: Event) => {
		e.preventDefault()
		setState('saving', true)
		setState('status', null)
		try {
			if (!state.userId) throw new Error('Нет userId')
			const body: any = {
				userId: state.userId,
				twitchClientId: state.twitchClientId || null,
				openAiOrganizationId: state.openAiOrganizationId || null,
				openAiProjectId: state.openAiProjectId || null,
			}
			if (state.twitchToken) body.twitchAccessToken = state.twitchToken
			if (state.openaiToken) body.openAiApiKey = state.openaiToken
      await api.post('/v1.0/user-settings', body)
      setState('status', 'Сохранено')
      toastSuccess('Интеграции сохранены')
		} catch (e: any) {
      const msg = e?.message ?? 'Ошибка'
      setState('status', msg)
      toastError(msg)
		} finally {
			setState('saving', false)
		}
	}

	const checkTwitch = async () => {
		setState('checkingTwitch', true)
    try {
      const token = (state.twitchToken || '').trim()
      const client = (state.twitchClientId || '').trim()
      const payload: any = { AccessToken: token, ClientId: client, accessToken: token, clientId: client }
      const { data } = await api.post('/v1.0/user-settings/check/twitch', payload, { headers: { 'Content-Type': 'application/json' }})
      const val = (data?.result ?? data?.data ?? data) as any
      const ok = val && ((typeof val === 'string' && val.toLowerCase() === 'ok') || (typeof val === 'object' && val?.ok === true))
		setState('twitchOk', !!ok)
		if (ok) { toastInfo('Twitch OK'); setState('successTwitch', true); setTimeout(() => setState('successTwitch', false), 1000) } else { toastError('Twitch ошибка'); setState('pulseTwitch', true); setTimeout(() => setState('pulseTwitch', false), 1000) }
      if (!ok) { setState('pulseTwitch', true); setTimeout(() => setState('pulseTwitch', false), 1000) }
    } catch (err: any) {
      setState('twitchOk', false)
      const title = err?.response?.data?.title as string | undefined
      const hasValidation = err?.response?.status === 400 && (!!err?.response?.data?.errors || title?.includes('validat'))
      const msg = hasValidation ? 'Ошибка валидации полей' : 'Twitch ошибка'
      toastError(msg)
      setState('pulseTwitch', true); setTimeout(() => setState('pulseTwitch', false), 1000)
		} finally {
      setState('checkingTwitch', false)
    }
	}

	const checkOpenAI = async () => {
		setState('checkingOpenAI', true)
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
		setState('openaiOk', !!ok)
		if (ok) { toastInfo('OpenAI OK'); setState('successOpenAI', true); setTimeout(() => setState('successOpenAI', false), 1000) } else { toastError('OpenAI ошибка'); setState('pulseOpenAI', true); setTimeout(() => setState('pulseOpenAI', false), 1000) }
      if (!ok) { setState('pulseOpenAI', true); setTimeout(() => setState('pulseOpenAI', false), 1000) }
    } catch (err: any) {
      setState('openaiOk', false)
      const title = err?.response?.data?.title as string | undefined
      const hasValidation = err?.response?.status === 400 && (!!err?.response?.data?.errors || title?.includes('validat'))
      const msg = hasValidation ? 'Ошибка валидации полей' : (err?.response?.data?.errors?.ApiKey?.[0] || err?.message || 'OpenAI ошибка')
      toastError(msg)
      setState('pulseOpenAI', true); setTimeout(() => setState('pulseOpenAI', false), 1000)
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
						<input class="input" type="text" placeholder="Twitch Client ID" value={state.twitchClientId} onInput={e => setState('twitchClientId', e.currentTarget.value)} />
						<div class="flex gap-2">
							<input class="input flex-1" type={state.maskTwitch ? 'password' : 'text'} placeholder="••••••" value={state.twitchToken} onInput={e => setState('twitchToken', e.currentTarget.value)} />
							<button type="button" class="btn" onClick={() => setState('maskTwitch', !state.maskTwitch)}>{state.maskTwitch ? 'Показать' : 'Скрыть'}</button>
						</div>
						<div class="flex items-center gap-2">
						<button type="button" class={`btn btn-secondary ${state.pulseTwitch ? '!bg-red-600/60 !border-red-400/60 ring-2 ring-red-400/60' : ''} ${state.successTwitch ? '!bg-green-600/60 !border-green-400/60 ring-2 ring-green-400/60' : ''}`} onClick={checkTwitch} disabled={state.checkingTwitch || !state.twitchClientId || !state.twitchToken}>
							{state.checkingTwitch && (<span class="mr-2 h-4 w-4 rounded-full border-2 border-white/30 border-t-white animate-spin" />)}
							{state.checkingTwitch ? 'Проверяем...' : 'Проверить'}
						</button>
							{state.twitchOk !== null && (
								<span class={state.twitchOk ? 'text-green-400' : 'text-red-400'}>{state.twitchOk ? 'OK' : 'Ошибка'}</span>
							)}
						</div>
					</div>
					<div class="space-y-2 card-secondary p-4" ref={el => reveal(el, () => ({ delayMs: 120 }))}>
						<label class="block text-slate-300">OpenAI Token</label>
						<input class="input" type="text" placeholder="OpenAI Organization Id (необязательно)" value={state.openAiOrganizationId} onInput={e => setState('openAiOrganizationId', e.currentTarget.value)} />
						<input class="input" type="text" placeholder="OpenAI Project Id (необязательно)" value={state.openAiProjectId} onInput={e => setState('openAiProjectId', e.currentTarget.value)} />
						<div class="flex gap-2">
							<input class="input flex-1" type={state.maskOpenAI ? 'password' : 'text'} placeholder="••••••" value={state.openaiToken} onInput={e => setState('openaiToken', e.currentTarget.value)} />
							<button type="button" class="btn" onClick={() => setState('maskOpenAI', !state.maskOpenAI)}>{state.maskOpenAI ? 'Показать' : 'Скрыть'}</button>
						</div>
						<div class="flex items-center gap-2">
						<button type="button" class={`btn btn-secondary ${state.pulseOpenAI ? '!bg-red-600/60 !border-red-400/60 ring-2 ring-red-400/60' : ''} ${state.successOpenAI ? '!bg-green-600/60 !border-green-400/60 ring-2 ring-green-400/60' : ''}`} onClick={checkOpenAI} disabled={state.checkingOpenAI || !state.openaiToken}>
							{state.checkingOpenAI && (<span class="mr-2 h-4 w-4 rounded-full border-2 border-white/30 border-t-white animate-spin" />)}
							{state.checkingOpenAI ? 'Проверяем...' : 'Проверить'}
						</button>
							{state.openaiOk !== null && (
								<span class={state.openaiOk ? 'text-green-400' : 'text-red-400'}>{state.openaiOk ? 'OK' : 'Ошибка'}</span>
							)}
						</div>
					</div>
				</div>
				{state.status && <div class="text-sm text-slate-400">{state.status}</div>}
				<button class="btn btn-primary" disabled={state.saving || !state.userId}>{state.saving ? '...' : 'Сохранить'}</button>
			</form>
		</div>
	)
}


