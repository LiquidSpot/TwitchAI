using System.Text.Json.Serialization;

namespace TwitchAI.Application.Dto.Response
{
    /// <summary>
    /// Выходные данные Responses API
    /// </summary>
    public class ResponsesOutputDto
    {
        /// <summary>
        /// Идентификатор вывода
        /// </summary>
        public string? id { get; set; }

        /// <summary>
        /// Тип вывода (message, function_call, etc.)
        /// </summary>
        public string type { get; set; } = string.Empty;

        /// <summary>
        /// Роль (assistant, user, system)
        /// </summary>
        public string? role { get; set; }

        /// <summary>
        /// Контент вывода
        /// </summary>
        public ResponsesOutputContentDto[]? content { get; set; }

        /// <summary>
        /// Статус вывода
        /// </summary>
        public string? status { get; set; }

        /// <summary>
        /// Результат для вызовов функций
        /// </summary>
        public object? result { get; set; }

        /// <summary>
        /// Название функции (для function_call типа)
        /// </summary>
        public string? function_name { get; set; }

        /// <summary>
        /// Аргументы функции (для function_call типа)
        /// </summary>
        public object? arguments { get; set; }
    }
} 