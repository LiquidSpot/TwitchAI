using TwitchAI.Domain.Entites.Abstract;

namespace TwitchAI.Domain.Entites
{
    /// <summary>
    /// Сообщение в истории диалога между пользователем и GPT
    /// </summary>
    public class ConversationMessage : Entity
    {
        /// <summary>
        /// Идентификатор пользователя Twitch
        /// </summary>
        public Guid TwitchUserId { get; set; }

        /// <summary>
        /// Навигационное свойство к пользователю
        /// </summary>
        public virtual TwitchUser TwitchUser { get; set; } = null!;

        /// <summary>
        /// Роль отправителя сообщения (user, assistant, system)
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Содержимое сообщения
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Порядковый номер сообщения в диалоге для сортировки
        /// </summary>
        public int MessageOrder { get; set; }

        /// <summary>
        /// Идентификатор ответа OpenAI (если это сообщение от GPT)
        /// </summary>
        public string? OpenAiResponseId { get; set; }

        /// <summary>
        /// Модель OpenAI, которая использовалась для ответа
        /// </summary>
        public string? OpenAiModel { get; set; }

        /// <summary>
        /// Количество токенов в сообщении
        /// </summary>
        public int? TokenCount { get; set; }

        /// <summary>
        /// Связанное сообщение из Twitch чата (если есть)
        /// </summary>
        public Guid? ChatMessageId { get; set; }

        /// <summary>
        /// Навигационное свойство к сообщению из чата
        /// </summary>
        public virtual ChatMessage? ChatMessage { get; set; }
    }
} 