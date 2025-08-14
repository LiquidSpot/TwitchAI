import { ErrorBoundary as SolidErrorBoundary } from 'solid-js'

export default function ErrorBoundary(props: { children: any }) {
	return (
		<SolidErrorBoundary fallback={(err) => (
			<div class="max-w-3xl mx-auto p-6 glass">
				<div class="text-xl mb-2">Что-то пошло не так</div>
				<pre class="text-sm text-red-300 whitespace-pre-wrap">{String(err)}</pre>
			</div>
		)}>
			{props.children}
		</SolidErrorBoundary>
	)
}


