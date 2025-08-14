import { createStore } from 'solid-js/store'
import { For } from 'solid-js/web'

type ToastPosition = 'top-right' | 'bottom-right'
type ToastType = 'success' | 'error' | 'info'
type Toast = {
	id: number
	type: ToastType
	position: ToastPosition
	title?: string
	text?: string
	imageUrl?: string
	content?: any
	durationMs?: number
	closing?: boolean
}
type ToastOptions = { position?: ToastPosition; durationMs?: number }
type ToastInput = string | { title?: string; text?: string; imageUrl?: string; content?: any; position?: ToastPosition; durationMs?: number }

const [store, setStore] = createStore<{ list: Toast[] }>({ list: [] })
let seq = 1

export function toastSuccess(input: ToastInput, opts?: ToastOptions) { push('success', input, opts) }
export function toastError(input: ToastInput, opts?: ToastOptions) { push('error', input, opts) }
export function toastInfo(input: ToastInput, opts?: ToastOptions) { push('info', input, opts) }

function push(type: ToastType, input: ToastInput, opts?: ToastOptions) {
    const id = seq++
    const position: ToastPosition = (typeof input === 'object' && input?.position) || opts?.position || 'bottom-right'
    const durationMs = (typeof input === 'object' && input?.durationMs) || opts?.durationMs || 3500
    const title = typeof input === 'string' ? undefined : input?.title
    const text = typeof input === 'string' ? input : input?.text
    const imageUrl = typeof input === 'string' ? undefined : input?.imageUrl
    const content = typeof input === 'string' ? undefined : input?.content
    setStore('list', l => {
        const next = [...l, { id, type, position, title, text, imageUrl, content, durationMs }]
        return next.slice(-3) // максимум 3 уведомления одновременно
    })
    setTimeout(() => beginClose(id), durationMs)
}

function remove(id: number) {
	setStore('list', l => l.filter(t => t.id !== id))
}

function beginClose(id: number) {
	// переведём тост в состояние закрытия, затем удалим после завершения transition
	setStore('list', l => l.map(t => (t.id === id ? { ...t, closing: true } : t)))
	setTimeout(() => remove(id), 1200) // должно совпадать с длительностью CSS анимации
}

export function ToastHost() {
	return (
		<>
            <div class="fixed top-4 right-4 z-50 space-y-2">
                <For each={store.list.filter(t => t.position === 'top-right')}>{renderToast}</For>
            </div>
            <div class="fixed bottom-4 right-4 z-50 space-y-2">
                <For each={store.list.filter(t => t.position === 'bottom-right')}>{renderToast}</For>
            </div>
		</>
	)
}

function renderToast(t: Toast) {
    const border = t.type === 'success' ? 'border border-green-400/40' : t.type === 'error' ? 'border border-red-400/40' : 'border border-slate-400/40'
    const badge = t.type === 'success' ? 'bg-green-500/90' : t.type === 'error' ? 'bg-red-500/90' : 'bg-slate-500/90'
    return (
		<div class={`px-4 py-3 rounded shadow-md backdrop-blur glass ${border} w-80 max-w-[90vw] toast-reveal ${t.closing ? 'toast-reveal-hide' : ''} toast-type-${t.type}`} ref={el => requestAnimationFrame(() => el.classList.add('toast-reveal-show'))}>
            <div class="flex items-start gap-3">
                {t.imageUrl ? (
                    <img src={t.imageUrl} alt={t.title ?? ''} class="h-10 w-10 rounded object-cover" />
                ) : t.content ? (
                    <div>{t.content}</div>
                ) : (
                    <div class={`h-10 w-10 rounded flex items-center justify-center text-white ${badge}`}>{(t.title ?? t.type).slice(0,1).toUpperCase()}</div>
                )}
                <div class="min-w-0">
                    {t.title && <div class="font-medium leading-5 truncate">{t.title}</div>}
                    {t.text && <div class="text-sm text-slate-200 leading-5 break-words">{t.text}</div>}
                </div>
            </div>
        </div>
    )
}


