using System.Text.Json.Serialization;

namespace TwitchAI.Application.Dto.Request
{
    /// <summary>
    /// Модель запроса для нового Responses API
    /// </summary>
    public class ResponsesRequestDto
    {
        /// <summary>
        /// Название модели для использования
        /// </summary>
        public string model { get; set; } = "gpt-4o";

        /// <summary>
        /// Входные данные для обработки (может быть строкой или массивом объектов)
        /// </summary>
        public object input { get; set; } = string.Empty;

        /// <summary>
        /// Максимальное количество выходных токенов
        /// </summary>
        [JsonPropertyName("max_output_tokens")]
        public int? max_output_tokens { get; set; }

        /// <summary>
        /// Температура для генерации (0.0 - 2.0)
        /// </summary>
        public double? temperature { get; set; } = 1.0;

        /// <summary>
        /// Top-p для генерации
        /// </summary>
        public double? top_p { get; set; } = 1.0;

        /// <summary>
        /// Включать ли потоковую передачу
        /// </summary>
        public bool? stream { get; set; } = false;

        /// <summary>
        /// Сохранять ли ответ для дальнейшего использования
        /// </summary>
        public bool? store { get; set; } = false;

        /// <summary>
        /// Метаданные для запроса
        /// </summary>
        public Dictionary<string, object>? metadata { get; set; }

        /// <summary>
        /// Инструкции для модели
        /// </summary>
        public string? instructions { get; set; }

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
        /// Усилие для рассуждения (для reasoning моделей)
        /// </summary>
        public object? reasoning { get; set; }

        /// <summary>
        /// Что включать в ответ
        /// </summary>
        public string[]? include { get; set; }
    }
} 