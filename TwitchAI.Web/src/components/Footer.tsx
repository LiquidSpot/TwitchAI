 

export default function Footer() {
  return (
    <footer class="mt-24 border-t border-white/10 bg-slate-900/60">
      <div class="max-w-6xl mx-auto px-4 py-10 grid md:grid-cols-3 gap-8 text-slate-300">
        <div>
          <div class="text-white font-semibold mb-3">Контакты</div>
          <ul class="space-y-1">
            <li>Email: <a class="underline hover:text-white" href="mailto:liquidspot.live@gmail.com">liquidspot.live@gmail.com</a></li>
            <li>Telegram: <a class="underline hover:text-white" href="https://t.me/LiquidSpoty" target="_blank" rel="noreferrer">@LiquidSpoty</a></li>
            <li>
              Discord: <a class="underline hover:text-white" href="https://discord.com/users/344125218014625793" target="_blank" rel="noreferrer">LiquidSpot</a>
            </li>
          </ul>
        </div>

        <div>
          <div class="text-white font-semibold mb-3">Лицензия</div>
          <p class="text-sm">Проект распространяется по лицензии Apache 2.0.</p>
          <div class="mt-2 flex gap-3">
            <a href="/docs" class="btn btn-secondary">Документация</a>
            <a class="btn btn-secondary" href="https://www.apache.org/licenses/LICENSE-2.0" target="_blank" rel="noreferrer">Apache 2.0</a>
          </div>
        </div>

        <div>
          <div class="text-white font-semibold mb-3">Мы на карте</div>
          <div class="rounded-xl overflow-hidden border border-white/10">
            <iframe
              title="Map"
              src="https://www.openstreetmap.org/export/embed.html?bbox=69.275%2C41.31%2C69.292%2C41.323&layer=mapnik&marker=41.3166674%2C69.2839036"
              class="w-full h-40"
              loading="lazy"
            />
          </div>
          <div class="mt-2 text-sm">
            <a class="underline hover:text-white" href="https://www.google.com/maps/place/Central+Post+Office/@41.3157001,69.2846963,17z/data=!4m15!1m8!3m7!1s0x38aef4d50c7d70cf:0xe052d7ca52aa8e25!2s100000,+Tashkent!3b1!8m2!3d41.3102836!4d69.2903244!16s%2Fg%2F11twpdg__3!3m5!1s0x38ae8b2d901c2def:0x1bb221455121c6e8!8m2!3d41.3166674!4d69.2839036!16s%2Fg%2F1tx1rkyz?entry=ttu&g_ep=EgoyMDI1MDgwNi4wIKXMDSoASAFQAw%3D%3D" target="_blank" rel="noreferrer">Открыть в Google Maps</a>
          </div>
        </div>
      </div>

      <div class="text-center text-slate-400 text-sm py-6 border-t border-white/5">
        © 2025 TwitchAI Project. Все права защищены. Apache-2.0
      </div>
    </footer>
  )
}


