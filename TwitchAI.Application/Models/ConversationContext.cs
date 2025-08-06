namespace TwitchAI.Application.Models
{
    /// <summary>
    /// Контекст для передачи информации о ConversationMessage в рамках запроса
    /// </summary>
    public static class ConversationContext
    {
        private static readonly AsyncLocal<Guid?> _conversationMessageId = new();
        
        /// <summary>
        /// ID текущего ConversationMessage в контексте запроса
        /// </summary>
        public static Guid? ConversationMessageId
        {
            get => _conversationMessageId.Value;
            set => _conversationMessageId.Value = value;
        }
    }
}