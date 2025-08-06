using TwitchAI.Domain.Entites;

namespace TwitchAI.Application.Dto.Response
{
    public class ChatMessageDto
    {
        public virtual TwitchUser? TwitchUser { get; set; }

        public string Message { get; set; } = null!;
        
        /// <summary>
        /// ID ConversationMessage для связи с ChatMessage бота (если есть)
        /// </summary>
        public Guid? ConversationMessageId { get; set; }
    }
}
