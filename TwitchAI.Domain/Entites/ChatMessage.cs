using TwitchAI.Domain.Entites.Abstract;

namespace TwitchAI.Domain.Entites
{
    public class ChatMessage : Entity
    {
        /*------------- связь с пользователем --------------------*/
        public Guid TwitchUserId { get; set; }
        public virtual TwitchUser TwitchUser { get; set; } = null!;

        /*------------- «паспорт» сообщения ----------------------*/
        /// <summary>Уникальный <c>id</c>, который TMI выдаёт каждому сообщению.</summary>
        public string MessageId { get; set; } = null!;      // tag  id
        /// <summary>Канал, куда пришло сообщение (lower-case, без '#').</summary>
        public string Channel { get; set; } = null!;
        public string RoomId { get; set; } = null!;      // tag room-id

        /*------------- reply functionality ----------------------*/
        /// <summary>Является ли это сообщение ответом на другое сообщение</summary>
        public bool IsReply { get; set; }
        /// <summary>ID сообщения, на которое отвечают (reply-parent-msg-id)</summary>
        public string? ReplyParentMessageId { get; set; }

        /*------------- контент ----------------------------------*/
        public string Text { get; set; } = null!;
        /// <summary>Текст после подстановки emotes (может быть <c>null</c>).</summary>
        public string? EmoteReplacedText { get; set; }

        /*------------- денежные события -------------------------*/
        public int Bits { get; set; }                // tag bits
        public double BitsUsd { get; set; }

        /*------------- флаги и состояние на момент сообщения ----*/
        public bool IsFirstMessage { get; set; }           // tag first-msg
        public bool IsHighlighted { get; set; }           // tag msg-id == highlighted-…
        public bool IsMeAction { get; set; }           // /me
        public bool IsSkippingSubMode { get; set; }           // tag msg-id == skip-subs-…
        public bool IsModerator { get; set; }           // tag mod
        public bool IsSubscriber { get; set; }           // tag subscriber
        public bool IsBroadcaster { get; set; }           // user == channel
        public bool IsTurbo { get; set; }           // tag turbo

        /*------------- свободное поле — все исходные тэги (json) */
        public string RawTagsJson { get; set; } = null!;

        /*------------- время, сгенерированное TMI ---------------*/
        public string TmiSentTs { get; set; } = null!;  // tag tmi-sent-ts

        /*------------- навигация ------------------------------ */
        /// <summary>
        /// Связанное сообщение из истории диалога (если есть)
        /// </summary>
        public virtual ICollection<ConversationMessage> ConversationMessages { get; set; }
            = new List<ConversationMessage>();
    }
}
