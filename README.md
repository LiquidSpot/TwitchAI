# 🤖 TwitchAI - Интеллектуальный Twitch Бот

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-blue?style=for-the-badge&logo=dotnet)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white)
![OpenAI](https://img.shields.io/badge/OpenAI-412991?style=for-the-badge&logo=openai&logoColor=white)
![Twitch](https://img.shields.io/badge/Twitch-9146FF?style=for-the-badge&logo=twitch&logoColor=white)

**Современный AI-powered Twitch бот с архитектурой Clean Architecture**

[Быстрый старт](#-быстрый-старт) • [Функциональность](#-функциональность) • [Конфигурация](#-конфигурация) • [API](#-архитектура-сервисов)

</div>

---

TwitchAI - это продвинутый Twitch бот нового поколения, построенный на .NET 8 с использованием принципов Clean Architecture. Интегрирован с OpenAI GPT для интеллектуального взаимодействия, предоставляет богатый набор развлекательных и утилитарных функций для создания живого и интерактивного чата.

## ✨ Ключевые особенности

<table>
<tr>
<td width="50%">

### 🧠 **Искусственный интеллект**
- Интеграция с OpenAI GPT-4o/4.1/o3/o4
- Контекстные диалоги с памятью
- Множественные роли бота (neko, assistant, friend)
- Динамическое переключение AI движков

</td>
<td width="50%">

### 🎮 **Интерактивность**
- Звуковые алерты и эффекты
- Генерация комплиментов
- Случайные факты
- Информация о праздниках

</td>
</tr>
<tr>
<td width="50%">

### 🌍 **Многоязычность**
- Перевод на 5 языков
- Локализация праздников
- Поддержка Unicode эмодзи

</td>
<td width="50%">

### 📊 **Аналитика**
- Мониторинг зрителей в реальном времени
- История сообщений
- Статистика использования команд

</td>
</tr>
</table>

## 🏗️ Архитектура

Проект построен с использованием Clean Architecture и разделен на следующие слои:

- **TwitchAI.Api** - API слой и точка входа
- **TwitchAI.Application** - Бизнес-логика и Use Cases
- **TwitchAI.Domain** - Доменные модели и интерфейсы
- **TwitchAI.Infrastructure** - Реализация внешних зависимостей
- **TwitchAI.AppHost** - Хост приложения (.NET Aspire)

## 🎯 Функциональность

### 🧠 **AI Команды**
<details>
<summary><b>Интеллектуальное общение с контекстом</b></summary>

#### `!ai <сообщение>`
Основная команда для общения с искусственным интеллектом. Бот запоминает контекст разговора для каждого пользователя.

**Примеры:**
```
!ai Привет! Как дела?
!ai Расскажи интересную историю
!ai Помоги решить задачу по математике
```

**Особенности:**
- 🧠 Память контекста диалога
- 🎭 Адаптация к выбранной роли бота
- ⚡ Лимиты ответов для управления нагрузкой
- 🔄 Автоматическое переключение движков при ошибках

</details>

<details>
<summary><b>Управление ролями и настройками</b></summary>

#### `!role <роль>`
Изменение персональности бота. Каждая роль имеет уникальный стиль общения.

**Доступные роли:**
- `bot` 🤖 - Стандартный AI ассистент
- `neko` 🐱 - Игривый котик с эмодзи и мяуканьем
- `assistant` 👔 - Профессиональный помощник
- `friend` 😊 - Дружелюбный компаньон

#### `!engine <движок>`
Переключение между различными моделями OpenAI.

**Доступные движки:**
- `gpt-4o-2024-11-20` - Основная модель (рекомендуется)
- `gpt-4.1-2025-04-14` - Улучшенная версия
- `chatgpt-4o-latest` - Последняя версия ChatGPT
- `o4-mini-2025-04-16` - Быстрая модель
- `o3-2025-04-16` - Новейшая модель

#### `!limit <число>`
Установка лимита ответов AI для пользователя (от 1 до 50).

</details>

### 🎵 **Развлекательные команды**
<details>
<summary><b>Звуковые эффекты и развлечения</b></summary>

#### `!sound <название>`
Воспроизведение звуковых файлов из настроенной папки.

**Примеры:**
```
!sound applause    # Аплодисменты
!sound drumroll    # Барабанная дробь
!sound wow         # Звук удивления
```

**Особенности:**
- 🔊 Поддержка MP3 и WAV файлов
- ⏰ Кулдаун между воспроизведениями (10 сек)
- 🔇 Настраиваемая громкость
- 📁 Автоматическое сканирование папки со звуками

#### `!compliment [@пользователь]`
Генерация персонализированных комплиментов через AI.

**Примеры:**
```
!compliment @username    # Комплимент конкретному пользователю
!compliment             # Комплимент отправителю
```

**Особенности:**
- 🎨 AI-генерированные уникальные комплименты
- 💾 Резервные комплименты при сбое AI
- 👤 Персонализация по имени пользователя

#### `!fact`
Случайные интересные факты из настроенного файла.

**Особенности:**
- 📚 Загрузка фактов из текстового файла
- 🔄 Кэширование в памяти
- ⏱️ Кулдаун 1 минута между фактами
- 🎲 Случайный выбор из коллекции

</details>

### 🌍 **Утилиты и информация**
<details>
<summary><b>Переводчик и праздники</b></summary>

#### `!translate <язык> <текст>`
Перевод текста на поддерживаемые языки через OpenAI.

**Поддерживаемые языки:**
- `ru` 🇷🇺 Русский
- `en` 🇺🇸 Английский  
- `zh` 🇨🇳 Китайский
- `ja` 🇯🇵 Японский
- `es` 🇪🇸 Испанский

**Примеры:**
```
!translate en Привет мир!
!translate ru Hello world!
!translate ja こんにちは
```

#### `!holiday [дата]`
Информация о праздниках через OpenHolidays API.

**Примеры:**
```
!holiday              # Праздники сегодня
!holiday 2024-12-31   # Праздники на конкретную дату
```

**Особенности:**
- 🌍 Данные из международной базы праздников
- 🇷🇺 Автоматический перевод на русский язык
- 📅 Поддержка любых дат
- 🎉 Красивое форматирование с эмодзи

</details>

<details>
<summary><b>Статистика и мониторинг</b></summary>

#### `!viewers`
Статистика зрителей в чате в реальном времени.

**Информация:**
- 👥 Количество активных зрителей
- 📊 Статистика присутствия
- ⏰ Время последней активности

**Особенности:**
- 🔄 Обновление каждые 2 минуты
- 💾 Сохранение истории присутствия
- 📈 Аналитика активности чата

</details>

## 🔧 Use Cases и Handler'ы

### OpenAI Handler'ы
- **AiChatCommandHandler** - Обработка AI чата с сохранением контекста разговора
- **ChangeRoleCommandHandler** - Смена роли бота (bot, neko, etc.)
- **EngineCommandHandler** - Переключение между движками OpenAI
- **ReplyLimitCommandHandler** - Управление лимитами ответов AI

### Развлекательные Handler'ы
- **SoundChatCommandHandler** - Воспроизведение звуковых файлов
- **ComplimentCommandHandler** - Генерация персонализированных комплиментов
- **FactCommandHandler** - Выдача случайных фактов из файла
- **HolidayCommandHandler** - Информация о праздниках через OpenHolidays API

### Утилиты Handler'ы
- **TranslateCommandHandler** - Перевод текста на поддерживаемые языки
- **ViewerStatsCommandHandler** - Статистика и мониторинг зрителей
- **ParseChatMessageQueryHandler** - Парсинг и обработка сообщений чата
- **HandleMessageCommandHandler** - Основной обработчик входящих сообщений

## ⚙️ Технологии

- **.NET 8** - Основной фреймворк
- **ASP.NET Core** - Web API
- **Entity Framework Core** - ORM для работы с БД
- **PostgreSQL** - База данных
- **OpenAI API** - Интеграция с GPT
- **TwitchLib** - Библиотека для работы с Twitch
- **MediatR** - Паттерн Mediator для CQRS
- **AutoMapper** - Маппинг объектов
- **Serilog** - Логирование
- **.NET Aspire** - Оркестрация и мониторинг

## 🚀 Быстрый старт

### Предварительные требования
- .NET 8 SDK
- PostgreSQL
- OpenAI API ключ
- Twitch токены (через Developer Console или TwitchTokenGenerator)

### Установка

1. **Клонируйте репозиторий**
   ```bash
   git clone https://github.com/your-username/TwitchAI.git
   cd TwitchAI
   ```

2. **Настройте конфигурацию**
   
   Скопируйте `TwitchAI.Api/appsettings.Environment.json` в `appsettings.Development.json` и заполните:

   ```json
   {
     "AppConfiguration": {
       "OpenAi": {
         "OrganizationId": "your_openai_org_id",
         "ProjectId": "your_openai_project_id",
         "Model": "gpt-4o-2024-11-20",
         "MaxTokens": 512,
         "Temperature": 0.3,
         "AvailableEngines": [
           "gpt-4o-2024-11-20",
           "gpt-4.1-2025-04-14",
           "chatgpt-4o-latest",
           "o4-mini-2025-04-16",
           "o3-2025-04-16"
         ]
       },
       "SoundAlerts": "C:\\Path\\To\\Your\\Sounds\\",
       "Facts": "C:\\Path\\To\\Facts.txt"
     },
     "AuthClientsConfig": {
       "Clients": {
         "OpenAiClient": {
           "Token": "sk-proj-YOUR_OPENAI_API_KEY"
         }
       }
     },
     "ConnectionStrings": {
       "ConnectionString": "Host=localhost;Port=5432;Database=twitch_ai_db;Username=your_user;Password=your_password;"
     },
     "TwitchConfiguration": {
       "ChannelName": "your_channel",
       "BotUsername": "your_bot_name",
       "BotAccessToken": "your_bot_token",
       "ClientId": "your_twitch_client_id"
     }
   }
   ```

3. **Получите Twitch токены**

   **Вариант 1: TwitchTokenGenerator (Рекомендуется для разработки)**
   
   Простой и быстрый способ получить все необходимые токены:
   
   1. Перейдите на [TwitchTokenGenerator.com](https://twitchtokengenerator.com/)
   2. Выберите необходимые разрешения (scopes):
      - ✅ `chat:read` - чтение сообщений чата
      - ✅ `chat:edit` - отправка сообщений в чат
      - ✅ `channel:moderate` - модерация канала (опционально)
   3. Нажмите **"Generate Token!"** и авторизуйтесь через Twitch
   4. Скопируйте полученные токены:
      - **Access Token** → `BotAccessToken`
      - **Refresh Token** → `BotRefreshToken`
      - **Client ID** → `ClientId`

   **Вариант 2: Twitch Developer Console (Для production)**
   
   1. Создайте приложение в [Twitch Developer Console](https://dev.twitch.tv/console/apps)
   2. Получите Client ID и Client Secret
   3. Используйте OAuth flow для получения токенов

   > 💡 **Совет:** [TwitchTokenGenerator.com](https://twitchtokengenerator.com/) идеально подходит для разработки и тестирования, так как автоматически генерирует все необходимые токены за несколько кликов. Для production рекомендуется использовать собственное Twitch приложение.

4. **Создайте базу данных**
   ```bash
   dotnet ef database update --project TwitchAI.Infrastructure
   ```

5. **Запустите приложение**
   ```bash
   dotnet run --project TwitchAI.AppHost
   ```

## 📁 Структура проекта

```
TwitchAI/
├── TwitchAI.Api/                 # API контроллеры и конфигурация
├── TwitchAI.Application/         # Бизнес-логика
│   ├── UseCases/                 # Use Cases и Handler'ы
│   │   ├── OpenAi/              # AI функциональность
│   │   ├── Songs/               # Звуковые команды
│   │   ├── Translation/         # Переводчик
│   │   ├── Viewers/             # Статистика зрителей
│   │   ├── Facts/               # Факты
│   │   ├── Holidays/            # Праздники
│   │   ├── Compliment/          # Комплименты
│   │   ├── Parser/              # Парсер сообщений
│   │   └── Twitch/              # Twitch интеграция
│   ├── Models/                  # Модели приложения
│   ├── Interfaces/              # Интерфейсы
│   └── Dto/                     # Data Transfer Objects
├── TwitchAI.Domain/             # Доменные модели
├── TwitchAI.Infrastructure/     # Внешние зависимости
└── TwitchAI.AppHost/            # Aspire хост
```

## 🎮 Роли бота

Бот поддерживает различные роли, каждая со своей персональностью:

- **bot** 🤖 - Стандартный AI ассистент
- **neko** 🐱 - Игривый котик-помощник
- **assistant** 👔 - Профессиональный помощник
- **friend** 😊 - Дружелюбный компаньон

## 🔧 Доступные движки OpenAI

- **gpt-4o-2024-11-20** - Основная модель GPT-4o
- **gpt-4.1-2025-04-14** - Улучшенная версия GPT-4
- **chatgpt-4o-latest** - Последняя версия ChatGPT-4o
- **o4-mini-2025-04-16** - Облегченная версия
- **o3-2025-04-16** - Новейшая модель O3

## 🌍 Поддерживаемые языки перевода

- 🇷🇺 Русский (ru)
- 🇺🇸 Английский (en)
- 🇨🇳 Китайский (zh)
- 🇯🇵 Японский (ja)
- 🇪🇸 Испанский (es)

## 📊 Мониторинг и логирование

- **Serilog** - Структурированное логирование
- **Health Checks** - Проверка состояния сервисов
- **Metrics** - Метрики производительности
- **Distributed Tracing** - Трассировка запросов

## 🤝 Участие в разработке

1. Форкните репозиторий
2. Создайте ветку для новой функции (`git checkout -b feature/amazing-feature`)
3. Зафиксируйте изменения (`git commit -m 'Add amazing feature'`)
4. Отправьте в ветку (`git push origin feature/amazing-feature`)
5. Создайте Pull Request

## 📝 Лицензия

Этот проект лицензирован под лицензией MIT - см. файл [LICENSE](LICENSE.txt) для подробностей.

## 🆘 Поддержка

Если у вас возникли вопросы или проблемы:

1. Проверьте [CONFIGURATION.md](CONFIGURATION.md) для подробных инструкций по настройке
2. Создайте Issue в GitHub
3. Проверьте логи приложения в папке `logs/`

## 🎯 Планы развития

- [ ] Веб-интерфейс для управления ботом
- [ ] Поддержка кастомных команд
- [ ] Интеграция с другими AI провайдерами
- [ ] Система достижений для зрителей
- [ ] Мини-игры в чате
- [ ] Поддержка Discord интеграции

---

Создано с ❤️ для Twitch сообщества
