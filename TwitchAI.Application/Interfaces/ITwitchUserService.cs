﻿using TwitchAI.Domain.Entites;

namespace TwitchAI.Application.Interfaces
{
    /// <summary>
    /// Сервис для проверки/добавления/получения пользователей из базы.
    /// </summary>
    public interface ITwitchUserService
    {
        Task<TwitchUser> GetOrCreateUserAsync(TwitchLib.Client.Models.ChatMessage message, CancellationToken cancellationToken);
        
        /// <summary>
        /// Получить или создать пользователя с информацией о том, был ли он создан
        /// </summary>
        /// <param name="message">Сообщение из чата</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Кортеж: пользователь и флаг создания</returns>
        Task<(TwitchUser User, bool WasCreated)> GetOrCreateUserWithStatusAsync(TwitchLib.Client.Models.ChatMessage message, CancellationToken cancellationToken);
        
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
        
        /// <summary>
        /// Получить reply-цепочку сообщений для контекста (последние 3 reply между пользователем и ботом)
        /// </summary>
        /// <param name="replyParentMessageId">ID сообщения, на которое отвечают</param>
        /// <param name="userId">ID пользователя</param>
        /// <param name="limit">Максимальное количество сообщений в контексте</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список сообщений reply-цепочки</returns>
        Task<List<ConversationMessage>> GetReplyChainContextAsync(string replyParentMessageId, Guid userId, int limit = 3, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Проверить, является ли сообщение ответом на сообщение от бота
        /// </summary>
        /// <param name="replyParentMessageId">ID сообщения, на которое отвечают</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>True, если это reply на сообщение бота</returns>
        Task<bool> IsReplyToBotMessageAsync(string replyParentMessageId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Получить сообщение чата по ID
        /// </summary>
        /// <param name="chatMessageId">ID сообщения чата</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Сообщение чата или null если не найдено</returns>
        Task<ChatMessage?> GetChatMessageByIdAsync(Guid chatMessageId, CancellationToken cancellationToken = default);
    }
}
