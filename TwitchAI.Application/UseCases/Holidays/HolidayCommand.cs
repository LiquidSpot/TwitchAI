using TwitchAI.Application.Interfaces;

namespace TwitchAI.Application.UseCases.Holidays
{
    /// <summary>
    /// Команда для получения праздника дня
    /// </summary>
    public record HolidayCommand(Guid UserId, DateTime? Date = null) : IChatCommand;
} 