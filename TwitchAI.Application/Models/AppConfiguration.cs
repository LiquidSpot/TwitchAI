using TwitchAI.Domain.Enums;

namespace TwitchAI.Application.Models
{
    public class AppConfiguration
    {
        public bool MigrateDb { get; set; }
        public string SoundAlerts { get; set; }
        public int CooldownSecounds { get; set; }
        
        /// <summary>
        /// Версия OpenAI API для использования (по умолчанию - новый Responses API)
        /// </summary>
        public OpenAiApiVersion OpenAiApiVersion { get; set; } = OpenAiApiVersion.Responses;
        
        /// <summary>
        /// Настройки OpenAI
        /// </summary>
        public OpenAiConfiguration OpenAi { get; set; } = new();
    }
}
