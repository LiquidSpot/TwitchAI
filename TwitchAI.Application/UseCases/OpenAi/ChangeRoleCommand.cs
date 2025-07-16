using TwitchAI.Application.Interfaces;
using TwitchAI.Domain.Enums;

namespace TwitchAI.Application.UseCases.OpenAi
{
    public record ChangeRoleCommand(string RoleName, Guid UserId) : IChatCommand;
} 