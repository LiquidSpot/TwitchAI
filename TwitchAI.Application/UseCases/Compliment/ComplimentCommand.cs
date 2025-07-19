using TwitchAI.Application.Interfaces;

namespace TwitchAI.Application.UseCases.Compliment
{
    /// <summary>
    /// Команда для генерации комплиментов пользователям
    /// </summary>
    public record ComplimentCommand(
        Guid UserId,
        string? TargetUsername = null
    ) : IChatCommand;
} 