using TwitchAI.Application.Models;
using TwitchAI.Domain.Entites;

namespace TwitchAI.Application.Interfaces
{
    /// <summary>
    /// Сервис для управления сообщениями чата
    /// </summary>
    public interface IChatMessageService
    {
        /// <summary>
        /// Добавить сообщение пользователя в чат
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <param name="message">Сообщение из Twitch</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Созданное сообщение чата</returns>
        Task<ChatMessage> AddMessageAsync(TwitchUser user, TwitchLib.Client.Models.ChatMessage message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить сообщение чата по ID
        /// </summary>
        /// <param name="chatMessageId">ID сообщения чата</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Сообщение чата или null если не найдено</returns>
        Task<ChatMessage?> GetChatMessageByIdAsync(Guid chatMessageId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Получить сообщение чата по MessageId из Twitch
        /// </summary>
        /// <param name="messageId">MessageId из Twitch</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Сообщение чата или null если не найдено</returns>
        Task<ChatMessage?> GetChatMessageByMessageIdAsync(string messageId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Сохранить сообщение бота
        /// </summary>
        /// <param name="sentMessage">Отправленное сообщение бота</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Созданное сообщение чата</returns>
        Task<ChatMessage> SaveBotMessageAsync(BotSentMessage sentMessage, CancellationToken cancellationToken = default);
    }
}