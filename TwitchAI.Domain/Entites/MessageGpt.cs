using TwitchAI.Domain.Entites.Abstract;

namespace TwitchAI.Domain.Entites
{
    /// <summary>
    /// message for GPT
    /// </summary>
    public class MessageGpt : Entity
    {
        public string? role { get; set; }

        public string? content { get; set; }
    }
}
