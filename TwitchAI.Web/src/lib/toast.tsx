import { createStore } from 'solid-js/store'
import { For } from 'solid-js/web'

type Toast = { id: number; type: 'success' | 'error' | 'info'; text: string }

const [store, setStore] = createStore<{ list: Toast[] }>({ list: [] })
let seq = 1

export function toastSuccess(text: string) { push('success', text) }
export function toastError(text: string) { push('error', text) }
export function toastInfo(text: string) { push('info', text) }

function push(type: Toast['type'], text: string) {
	const id = seq++
	setStore('list', l => [...l, { id, type, text }])
	setTimeout(() => remove(id), 3500)
}

function remove(id: number) {
	setStore('list', l => l.filter(t => t.id !== id))
}

export function ToastHost() {
	return (
		<div class="fixed bottom-4 right-4 z-50 space-y-2">
			<For each={store.list}>{t => (
				<div class={`px-4 py-2 rounded shadow-md backdrop-blur glass ${t.type === 'success' ? 'border border-green-400/40' : t.type === 'error' ? 'border border-red-400/40' : 'border border-slate-400/40'}`}>
					{t.text}
				</div>
			)}</For>
		</div>
	)
}


