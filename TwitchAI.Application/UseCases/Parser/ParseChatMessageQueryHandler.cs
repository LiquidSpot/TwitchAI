﻿using System.Diagnostics.CodeAnalysis;
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
using TwitchAI.Application.UseCases.Compliment;
using TwitchAI.Domain.Enums.ErrorCodes;
using TwitchAI.Domain.Entites;
using Microsoft.Extensions.Options;
using TwitchAI.Application.Models;

namespace TwitchAI.Application.UseCases.Parser;

internal class ParseChatMessageQueryHandler : IQueryHandler<ParseChatMessageQuery, IChatCommand?>
{
    private readonly IMediator _mediator;
    private readonly IExternalLogger<ParseChatMessageQueryHandler> _logger;
    private readonly IUserMessageParser _parser;
    private readonly IBotRoleService _botRoleService;
    private readonly AppConfiguration _appConfig;

    public ParseChatMessageQueryHandler(
        IMediator mediator, 
        IExternalLogger<ParseChatMessageQueryHandler> logger, 
        IUserMessageParser parser,
        IBotRoleService botRoleService,
        IOptions<AppConfiguration> appConfig)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _botRoleService = botRoleService ?? throw new ArgumentNullException(nameof(botRoleService));
        _appConfig = appConfig?.Value ?? throw new ArgumentNullException(nameof(appConfig));
    }

    [SuppressMessage("ReSharper", "AsyncConverter.ConfigureAwaitHighlighting")]
    public async Task<IChatCommand?> Handle(ParseChatMessageQuery query, CancellationToken cancellation)
    {
        var txt = query.RawMessage?.Message.Trim();
        if (string.IsNullOrWhiteSpace(txt)) throw new LSException(BaseErrorCodes.DataNotFound);

        // Проверяем, является ли это reply сообщением (нужно извлекать из RawIrcMessage)
        string? replyParentMessageId = ExtractReplyParentMessageId(query.RawMessage);
        if (!string.IsNullOrEmpty(replyParentMessageId))
        {
            _logger.LogInformation(new { 
                Method = nameof(Handle),
                Status = "ReplyDetected",
                UserId = query.userId,
                ReplyParentMessageId = replyParentMessageId,
                Message = txt
            });

            // Создаем UserMessage для reply
            var userMessage = new UserMessage 
            { 
                message = txt,
                role = _botRoleService.GetCurrentRole(),
                temp = _appConfig.OpenAi.Temperature,
                maxToken = _appConfig.OpenAi.MaxTokens
            };

            // Создаем AiChatCommand для обработки reply с контекстом
            // chatMessageId будет установлен позже в HandleMessageCommandHandler
            return new AiChatCommand(userMessage, query.userId, null);
        }

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

            // Команда комплиментов
            if (txt.StartsWith("!комплимент", StringComparison.OrdinalIgnoreCase) ||
                txt.StartsWith("!compliment", StringComparison.OrdinalIgnoreCase))
            {
                var parts = txt.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                string? targetUsername = null;
                
                // Проверяем, есть ли указанный пользователь
                if (parts.Length > 1)
                {
                    targetUsername = parts[1].Trim();
                }
                
                return new ComplimentCommand(query.userId, targetUsername);
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

    /// <summary>
    /// Извлекает reply-parent-msg-id из IRC сообщения
    /// </summary>
    private static string? ExtractReplyParentMessageId(TwitchLib.Client.Models.ChatMessage? message)
    {
        if (message?.RawIrcMessage == null) return null;

        try
        {
            var rawMessage = message.RawIrcMessage;
            if (rawMessage.Contains("reply-parent-msg-id="))
            {
                var replyTagStart = rawMessage.IndexOf("reply-parent-msg-id=");
                if (replyTagStart != -1)
                {
                    var valueStart = replyTagStart + "reply-parent-msg-id=".Length;
                    var semicolonIndex = rawMessage.IndexOf(';', valueStart);
                    var spaceIndex = rawMessage.IndexOf(' ', valueStart);
                    
                    var valueEnd = Math.Min(
                        semicolonIndex == -1 ? int.MaxValue : semicolonIndex,
                        spaceIndex == -1 ? int.MaxValue : spaceIndex
                    );
                    
                    if (valueEnd != int.MaxValue && valueEnd > valueStart)
                    {
                        return rawMessage.Substring(valueStart, valueEnd - valueStart);
                    }
                }
            }
        }
        catch
        {
            // Если не удалось извлечь, возвращаем null
        }

        return null;
    }
}