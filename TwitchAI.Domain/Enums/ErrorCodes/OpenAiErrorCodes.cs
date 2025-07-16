#pragma warning disable CS1591
namespace TwitchAI.Domain.Enums.ErrorCodes;

/// <summary>
/// Коды ошибок для OpenAI API
/// </summary>
public enum OpenAiErrorCodes
{
    #region OpenAI API Errors (11000 .. 11099)

    /// <summary>
    /// Ошибка при вызове OpenAI API
    /// </summary>
    ApiCallError = 11000,

    /// <summary>
    /// Неверный API ключ
    /// </summary>
    InvalidApiKey = 11001,

    /// <summary>
    /// Превышен лимит запросов
    /// </summary>
    RateLimitExceeded = 11002,

    /// <summary>
    /// Недостаточно средств на счету
    /// </summary>
    InsufficientFunds = 11003,

    /// <summary>
    /// Неверный формат запроса
    /// </summary>
    InvalidRequestFormat = 11004,

    /// <summary>
    /// Модель не найдена
    /// </summary>
    ModelNotFound = 11005,

    /// <summary>
    /// Превышен лимит токенов
    /// </summary>
    TokenLimitExceeded = 11006,

    /// <summary>
    /// Пустой ответ от API
    /// </summary>
    EmptyResponse = 11007,

    /// <summary>
    /// Таймаут при обращении к API
    /// </summary>
    ApiTimeout = 11008,

    /// <summary>
    /// Неподдерживаемая модель
    /// </summary>
    UnsupportedModel = 11009,

    /// <summary>
    /// Неверный формат ответа
    /// </summary>
    InvalidResponseFormat = 11010,

    #endregion
} 