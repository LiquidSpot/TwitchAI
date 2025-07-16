using System.Text.Json.Serialization;

namespace TwitchAI.Application.Dto.Response
{
    /// <summary>
    /// Статистика использования токенов в Responses API
    /// </summary>
    public class ResponsesUsageDto
    {
        /// <summary>
        /// Количество входных токенов
        /// </summary>
        public int input_tokens { get; set; }

        /// <summary>
        /// Детали входных токенов
        /// </summary>
        public ResponsesInputTokensDetailsDto? input_tokens_details { get; set; }

        /// <summary>
        /// Количество выходных токенов
        /// </summary>
        public int output_tokens { get; set; }

        /// <summary>
        /// Общее количество токенов
        /// </summary>
        public int total_tokens { get; set; }

        /// <summary>
        /// Детали выходных токенов
        /// </summary>
        public ResponsesOutputTokensDetailsDto? output_tokens_details { get; set; }
    }

    /// <summary>
    /// Детали входных токенов
    /// </summary>
    public class ResponsesInputTokensDetailsDto
    {
        /// <summary>
        /// Количество кэшированных токенов
        /// </summary>
        public int cached_tokens { get; set; }
    }

    /// <summary>
    /// Детали выходных токенов
    /// </summary>
    public class ResponsesOutputTokensDetailsDto
    {
        /// <summary>
        /// Токены рассуждения (для reasoning моделей)
        /// </summary>
        public int reasoning_tokens { get; set; }
    }
} 