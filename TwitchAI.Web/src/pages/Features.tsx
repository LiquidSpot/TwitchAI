 
import { reveal } from '../utils/reveal'

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
      <h1 class="text-3xl" ref={el => reveal(el, () => ({}))}>Возможности</h1>

      <div class="grid md:grid-cols-3 gap-4">
        {toc.map((item, idx) => (
          <a href={`#${item.id}`} class="block">
            <div class="card-secondary p-5 glow-hover cursor-pointer" ref={el => reveal(el, () => ({ delayMs: idx * 120 }))}>
              <div class="text-xl mb-1">{item.title}</div>
              <div class="text-slate-300">{item.desc}</div>
            </div>
          </a>
        ))}
      </div>

      <section id="ai" class="card-secondary p-6 scroll-mt-28">
        <h2 class="text-2xl mb-3" ref={el => reveal(el, () => ({}))}>ИИ-диалоги: что это представляет из себя</h2>
        <ul class="list-disc pl-5 space-y-1 text-slate-300">
          {[
            'Контекстные ответы с историей переписки и ролями бота.',
            'Поддержка разных ролей (например: дружелюбный, токсичный, neko, и др.).',
            'Гибкие лимиты на ответы: частота, длина, приоритет по пользователям.',
            'Смена движков OpenAI через команды или панель настроек.'
          ].map((t, i) => (
            <li ref={el => reveal(el, () => ({ delayMs: 100 + i * 120 }))}>{t}</li>
          ))}
        </ul>
      </section>

      <section id="integrations" class="card-secondary p-6 scroll-mt-28">
        <h2 class="text-2xl mb-3" ref={el => reveal(el, () => ({}))}>Интеграции: какие и как</h2>
        <ul class="list-disc pl-5 space-y-1 text-slate-300">
          {[
            'Twitch: подключение канала, чтение чата, идентификация зрителей.',
            'OpenAI: ключ API, выбор модели, проверка токена в один клик.',
            'Звуковые алерты: запуск по командам и событиям с кулдаунами.',
            'Простая проверка интеграций и статуса прямо из настроек.'
          ].map((t, i) => (
            <li ref={el => reveal(el, () => ({ delayMs: 100 + i * 120 }))}>{t}</li>
          ))}
        </ul>
      </section>

      <section id="content" class="card-secondary p-6 scroll-mt-28">
        <h2 class="text-2xl mb-3" ref={el => reveal(el, () => ({}))}>Контент: что за контент</h2>
        <ul class="list-disc pl-5 space-y-1 text-slate-300">
          {[
            'Переводы сообщений и ответов на популярные языки.',
            'Факты дня и праздники с локализацией.',
            'Звуковые эффекты и реакции на команды чата.',
            'Автогенерация кратких ответов и подсказок для вовлечения.'
          ].map((t, i) => (
            <li ref={el => reveal(el, () => ({ delayMs: 100 + i * 120 }))}>{t}</li>
          ))}
        </ul>
      </section>

      <section id="moderation" class="card-secondary p-6 scroll-mt-28">
        <h2 class="text-2xl mb-3" ref={el => reveal(el, () => ({}))}>Модерация: как это помогает</h2>
        <ul class="list-disc pl-5 space-y-1 text-slate-300">
          {[
            'Базовые фильтры, автоответы и подсветка запрещённого.',
            'Гибкая настройка правил и исключений.',
            'Снижение шума и токсичности без ручной модерации.'
          ].map((t, i) => (
            <li ref={el => reveal(el, () => ({ delayMs: 100 + i * 120 }))}>{t}</li>
          ))}
        </ul>
      </section>

      <section id="analytics" class="card-secondary p-6 scroll-mt-28">
        <h2 class="text-2xl mb-3" ref={el => reveal(el, () => ({}))}>Статистика: что показываем</h2>
        <ul class="list-disc pl-5 space-y-1 text-slate-300">
          {[
            'Активные, молчаливые и общие зрители по периодам.',
            'Тренды вовлеченности и активности чата.',
            'Сводки по командам, ответам и событиям.'
          ].map((t, i) => (
            <li ref={el => reveal(el, () => ({ delayMs: 100 + i * 120 }))}>{t}</li>
          ))}
        </ul>
      </section>

      <section id="settings" class="card-secondary p-6 scroll-mt-28">
        <h2 class="text-2xl mb-3" ref={el => reveal(el, () => ({}))}>Гибкие настройки: как персонализировать</h2>
        <ul class="list-disc pl-5 space-y-1 text-slate-300">
          {[
            'Порог частоты ответов, длина, форматирование.',
            'Лимиты по пользователям и ролям.',
            'Выбор моделей, автопереводы, реакции и звуки.'
          ].map((t, i) => (
            <li ref={el => reveal(el, () => ({ delayMs: 100 + i * 120 }))}>{t}</li>
          ))}
        </ul>
      </section>
    </div>
  )
}


