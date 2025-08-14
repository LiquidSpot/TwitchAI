 

export default function Features() {
  const toc = [
    { id: 'ai', title: 'ИИ-диалоги', desc: 'Что это представляет из себя' },
    { id: 'integrations', title: 'Интеграции', desc: 'Какие интеграции и как' },
    { id: 'content', title: 'Контент', desc: 'Что за контент' },
    { id: 'moderation', title: 'Модерация', desc: 'Как это помогает' },
    { id: 'analytics', title: 'Статистика', desc: 'Что показываем' },
    { id: 'settings', title: 'Гибкие настройки', desc: 'Как персонализировать' },
  ]

  return (
    <div class="max-w-6xl mx-auto px-4 space-y-10">
      <h1 class="text-3xl">Возможности</h1>

      <div class="grid md:grid-cols-3 gap-4">
        {toc.map(item => (
          <a href={`#${item.id}`} class="block">
            <div class="card-secondary p-5 glow-hover cursor-pointer">
              <div class="text-xl mb-1">{item.title}</div>
              <div class="text-slate-300">{item.desc}</div>
            </div>
          </a>
        ))}
      </div>

      <section id="ai" class="card-secondary p-6 scroll-mt-28">
        <h2 class="text-2xl mb-3">ИИ-диалоги: что это представляет из себя</h2>
        <ul class="list-disc pl-5 space-y-1 text-slate-300">
          <li>Контекстные ответы с историей переписки и ролями бота.</li>
          <li>Поддержка разных ролей (например: дружелюбный, токсичный, neko, и др.).</li>
          <li>Гибкие лимиты на ответы: частота, длина, приоритет по пользователям.</li>
          <li>Смена движков OpenAI через команды или панель настроек.</li>
        </ul>
      </section>

      <section id="integrations" class="card-secondary p-6 scroll-mt-28">
        <h2 class="text-2xl mb-3">Интеграции: какие и как</h2>
        <ul class="list-disc pl-5 space-y-1 text-slate-300">
          <li>Twitch: подключение канала, чтение чата, идентификация зрителей.</li>
          <li>OpenAI: ключ API, выбор модели, проверка токена в один клик.</li>
          <li>Звуковые алерты: запуск по командам и событиям с кулдаунами.</li>
          <li>Простая проверка интеграций и статуса прямо из настроек.</li>
        </ul>
      </section>

      <section id="content" class="card-secondary p-6 scroll-mt-28">
        <h2 class="text-2xl mb-3">Контент: что за контент</h2>
        <ul class="list-disc pl-5 space-y-1 text-slate-300">
          <li>Переводы сообщений и ответов на популярные языки.</li>
          <li>Факты дня и праздники с локализацией.</li>
          <li>Звуковые эффекты и реакции на команды чата.</li>
          <li>Автогенерация кратких ответов и подсказок для вовлечения.</li>
        </ul>
      </section>

      <section id="moderation" class="card-secondary p-6 scroll-mt-28">
        <h2 class="text-2xl mb-3">Модерация: как это помогает</h2>
        <ul class="list-disc pl-5 space-y-1 text-slate-300">
          <li>Базовые фильтры, автоответы и подсветка запрещённого.</li>
          <li>Гибкая настройка правил и исключений.</li>
          <li>Снижение шума и токсичности без ручной модерации.</li>
        </ul>
      </section>

      <section id="analytics" class="card-secondary p-6 scroll-mt-28">
        <h2 class="text-2xl mb-3">Статистика: что показываем</h2>
        <ul class="list-disc pl-5 space-y-1 text-slate-300">
          <li>Активные, молчаливые и общие зрители по периодам.</li>
          <li>Тренды вовлеченности и активности чата.</li>
          <li>Сводки по командам, ответам и событиям.</li>
        </ul>
      </section>

      <section id="settings" class="card-secondary p-6 scroll-mt-28">
        <h2 class="text-2xl mb-3">Гибкие настройки: как персонализировать</h2>
        <ul class="list-disc pl-5 space-y-1 text-slate-300">
          <li>Порог частоты ответов, длина, форматирование.</li>
          <li>Лимиты по пользователям и ролям.</li>
          <li>Выбор моделей, автопереводы, реакции и звуки.</li>
        </ul>
      </section>
    </div>
  )
}


