using TwitchAI.Domain.Entites.Abstract;

namespace TwitchAI.Domain.Entites
{
    /// <summary>
    /// Сущность для отслеживания присутствия зрителей в чате
    /// </summary>
    public class ViewerPresence : Entity
    {
        /// <summary>
        /// ID пользователя Twitch
        /// </summary>
        public Guid TwitchUserId { get; set; }

        /// <summary>
        /// Связь с пользователем
        /// </summary>
        public virtual TwitchUser TwitchUser { get; set; } = null!;

        /// <summary>
        /// Название канала
        /// </summary>
        public string ChannelName { get; set; } = string.Empty;

        /// <summary>
        /// Когда зритель был последний раз замечен в чате
        /// </summary>
        public DateTime LastSeenInChat { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Когда зритель последний раз писал сообщение
        /// </summary>
        public DateTime? LastMessageAt { get; set; }

        /// <summary>
        /// Является ли зритель активным (присутствует в чате)
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Является ли зритель "молчаливым" (не писал сообщения)
        /// </summary>
        public bool IsSilent { get; set; } = true;

        /// <summary>
        /// Общее время присутствия в чате (в минутах)
        /// </summary>
        public int TotalPresenceMinutes { get; set; }

        /// <summary>
        /// Количество сессий присутствия
        /// </summary>
        public int SessionCount { get; set; } = 1;

        /// <summary>
        /// Время начала текущей сессии
        /// </summary>
        public DateTime? CurrentSessionStarted { get; set; }

        /// <summary>
        /// Дополнительные метаданные в JSON формате
        /// </summary>
        public string? MetadataJson { get; set; }
    }
} 