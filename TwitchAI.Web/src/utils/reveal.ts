import { onCleanup } from 'solid-js'

export type RevealOptions = {
  threshold?: number
  rootMargin?: string
  once?: boolean
  delayMs?: number
}

// Solid directive: use:reveal
export function reveal(el: HTMLElement, accessor: () => RevealOptions | undefined) {
  const options = accessor() || {}
  const threshold = options.threshold ?? 0.15
  const rootMargin = options.rootMargin ?? '0px 0px -10% 0px'
  const once = options.once ?? true
  const delayMs = options.delayMs ?? 0

  const observer = new IntersectionObserver(
    entries => {
      entries.forEach(entry => {
        if (entry.isIntersecting) {
          const show = () => el.classList.add('reveal-show')
          if (delayMs > 0) setTimeout(show, delayMs)
          else show()
          if (once) observer.unobserve(el)
        }
      })
    },
    { threshold, rootMargin }
  )

  // initial state
  el.classList.add('reveal')
  observer.observe(el)

  onCleanup(() => observer.disconnect())
}


