using TwitchAI.Domain.Entites;

namespace TwitchAI.Application.Dto.Request;

public class HandleChatDto
{
    public string Command{ get; set; }
    public TwitchUser User{ get; set; }
    public string[] Args { get; set; }
}