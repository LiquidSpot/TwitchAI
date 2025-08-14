import { useNavigate } from '@solidjs/router'
import Login from './pages/Login'
import Settings from './pages/Settings'
import Home from './pages/Home'
import Features from './pages/Features'
import Pricing from './pages/Pricing'
import Docs from './pages/Docs'
import Footer from './components/Footer'

type Props = { children?: any }

export default function App(props: Props) {
  const navigate = useNavigate()
  const token = () => localStorage.getItem('access_token')

  return (
    <div class="min-h-screen text-white">
      <nav class="fixed top-0 left-0 right-0 z-10 bg-slate-800/90 backdrop-blur">
        <div class="max-w-6xl mx-auto p-4 flex items-center justify-between">
          <div class="text-2xl font-semibold">TwitchAI</div>
          <div class="space-x-3">
            <button class="btn" onClick={() => navigate('/')}>Главная</button>
            <button class="btn" onClick={() => navigate('/features')}>Функции</button>
            <button class="btn" onClick={() => navigate('/pricing')}>Цены</button>
            <button class="btn" onClick={() => navigate('/docs')}>Документация</button>
            <button class="btn bg-primary hover:bg-primary/90" onClick={() => navigate('/settings')}>Настройки</button>
            {token() ? (
              <button class="btn" onClick={() => { localStorage.removeItem('access_token'); navigate('/login') }}>Выйти</button>
            ) : (
              <button class="btn" onClick={() => navigate('/login')}>Войти</button>
            )}
          </div>
        </div>
      </nav>

      <main class="pt-24">
        {props.children}
      </main>
      <Footer />
    </div>
  )
}


