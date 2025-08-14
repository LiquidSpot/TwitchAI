import { render } from 'solid-js/web'
import { Router, Route } from '@solidjs/router'
import App from './App'
import Home from './pages/Home'
import Features from './pages/Features'
import Pricing from './pages/Pricing'
import Docs from './pages/Docs'
import Login from './pages/Login'
import Settings from './pages/Settings'
import Register from './pages/Register'
import ResetPassword from './pages/ResetPassword'
import './index.css'

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
      <Route path="/settings" component={Settings} />

      <Route path="*" component={Home} />
    </Route>
  </Router>
), document.getElementById('root') as HTMLElement)