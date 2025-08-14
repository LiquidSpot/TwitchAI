import { render } from 'solid-js/web'
import { Router, Route } from '@solidjs/router'
// lazy/Suspense недоступны в текущей типовой конфигурации, используем прямые импорты
import App from './App'
import Home from './pages/Home'
import Features from './pages/Features'
import Pricing from './pages/Pricing'
import Docs from './pages/Docs'
import Login from './pages/Login'
import Settings from './pages/Settings'
import Register from './pages/Register'
import ResetPassword from './pages/ResetPassword'
import Protected from './components/Protected'
import Dashboard from './pages/Dashboard'
import Integrations from './pages/Integrations'
import Onboarding from './pages/Onboarding'
import NotFound from './pages/NotFound'
import './index.css'
import ErrorBoundary from './components/ErrorBoundary'

render(() => (
  <Router>
    <Route path="/" component={App}>
      {/* Публичные страницы */}
      <Route path="/" component={Home} />
      <Route path="/features" component={Features} />
      <Route path="/pricing" component={Pricing} />
      <Route path="/docs" component={Docs} />

      {/* Личный кабинет (доступ через кнопку Войти) */}
      <Route path="/login" component={Login} />
      <Route path="/register" component={Register} />
      <Route path="/reset-password" component={ResetPassword} />
      <Route path="/dashboard" component={() => (
        <Protected>
          <ErrorBoundary>
            <Dashboard />
          </ErrorBoundary>
        </Protected>
      )} />
      <Route path="/integrations" component={() => (
        <Protected>
          <ErrorBoundary>
            <Integrations />
          </ErrorBoundary>
        </Protected>
      )} />
      <Route path="/onboarding" component={() => (
        <Protected>
          <ErrorBoundary>
            <Onboarding />
          </ErrorBoundary>
        </Protected>
      )} />
      <Route path="/settings" component={() => (
        <Protected>
          <ErrorBoundary>
            <Settings />
          </ErrorBoundary>
        </Protected>
      )} />

      <Route path="*" component={NotFound} />
    </Route>
  </Router>
), document.getElementById('root') as HTMLElement)