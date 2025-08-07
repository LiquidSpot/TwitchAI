# Конфигурация TwitchAI

## Настройка для разработки

### 1. Клонирование репозитория
```bash
git clone https://github.com/your-repo/TwitchAI.git
cd TwitchAI
```

### 2. Настройка конфигурации

#### API проект (TwitchAI.Api)
Скопируйте `appsettings.Environment.json` в `appsettings.Development.json` и заполните реальными данными:

```json
{
  "AppConfiguration": {
    "MigrateDb": false,
    "SoundAlerts": "C:\\Path\\To\\Your\\SoundAlerts\\",
    "CooldownSecounds": 25,
    "OpenAiApiVersion": 1,
    "OpenAi": {
      "OrganizationId": "your_real_openai_organization_id",
      "ProjectId": "your_real_openai_project_id",
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
    }
  },
  "AuthClientsConfig": {
    "Clients": {
      "OpenAiClient": {
        "Token": "sk-proj-YOUR_REAL_OPENAI_API_KEY_HERE"
      }
    }
  },
  "ConnectionStrings": {
    "ConnectionString": "Host=localhost;Port=5432;Database=your_db;Username=your_user;Password=your_password;..."
  },
  "TwitchConfiguration": {
    "ChannelName": "your_actual_twitch_channel",
    "BotUsername": "your_bot_username",
    "BotAccessToken": "your_bot_access_token",
    "BotRefreshToken": "your_bot_refresh_token",
    "ClientId": "your_twitch_client_id",
    "IrcServer": "irc.chat.twitch.tv"
  }
}
```

#### AppHost проект (TwitchAI.AppHost)
Скопируйте `appsettings.Environment.json` в `appsettings.Development.json` (если нужно).

### 3. Необходимые переменные окружения

#### OpenAI API Key
1. Зарегистрируйтесь на [OpenAI Platform](https://platform.openai.com/)
2. Создайте API ключ
3. Замените `YOUR_OPENAI_API_KEY_HERE` на ваш реальный ключ
4. Получите Organization ID и Project ID из настроек OpenAI
5. Настройте доступные движки в массиве `AvailableEngines`:
   - `gpt-4o-2024-11-20`: GPT-4o модель (рекомендуется)
   - `gpt-4.1-2025-04-14`: GPT-4.1 модель
   - `chatgpt-4o-latest`: Последняя версия ChatGPT-4o
   - `o4-mini-2025-04-16`: Облегченная версия O4
   - `o3-2025-04-16`: O3 модель
   
   Пользователи смогут переключаться между этими движками командой `!engine <название>`

#### PostgreSQL Database
1. Установите PostgreSQL
2. Создайте базу данных
3. Обновите строку подключения в `ConnectionStrings.ConnectionString`

#### Twitch Configuration
1. Создайте Twitch приложение в [Twitch Developer Console](https://dev.twitch.tv/console/apps)
2. Получите Client ID и Client Secret
3. Создайте бота и получите Access Token и Refresh Token
4. Заполните все поля в `TwitchConfiguration`:
   - `ChannelName`: имя канала для мониторинга
   - `BotUsername`: имя бота
   - `BotAccessToken`: токен доступа бота
   - `BotRefreshToken`: refresh token для обновления токена
   - `ClientId`: ID приложения из Twitch Developer Console

#### Sound Alerts (опционально)
1. Создайте папку для звуковых файлов
2. Обновите путь в `AppConfiguration.SoundAlerts`
3. Добавьте .mp3 или .wav файлы в папку

### 4. Запуск приложения

```bash
# Из корневой папки проекта
dotnet run --project TwitchAI.AppHost
```

## Структура конфигурации

### AppConfiguration
- `MigrateDb`: Автоматическое применение миграций при старте
- `SoundAlerts`: Путь к папке со звуковыми файлами
- `CooldownSecounds`: Время отката для команд (в секундах)
- `OpenAiApiVersion`: Версия OpenAI API (1 = Responses API, 0 = Chat Completions API)
- `OpenAi`: Настройки OpenAI
  - `OrganizationId`: ID организации OpenAI
  - `ProjectId`: ID проекта OpenAI
  - `Model`: Модель OpenAI по умолчанию (например, "gpt-4o-2024-11-20")
  - `MaxTokens`: Максимальное количество токенов для ответа (по умолчанию 512)
  - `Temperature`: Температура генерации текста (0.0-1.0, по умолчанию 0.3)
  - `AvailableEngines`: Массив доступных движков OpenAI для команды !engine

### AuthClientsConfig
Настройки для внешних API клиентов (OpenAI).

### ConnectionStrings
Строки подключения к базам данных.

### Serilog
Настройки логирования.

### TwitchConfiguration
Настройки для интеграции с Twitch:
- `ChannelName`: Имя канала для мониторинга
- `BotUsername`: Имя пользователя бота
- `BotAccessToken`: Токен доступа бота
- `BotRefreshToken`: Refresh token для обновления токена
- `ClientId`: ID приложения Twitch
- `IrcServer`: IRC сервер Twitch (обычно irc.chat.twitch.tv)

## Миграция с Credentials.resx

Если вы использовали предыдущую версию с `Credentials.resx`, перенесите настройки в конфигурационные файлы:

### Twitch настройки
- `TwitichUserName` → `TwitchConfiguration.BotUsername`
- `TwitchTokenAccess` → `TwitchConfiguration.BotAccessToken`
- `TwitchRefreshToken` → `TwitchConfiguration.BotRefreshToken`
- `TwitchClientId` → `TwitchConfiguration.ClientId`
- `TwitchIRC` → `TwitchConfiguration.IrcServer`

### OpenAI настройки
- `GptOrganizationId` → `AppConfiguration.OpenAi.OrganizationId`
- `GptProjectId` → `AppConfiguration.OpenAi.ProjectId`

После миграции удалите файлы `Credentials.resx` и `Credentials.Designer.cs`.

## Безопасность

⚠️ **Важно**: Никогда не коммитьте файлы с реальными API ключами или паролями!

- Используйте `appsettings.Environment.json` как шаблон
- Реальные данные храните в `appsettings.Development.json` (добавлен в .gitignore)
- Для production используйте переменные окружения или Azure Key Vault

## Troubleshooting

### Ошибки подключения к OpenAI
- Проверьте правильность API ключа
- Убедитесь, что у вас есть кредиты на OpenAI аккаунте
- Проверьте, что используется правильная версия API
- Убедитесь, что модель в `Model` доступна для вашего аккаунта
- Проверьте, что все движки в `AvailableEngines` корректны и доступны
- Если команда `!engine` не работает, проверьте список доступных движков в конфигурации

### Ошибки подключения к базе данных
- Убедитесь, что PostgreSQL запущен
- Проверьте строку подключения
- Убедитесь, что база данных создана

### Ошибки Twitch интеграции
- Проверьте имя канала
- Убедитесь, что канал существует и доступен
- Проверьте правильность Bot Access Token
- Убедитесь, что бот имеет права на чтение чата
- Проверьте, что Client ID соответствует вашему приложению
- Убедитесь, что Refresh Token актуален

### Ошибки конфигурации
- Проверьте синтаксис JSON в appsettings файлах
- Убедитесь, что все обязательные поля заполнены
- Проверьте, что пути к файлам корректны (особенно SoundAlerts) 