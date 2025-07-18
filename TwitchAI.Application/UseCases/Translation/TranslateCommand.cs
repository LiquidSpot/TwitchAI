using TwitchAI.Application.Interfaces;

namespace TwitchAI.Application.UseCases.Translation
{
    /// <summary>
    /// Команда для перевода сообщений на указанный язык
    /// </summary>
    public record TranslateCommand(
        string Language,
        string Message,
        Guid UserId
    ) : IChatCommand;
} 