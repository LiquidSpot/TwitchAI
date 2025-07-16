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

    public ParseChatMessageQueryHandler(IMediator mediator, IExternalLogger<ParseChatMessageQueryHandler> logger, IUserMessageParser parser)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
    }

    [SuppressMessage("ReSharper", "AsyncConverter.ConfigureAwaitHighlighting")]
    public async Task<IChatCommand?> Handle(ParseChatMessageQuery query, CancellationToken cancellation)
    {
        var txt = query.RawMessage?.Message.Trim();
        if (string.IsNullOrWhiteSpace(txt)) throw new LSException(BaseErrorCodes.DataNotFound);

        if (txt.StartsWith("!ai", StringComparison.OrdinalIgnoreCase) ||
            txt.StartsWith("!аи", StringComparison.OrdinalIgnoreCase))
        {
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