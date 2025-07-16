using TwitchAI.Application.Interfaces;
using TwitchAI.Domain.Entites;

namespace TwitchAI.Application.UseCases.OpenAi;

public record class AiChatCommand(UserMessage Message, Guid userId, Guid? chatMessageId = null) : IChatCommand
{
}