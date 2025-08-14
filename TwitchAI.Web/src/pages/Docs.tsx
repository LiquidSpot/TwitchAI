export default function Docs() {
  return (
    <div class="max-w-6xl mx-auto px-4 space-y-6">
      <h1 class="text-3xl">Документация</h1>
      <div class="glass p-6 space-y-3 text-slate-300">
        <p>
          Быстрый старт: настройте OpenAI и Twitch, затем войдите в админку и заполните интеграции. 
          Проверьте токены кнопками проверки и сохраните настройки.
        </p>
        <ol class="list-decimal pl-5">
          <li>Заполните appsettings или через UI добавьте OpenAI/Twitch ключи</li>
          <li>Проверьте соединения</li>
          <li>Включите нужные модули бота</li>
          <li>Начните чат с ИИ и команды: !ai, !engine, !reply-limit, !viewers</li>
        </ol>
        <p>Подробности см. в README репозитория.</p>
      </div>
    </div>
  )
}


