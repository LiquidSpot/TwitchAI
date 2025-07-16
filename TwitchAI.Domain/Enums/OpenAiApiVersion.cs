namespace TwitchAI.Domain.Enums;

/// <summary>
/// Версия OpenAI API для использования
/// </summary>
public enum OpenAiApiVersion
{
    /// <summary>
    /// Старый Chat Completions API (для совместимости)
    /// </summary>
    ChatCompletions = 0,

    /// <summary>
    /// Новый Responses API (рекомендуется)
    /// </summary>
    Responses = 1
} 