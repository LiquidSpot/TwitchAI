using TwitchAI.Domain.Entites;

namespace TwitchAI.Application.Interfaces
{
    /// <summary>
    /// Сервис для проверки/добавления/получения пользователей из базы.
    /// </summary>
    public interface ITwitchUserService
    {
        Task<TwitchUser> GetOrCreateUserAsync(TwitchLib.Client.Models.ChatMessage message, CancellationToken cancellationToken);
        Task<ChatMessage> AddMessage(TwitchUser user, TwitchLib.Client.Models.ChatMessage message, CancellationToken cancellationToken);
        
        /// <summary>
        /// Получить пользователя по ID
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Пользователь или null если не найден</returns>
        Task<TwitchUser?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Сохранить сообщение пользователя в контекст диалога
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <param name="message">Сообщение пользователя</param>
        /// <param name="chatMessageId">ID сообщения из чата (необязательно)</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Созданное сообщение в контексте</returns>
        Task<ConversationMessage> AddUserMessageToContextAsync(TwitchUser user, string message, Guid? chatMessageId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Сохранить ответ GPT в контекст диалога
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <param name="gptResponse">Ответ от GPT</param>
        /// <param name="openAiResponseId">ID ответа OpenAI</param>
        /// <param name="modelName">Название модели</param>
        /// <param name="tokenCount">Количество токенов</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Созданное сообщение в контексте</returns>
        Task<ConversationMessage> AddGptResponseToContextAsync(TwitchUser user, string gptResponse, string? openAiResponseId = null, string? modelName = null, int? tokenCount = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить последние сообщения пользователя для контекста (по умолчанию 3 последних)
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <param name="limit">Количество сообщений для получения</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список сообщений для контекста</returns>
        Task<List<ConversationMessage>> GetUserConversationContextAsync(Guid userId, int limit = 3, CancellationToken cancellationToken = default);

        /// <summary>
        /// Очистить контекст диалога пользователя (например, если пользователь хочет начать новый диалог)
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Количество удаленных сообщений</returns>
        Task<int> ClearUserConversationContextAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
