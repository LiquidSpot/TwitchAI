using System.Text.Json.Serialization;

namespace TwitchAI.Application.Dto.Response
{
    /// <summary>
    /// Основной ответ Responses API
    /// </summary>
    public class ResponsesApiResponseDto
    {
        /// <summary>
        /// Идентификатор ответа
        /// </summary>
        public string id { get; set; } = string.Empty;

        /// <summary>
        /// Тип объекта (всегда "response")
        /// </summary>
        public string @object { get; set; } = "response";

        /// <summary>
        /// Время создания (Unix timestamp)
        /// </summary>
        public long created_at { get; set; }

        /// <summary>
        /// Модель, которая была использована
        /// </summary>
        public string? model { get; set; }

        /// <summary>
        /// Статус ответа (completed, in_progress, failed, etc.)
        /// </summary>
        public string status { get; set; } = string.Empty;

        /// <summary>
        /// Фоновое выполнение
        /// </summary>
        public bool? background { get; set; }

        /// <summary>
        /// Уровень сервиса
        /// </summary>
        public string? service_tier { get; set; }

        /// <summary>
        /// Выходные данные
        /// </summary>
        public ResponsesOutputDto[]? output { get; set; }

        /// <summary>
        /// Статистика использования токенов
        /// </summary>
        public ResponsesUsageDto? usage { get; set; }

        /// <summary>
        /// Ошибка, если произошла
        /// </summary>
        public object? error { get; set; }

        /// <summary>
        /// Детали незавершенного ответа
        /// </summary>
        public object? incomplete_details { get; set; }

        /// <summary>
        /// Инструкции для модели
        /// </summary>
        public string? instructions { get; set; }

        /// <summary>
        /// Метаданные ответа
        /// </summary>
        public Dictionary<string, object>? metadata { get; set; }

        /// <summary>
        /// Температура, использованная для генерации
        /// </summary>
        public double? temperature { get; set; }

        /// <summary>
        /// Top-p, использованный для генерации
        /// </summary>
        public double? top_p { get; set; }

        /// <summary>
        /// Максимальное количество выходных токенов
        /// </summary>
        [JsonPropertyName("max_output_tokens")]
        public int? max_output_tokens { get; set; }

        /// <summary>
        /// Максимальное количество вызовов инструментов
        /// </summary>
        [JsonPropertyName("max_tool_calls")]
        public int? max_tool_calls { get; set; }

        /// <summary>
        /// Доступные инструменты
        /// </summary>
        public object[]? tools { get; set; }

        /// <summary>
        /// Выбор инструмента
        /// </summary>
        public object? tool_choice { get; set; }

        /// <summary>
        /// Параллельные вызовы инструментов
        /// </summary>
        public bool? parallel_tool_calls { get; set; }

        /// <summary>
        /// Данные рассуждения (для reasoning моделей)
        /// </summary>
        public object? reasoning { get; set; }

        /// <summary>
        /// Усилие рассуждения (для reasoning моделей)
        /// </summary>
        public object? reasoning_effort { get; set; }

        /// <summary>
        /// Идентификатор предыдущего ответа
        /// </summary>
        public string? previous_response_id { get; set; }

        /// <summary>
        /// Информация об обрезании
        /// </summary>
        public object? truncation { get; set; }

        /// <summary>
        /// Пользователь
        /// </summary>
        public string? user { get; set; }

        /// <summary>
        /// Сохранять ли ответ
        /// </summary>
        public bool? store { get; set; }

        /// <summary>
        /// Количество top log probabilities
        /// </summary>
        public int? top_logprobs { get; set; }

        /// <summary>
        /// Текстовый формат ответа
        /// </summary>
        public ResponsesTextDto? text { get; set; }
    }

    /// <summary>
    /// Формат текстового ответа
    /// </summary>
    public class ResponsesTextDto
    {
        /// <summary>
        /// Формат текста
        /// </summary>
        public ResponsesTextFormatDto? format { get; set; }
    }

    /// <summary>
    /// Детали формата текста
    /// </summary>
    public class ResponsesTextFormatDto
    {
        /// <summary>
        /// Тип формата (обычно "text")
        /// </summary>
        public string type { get; set; } = "text";
    }
} 