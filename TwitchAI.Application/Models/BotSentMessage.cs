namespace TwitchAI.Application.Models
{
    /// <summary>
    /// Информация о сообщении, отправленном ботом
    /// </summary>
    public class BotSentMessage
    {
        /// <summary>
        /// Канал, в который было отправлено сообщение
        /// </summary>
        public string Channel { get; set; } = string.Empty;
        
        /// <summary>
        /// Текст сообщения
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}