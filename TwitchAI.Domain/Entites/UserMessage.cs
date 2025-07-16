using TwitchAI.Domain.Entites.Abstract;
using TwitchAI.Domain.Enums;

namespace TwitchAI.Domain.Entites
{
    [Serializable]
    public class UserMessage : Entity
    {
        public string message { get; set; } = string.Empty;

        public Role role { get; set; } = Role.bot;

        public double? temp { get; set; } = 0.3;

        public int? maxToken { get; set; } = 550;

        // Navigation properties
        public Guid TwitchUserId { get; set; }
        public virtual TwitchUser? TwitchUser { get; set; }
    }
}