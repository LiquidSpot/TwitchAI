import { useNavigate, useLocation } from '@solidjs/router'
import { BRAND_ICON, BRAND_NAME } from './utils/brand'
import Login from './pages/Login'
import Settings from './pages/Settings'
import Home from './pages/Home'
import Features from './pages/Features'
import Pricing from './pages/Pricing'
import Docs from './pages/Docs'
import Footer from './components/Footer'
import Register from './pages/Register'
import ResetPassword from './pages/ResetPassword'
import { ToastHost } from './lib/toast'

type Props = { children?: any }

export default function App(props: Props) {
  const navigate = useNavigate()
  const token = () => localStorage.getItem('access_token')
  const location = useLocation()

  return (
    <div class="min-h-screen text-white">
      <nav class="fixed top-0 left-0 right-0 z-10 bg-slate-800/90 backdrop-blur">
        <div class="max-w-6xl mx-auto p-4 flex items-center justify-between">
          <div class="flex items-center gap-2 text-2xl font-semibold">
            <img src={BRAND_ICON} alt={BRAND_NAME} class="h-7 w-7 rounded" loading="eager" decoding="async" />
            <span>{BRAND_NAME}</span>
          </div>
		  <div class="flex items-center gap-2">
			{/* Публичные */}
			<div class="flex items-center gap-2">
			  <button class="btn btn-secondary" onClick={() => navigate('/')}>Главная</button>
			  <button class="btn btn-secondary" onClick={() => navigate('/features')}>Функции</button>
			  <button class="btn btn-secondary" onClick={() => navigate('/pricing')}>Цены</button>
			  <button class="btn btn-secondary" onClick={() => navigate('/docs')}>Документация</button>
			</div>

			{/* Приватные (видны только после входа) */}
			{token() && (
				<>
					<div class="mx-2 h-6 w-px bg-slate-700" />
					<div class="flex items-center gap-2">
						<button class="btn btn-secondary" onClick={() => navigate('/dashboard')}>Дашборд</button>
						<button class="btn btn-secondary" onClick={() => navigate('/integrations')}>Интеграции</button>
						<button class="btn btn-primary" onClick={() => navigate('/settings')}>Настройки</button>
						<button class="btn btn-secondary" onClick={() => { localStorage.removeItem('access_token'); navigate('/login') }}>Выйти</button>
					</div>
				</>
			)}
			{!token() && (
				<>
					<div class="mx-2 h-6 w-px bg-slate-700" />
					<button class="btn btn-primary" onClick={() => navigate('/login')}>Войти</button>
				</>
			)}
		  </div>
        </div>
      </nav>

      <main class="pt-24">
        <div
          class="route-fade"
          ref={el => {
            // force reflow to restart transition
            // eslint-disable-next-line @typescript-eslint/no-unused-expressions
            (el as HTMLElement).offsetHeight
            requestAnimationFrame(() => el.classList.add('route-fade-show'))
          }}
          data-path={location.pathname}
        >
          {props.children}
        </div>
      </main>
      <Footer />
      <ToastHost />
    </div>
  )
}


