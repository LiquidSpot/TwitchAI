using TwitchAI.Application.Interfaces;

namespace TwitchAI.Application.UseCases.OpenAi;

public record ReplyLimitCommand(int Limit, Guid UserId) : IChatCommand;