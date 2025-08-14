import { useNavigate } from '@solidjs/router'
import { reveal } from '../utils/reveal'

export default function Home() {
  const navigate = useNavigate()
  return (
    <div class="max-w-6xl mx-auto px-4 space-y-24">
      <section class="glass p-10 text-center" ref={el => reveal(el, () => ({ delayMs: 50 }))}>
        <div class="flex items-center justify-center gap-3 mb-4">
          <h1 class="text-3xl md:text-5xl font-semibold">TwitchAI — умный бот для вашего Twitch канала</h1>
        </div>
        <p class="text-slate-300 max-w-3xl mx-auto">
          Чат с ИИ, переводы, звуковые алерты, праздники и аналитика — всё в одном решении. Простая настройка, современный дизайн, высокая надёжность.
        </p>
        <div class="mt-6 flex justify-center gap-3">
          <button class="btn btn-primary" onClick={() => navigate('/login')}>Попробовать бесплатно</button>
          <button class="btn btn-secondary" onClick={() => navigate('/docs')}>Документация</button>
        </div>
      </section>

      <section class="grid md:grid-cols-3 gap-4">
        {[
          { id: 'ai', title: 'ИИ-диалоги', desc: 'Контекстные ответы, роли бота, гибкие лимиты, смена движков OpenAI.' },
          { id: 'integrations', title: 'Интеграции', desc: 'Подключение Twitch и OpenAI, проверка токенов в один клик.' },
          { id: 'content', title: 'Контент', desc: 'Переводы, звуки, факты и праздники — вовлекайте аудиторию.' },
          { id: 'moderation', title: 'Модерация', desc: 'Простые правила, автоответы и фильтры для чата.' },
          { id: 'analytics', title: 'Статистика', desc: 'Активные, молчаливые и общие зрители в динамике.' },
          { id: 'settings', title: 'Гибкие настройки', desc: 'Порог частоты ответов, ограничения и персонализация.' },
        ].map((card, idx) => (
          <a href={`/features#${card.id}`} class="block">
            <div class="card-secondary p-6 glow-hover cursor-pointer" ref={el => reveal(el, () => ({ delayMs: idx * 100 }))}>
              <div class="text-xl font-medium mb-2">{card.title}</div>
              <div class="text-slate-300">{card.desc}</div>
            </div>
          </a>
        ))}
      </section>

      <section class="glass p-8" ref={el => reveal(el, () => ({ delayMs: 120 }))}>
        <h2 class="text-2xl mb-3">Почему TwitchAI</h2>
        <ul class="list-disc pl-5 space-y-1 text-slate-300">
          <li>Чистая архитектура .NET 8 + надёжная инфраструктура</li>
          <li>Простая установка и понятные настройки</li>
          <li>Гибкость — от развлекухи до аналитики</li>
        </ul>
      </section>

      <section class="glass p-8" ref={el => reveal(el, () => ({}))}>
        <h2 class="text-2xl mb-4">Как это работает</h2>
        <ol class="grid md:grid-cols-3 gap-4">
          {[
            'Регистрация в веб-интерфейсе и вход',
            'Подключение Twitch и OpenAI токенов, проверка в 1 клик',
            'Включение нужных функций: чат, переводы, алерты, аналитика',
          ].map((step, i) => (
            <li class="card-secondary p-6 glow-hover" ref={el => reveal(el, () => ({ delayMs: 200 + i * 200 }))}>
              <div class="text-accent mb-1">Шаг {i + 1}</div>
              <div class="text-slate-200">{step}</div>
            </li>
          ))}
        </ol>
      </section>

      <section class="glass p-8" ref={el => reveal(el, () => ({ delayMs: 120 }))}>
        <h2 class="text-2xl mb-4">Ответы на вопросы</h2>
        <div class="space-y-3 text-slate-300">
          <div>
            <div class="font-medium text-slate-200">Нужна ли установка на сервер?</div>
            <div>Нет, вы используете наш веб-интерфейс и подключаете свой канал.</div>
          </div>
          <div>
            <div class="font-medium text-slate-200">Можно ли выключить отдельные функции?</div>
            <div>Да, любая функция настраивается отдельно в разделе Настройки.</div>
          </div>
          <div>
            <div class="font-medium text-slate-200">Безопасно ли хранить токены?</div>
            <div>Да, токены шифруются через Microsoft Data Protection и хранятся безопасно.</div>
          </div>
        </div>
      </section>

      <section class="glass p-10 text-center" ref={el => reveal(el, () => ({ delayMs: 150 }))}>
        <h2 class="text-2xl md:text-3xl font-semibold mb-3">Готовы попробовать?</h2>
        <p class="text-slate-300">Запустите бота за пару минут — регистрация бесплатная.</p>
        <div class="mt-6 flex justify-center gap-3">
          <button class="btn btn-primary" onClick={() => navigate('/login')}>Начать</button>
          <button class="btn btn-secondary" onClick={() => navigate('/features')}>Посмотреть возможности</button>
        </div>
      </section>
    </div>
  )
}


