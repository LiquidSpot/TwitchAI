using TwitchAI.Application.Interfaces;

namespace TwitchAI.Application.UseCases.Viewers
{
    /// <summary>
    /// Команда для получения статистики зрителей
    /// </summary>
    public record ViewerStatsCommand(string CommandType, Guid UserId) : IChatCommand
    {
        public static readonly string[] ValidCommands = { "viewers", "silent", "stats" };
        
        public static bool IsValidCommand(string command)
        {
            return ValidCommands.Contains(command.ToLower());
        }
    }
} 