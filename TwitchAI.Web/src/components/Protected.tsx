import { Show } from 'solid-js'
import { useNavigate } from '@solidjs/router'
import { isAuthenticated } from '../lib/auth'

type Props = { children: any }

export default function Protected(props: Props) {
  const navigate = useNavigate()
  const authed = isAuthenticated()
  if (!authed) navigate('/login', { replace: true })
  return <Show when={authed}>{props.children}</Show>
}


