using TwitchAI.Domain.Entites;

namespace TwitchAI.Application.Dto.Response
{
    public class ChatMessageDto
    {
        public virtual TwitchUser? TwitchUser { get; set; }

        public string Message { get; set; } = null!;
    }
}
