import { useNavigate } from '@solidjs/router'

export default function NotFound() {
	const navigate = useNavigate()
	return (
		<div class="max-w-3xl mx-auto p-6 glass text-center space-y-3">
			<div class="text-3xl">404</div>
			<div class="text-slate-300">Страница не найдена</div>
			<div>
				<button class="btn btn-primary" onClick={() => navigate('/')}>На главную</button>
			</div>
		</div>
	)
}


