using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Common.Packages.Logger.Services.Interfaces;
using Common.Packages.Response.Behaviors;
using Common.Packages.Response.Exceptions;
using MediatR;
using TwitchAI.Application.Interfaces;
using TwitchAI.Application.UseCases.OpenAi;
using TwitchAI.Application.UseCases.Songs;
using TwitchAI.Application.UseCases.Viewers;
using TwitchAI.Application.UseCases.Holidays;
using TwitchAI.Application.UseCases.Translation;
using TwitchAI.Application.UseCases.Facts;
using TwitchAI.Domain.Enums.ErrorCodes;

namespace TwitchAI.Application.UseCases.Parser;

internal class ParseChatMessageQueryHandler : IQueryHandler<ParseChatMessageQuery, IChatCommand?>
{
    private readonly IMediator _mediator;
    private readonly IExternalLogger<ParseChatMessageQueryHandler> _logger;
    private readonly IUserMessageParser _parser;
    private readonly IBotRoleService _botRoleService;

    public ParseChatMessageQueryHandler(
        IMediator mediator, 
        IExternalLogger<ParseChatMessageQueryHandler> logger, 
        IUserMessageParser parser,
        IBotRoleService botRoleService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _botRoleService = botRoleService ?? throw new ArgumentNullException(nameof(botRoleService));
    }

    [SuppressMessage("ReSharper", "AsyncConverter.ConfigureAwaitHighlighting")]
    public async Task<IChatCommand?> Handle(ParseChatMessageQuery query, CancellationToken cancellation)
    {
        var txt = query.RawMessage?.Message.Trim();
        if (string.IsNullOrWhiteSpace(txt)) throw new LSException(BaseErrorCodes.DataNotFound);

        // Проверяем команду смены роли: !ai rolename (без дополнительного текста)
        if (txt.StartsWith("!ai", StringComparison.OrdinalIgnoreCase) ||
            txt.StartsWith("!аи", StringComparison.OrdinalIgnoreCase) ||
            txt.StartsWith("!ии", StringComparison.OrdinalIgnoreCase) ||
            txt.StartsWith("!ii", StringComparison.OrdinalIgnoreCase))
        {
            var parts = txt.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
            
            // Команда смены роли: !ai rolename (без дополнительного текста)
            if (parts.Length == 2 && _botRoleService.TryGetRole(parts[1], out var _))
            {
                return new ChangeRoleCommand(parts[1], query.userId);
            }
            
            // Обычная команда AI с сообщением
            if (_parser.TryParse(txt, out var message))
            {
                return new AiChatCommand(message, query.userId);
            }

            return default;
        }

        // --- song / sound aliases handled the same way as before ---
        //if (txt.StartsWith("!song", StringComparison.OrdinalIgnoreCase) ||
        //    txt.StartsWith("!песня", StringComparison.OrdinalIgnoreCase) ||
        //    txt.StartsWith("!спотифай", StringComparison.OrdinalIgnoreCase))
        //{
        //    return Task.FromResult<IChatCommand?>(new SongChatCommand());
        //}

                    // Команда праздника дня
            if (txt.StartsWith("!праздник", StringComparison.OrdinalIgnoreCase) ||
                txt.StartsWith("!holiday", StringComparison.OrdinalIgnoreCase))
            {
                return new HolidayCommand(query.userId);
            }

            // Команда перевода
            if (txt.StartsWith("!ru ", StringComparison.OrdinalIgnoreCase) ||
                txt.StartsWith("!en ", StringComparison.OrdinalIgnoreCase) ||
                txt.StartsWith("!zh ", StringComparison.OrdinalIgnoreCase) ||
                txt.StartsWith("!ja ", StringComparison.OrdinalIgnoreCase) ||
                txt.StartsWith("!es ", StringComparison.OrdinalIgnoreCase))
            {
                var parts = txt.Split(' ', 2);
                if (parts.Length >= 2)
                {
                    var language = parts[0].Substring(1); // Убираем !
                    var message = parts[1];
                    return new TranslateCommand(language, message, query.userId);
                }
            }

            // Команда фактов
            if (txt.StartsWith("!факт", StringComparison.OrdinalIgnoreCase) ||
                txt.StartsWith("!fact", StringComparison.OrdinalIgnoreCase))
            {
                return new FactCommand(query.userId);
            }

        // Команды статистики зрителей
        if (txt.StartsWith("!viewers", StringComparison.OrdinalIgnoreCase) ||
            txt.StartsWith("!silent", StringComparison.OrdinalIgnoreCase) ||
            txt.StartsWith("!stats", StringComparison.OrdinalIgnoreCase))
        {
            var commandParts = txt.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var command = commandParts[0].Substring(1); // Убираем '!'
            
            if (ViewerStatsCommand.IsValidCommand(command))
            {
                return new ViewerStatsCommand(command, query.userId);
            }
        }

        if (txt.StartsWith('!'))
        {
            return new SoundChatCommand(txt.Trim().Replace(" ",""));
        }

        return default;
    }
}