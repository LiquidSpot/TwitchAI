using System.Text.Json.Serialization;

namespace TwitchAI.Application.Dto.Response
{
    /// <summary>
    /// Контент в выходных данных Responses API
    /// </summary>
    public class ResponsesOutputContentDto
    {
        /// <summary>
        /// Тип контента (output_text, image, etc.)
        /// </summary>
        public string type { get; set; } = string.Empty;

        /// <summary>
        /// Текстовое содержимое
        /// </summary>
        public string? text { get; set; }

        /// <summary>
        /// Аннотации к контенту
        /// </summary>
        public object[]? annotations { get; set; }

        /// <summary>
        /// Логарифмические вероятности
        /// </summary>
        public object[]? logprobs { get; set; }

        /// <summary>
        /// Дополнительные данные изображения (для image типа)
        /// </summary>
        public object? image_url { get; set; }

        /// <summary>
        /// Данные файла (для file типа)
        /// </summary>
        public object? file_id { get; set; }
    }
} 