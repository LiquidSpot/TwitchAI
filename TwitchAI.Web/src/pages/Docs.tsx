import { reveal } from '../utils/reveal'

export default function Docs() {
  return (
    <div class="max-w-6xl mx-auto px-4 space-y-6">
      <h1 class="text-3xl" ref={el => reveal(el, () => ({}))}>Документация</h1>
      <div class="glass p-6 space-y-3 text-slate-300" ref={el => reveal(el, () => ({ delayMs: 60 }))}>
        <p ref={el => reveal(el, () => ({ delayMs: 120 }))}>
          Быстрый старт: настройте OpenAI и Twitch, затем войдите в админку и заполните интеграции. 
          Проверьте токены кнопками проверки и сохраните настройки.
        </p>
        <ol class="list-decimal pl-5">
          {[
            'Заполните appsettings или через UI добавьте OpenAI/Twitch ключи',
            'Проверьте соединения',
            'Включите нужные модули бота',
            'Начните чат с ИИ и команды: !ai, !engine, !reply-limit, !viewers'
          ].map((step, i) => (
            <li ref={el => reveal(el, () => ({ delayMs: 160 + i * 120 }))}>{step}</li>
          ))}
        </ol>
        <p ref={el => reveal(el, () => ({ delayMs: 200 }))}>Подробности см. в README репозитория.</p>
      </div>
    </div>
  )
}


