import { useNavigate, useLocation } from '@solidjs/router'
import { createStore } from 'solid-js/store'
import { clearTokens, getAccessToken } from './lib/auth'
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
import loadingIcon from './img/loading.png'


type Props = { children?: any }

export default function App(props: Props) {
  const navigate = useNavigate()
  const location = useLocation()
  const [ui, setUi] = createStore({ authVersion: 0, loggingOut: false })
  const isAuthed = () => (ui.authVersion, !!getAccessToken())

  if (typeof window !== 'undefined') {
    const handler = () => setUi('authVersion', ui.authVersion + 1)
    window.addEventListener('auth-changed', handler)
  }

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
			{isAuthed() && (
				<>
					<div class="mx-2 h-6 w-px bg-slate-700" />
					<div class="flex items-center gap-2">
						<button class="btn btn-secondary" onClick={() => navigate('/dashboard')}>Дашборд</button>
						<button class="btn btn-secondary" onClick={() => navigate('/integrations')}>Интеграции</button>
						<button class="btn btn-primary" onClick={() => navigate('/settings')}>Настройки</button>
						<button class="btn btn-secondary" onClick={async () => { setUi('loggingOut', true); await new Promise(res => requestAnimationFrame(() => setTimeout(res, 800))); clearTokens(); setUi('authVersion', ui.authVersion + 1); navigate('/login'); setUi('loggingOut', false) }}>Выйти</button>
					</div>
				</>
			)}
			{!isAuthed() && (
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
            ;(el as HTMLElement).offsetHeight
            requestAnimationFrame(() => el.classList.add('route-fade-show'))
          }}
          data-path={location.pathname}
        >
          {props.children}
        </div>
      </main>
      <Footer />
      <ToastHost />
      {ui.loggingOut && (
        <div class="fixed inset-0 z-50 bg-slate-900/40 backdrop-blur-sm flex items-center justify-center">
          <img src={loadingIcon} alt="loading" class="h-32 w-32 spin-slow" />
        </div>
      )}
    </div>
  )
}


