export default function Pricing() {
  const tiers = [
    { name: 'Free', price: '$0', features: ['1 канал', 'Базовые функции', 'Community support'] },
    { name: 'Pro', price: '$10/mo', features: ['До 3 каналов', 'Все интеграции', 'Приоритетная поддержка'] },
    { name: 'Business', price: 'Contact', features: ['Мульти-каналы', 'SLA', 'Кастомные интеграции'] },
  ]
  return (
    <div class="max-w-6xl mx-auto px-4">
      <h1 class="text-3xl mb-6">Цены</h1>
      <div class="grid md:grid-cols-3 gap-4">
        {tiers.map(t => (
          <div class="card-secondary glow-hover p-6 text-center">
            <div class="text-xl">{t.name}</div>
            <div class="text-3xl my-2">{t.price}</div>
            <ul class="text-slate-300 space-y-1">
              {t.features.map(f => <li>{f}</li>)}
            </ul>
            <button class="btn btn-primary mt-4 w-full">Выбрать</button>
          </div>
        ))}
      </div>
    </div>
  )
}


