using TwitchAI.Application.Interfaces;

namespace TwitchAI.Application.UseCases.Facts
{
    /// <summary>
    /// Команда для получения случайного факта
    /// </summary>
    public record FactCommand(Guid UserId) : IChatCommand;
} 