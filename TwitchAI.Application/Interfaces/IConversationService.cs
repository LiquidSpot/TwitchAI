using TwitchAI.Domain.Entites;

namespace TwitchAI.Application.Interfaces
{
    /// <summary>
    /// Сервис для управления контекстом разговора
    /// </summary>
    public interface IConversationService
    {
        /// <summary>
        /// Добавить сообщение пользователя в контекст диалога
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <param name="message">Сообщение пользователя</param>
        /// <param name="chatMessageId">ID сообщения из чата (необязательно)</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Созданное сообщение контекста</returns>
        Task<ConversationMessage> AddUserMessageToContextAsync(TwitchUser user, string message, Guid? chatMessageId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Добавить ответ GPT в контекст диалога
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <param name="gptResponse">Ответ от GPT</param>
        /// <param name="openAiResponseId">ID ответа OpenAI (необязательно)</param>
        /// <param name="modelName">Имя модели (необязательно)</param>
        /// <param name="tokenCount">Количество токенов (необязательно)</param>
        /// <param name="chatMessageId">ID сообщения из чата (необязательно)</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Созданное сообщение контекста</returns>
        Task<ConversationMessage> AddGptResponseToContextAsync(TwitchUser user, string gptResponse, string? openAiResponseId = null, string? modelName = null, int? tokenCount = null, Guid? chatMessageId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить контекст диалога пользователя
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <param name="limit">Лимит сообщений (по умолчанию 3)</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список сообщений контекста</returns>
        Task<List<ConversationMessage>> GetUserConversationContextAsync(Guid userId, int limit = 3, CancellationToken cancellationToken = default);

        /// <summary>
        /// Очистить контекст диалога пользователя
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Количество удаленных сообщений</returns>
        Task<int> ClearUserConversationContextAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить контекст цепочки ответов (reply chain)
        /// </summary>
        /// <param name="replyParentMessageId">ID родительского сообщения</param>
        /// <param name="userId">ID пользователя</param>
        /// <param name="limit">Лимит сообщений (по умолчанию 3)</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список сообщений контекста</returns>
        Task<List<ConversationMessage>> GetReplyChainContextAsync(string replyParentMessageId, Guid userId, int limit = 3, CancellationToken cancellationToken = default);

        /// <summary>
        /// Проверить, является ли сообщение ответом на сообщение бота
        /// </summary>
        /// <param name="replyParentMessageId">ID родительского сообщения</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>True если это ответ на сообщение бота</returns>
        Task<bool> IsReplyToBotMessageAsync(string replyParentMessageId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Связать сообщение контекста с сообщением чата бота
        /// </summary>
        /// <param name="conversationMessageId">ID сообщения контекста</param>
        /// <param name="botChatMessageId">ID сообщения чата бота</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>True если связь установлена успешно</returns>
        Task<bool> LinkConversationWithBotMessageAsync(Guid conversationMessageId, Guid botChatMessageId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Найти conversation по временному MessageId из ChatMessage
        /// </summary>
        /// <param name="tempMessageId">Временный MessageId из ChatMessage</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>ConversationMessage если найдено, иначе null</returns>
        Task<ConversationMessage?> FindConversationByTempMessageIdAsync(string tempMessageId, CancellationToken cancellationToken = default);
    }
}