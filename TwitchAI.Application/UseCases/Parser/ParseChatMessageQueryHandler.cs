using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Common.Packages.Logger.Services.Interfaces;
using Common.Packages.Response.Behaviors;
using Common.Packages.Response.Exceptions;
using MediatR;
using TwitchAI.Application.Interfaces;
using TwitchAI.Application.UseCases.OpenAi;
using TwitchAI.Application.UseCases.Songs;
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

        if (txt.StartsWith('!'))
        {
            return new SoundChatCommand(txt.Trim().Replace(" ",""));
        }

        return default;
    }
}