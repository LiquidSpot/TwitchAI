using Common.Packages.Logger.Services.Interfaces;
using Common.Packages.Response.Behaviors;
using Common.Packages.Response.Enums;
using Common.Packages.Response.Models;

using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Serilog.Context;

using TwitchAI.Application.Dto.Response;
using TwitchAI.Application.Interfaces;
using TwitchAI.Application.Models;
using TwitchAI.Application.UseCases.OpenAi;
using TwitchAI.Application.UseCases.Parser;
using TwitchAI.Application.UseCases.Songs;
using TwitchAI.Domain.Entites;
using TwitchAI.Domain.Enums.ErrorCodes;

namespace TwitchAI.Application.UseCases.Twitch.Message;

internal class HandleMessageCommandHandler : ICommandHandler<HandleMessageCommand, LSResponse<ChatMessageDto>>
{
    private readonly IMediator _mediator;
    private readonly IExternalLogger<HandleMessageCommandHandler> _logger;
    public readonly ITwitchUserService _twitchUserService;
    private readonly ITwitchIntegrationService _twitch;

    public HandleMessageCommandHandler(IMediator mediator, 
        IExternalLogger<HandleMessageCommandHandler> logger, 
        ITwitchUserService twitchUserService, 
        ITwitchIntegrationService twitch)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _twitch = twitch ?? throw new ArgumentNullException(nameof(twitch));
        _twitchUserService = twitchUserService ?? throw new ArgumentNullException(nameof(twitchUserService));
    }

    public async Task<LSResponse<ChatMessageDto>> Handle(HandleMessageCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(request);
        var response = new LSResponse<ChatMessageDto>();
        response.Result = new ChatMessageDto(); // Инициализируем Result

        if (string.IsNullOrWhiteSpace(request.message.UserId) || string.IsNullOrWhiteSpace(request.message.Username))
        {
            _logger.LogError((int)BaseErrorCodes.DataNotFound, new { 
                Method = nameof(Handle),
                Status = "Error",
                Message = "UserId or Username is empty",
                UserId = request.message.UserId,
                Username = request.message.Username
            });
            
            response.Result.Message = "❌ Ошибка: не удалось определить пользователя.";
            return response.Success(); // Возвращаем успех, чтобы сообщение об ошибке отправилось в чат
        }

        response.Result.TwitchUser = await _twitchUserService.GetOrCreateUserAsync(request.message, cancellationToken).ConfigureAwait(false);
        
        if (response.Result.TwitchUser == null)
        {
            _logger.LogError((int)BaseErrorCodes.DataNotFound, new { 
                Method = nameof(Handle),
                Status = "Error",
                Message = "Failed to get or create user",
                UserId = request.message.UserId,
                Username = request.message.Username
            });
            
            response.Result.Message = "❌ Ошибка: не удалось обработать пользователя.";
            return response.Success(); // Возвращаем успех, чтобы сообщение об ошибке отправилось в чат
        }
        
        // Сохраняем сообщение из чата в базу данных
        var chatMessage = await _twitchUserService.AddMessage(response.Result.TwitchUser, request.message, cancellationToken).ConfigureAwait(false);

        var chatCmd = await _mediator.Send(new ParseChatMessageQuery(request.message, response.Result.TwitchUser.Id), cancellationToken);

        if (chatCmd != null)
        {
            switch (chatCmd)
            {
                case AiChatCommand aiCmd:
                    {
                        // Обновляем команду с информацией о ChatMessage для последующего сохранения в контекст
                        var updatedAiCmd = aiCmd with { chatMessageId = chatMessage.Id };

                        var cmdResponse = await _mediator.Send(updatedAiCmd, cancellationToken);
                        
                        if (cmdResponse.Status == Common.Packages.Response.Enums.ResponseStatus.Success)
                        {
                            response.Result.Message = cmdResponse.Result;
                        }
                        else
                        {
                            _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                                Method = nameof(Handle),
                                Status = "Error",
                                ErrorCode = cmdResponse.ErrorCode,
                                Message = cmdResponse.ErrorObjects,
                                Command = nameof(AiChatCommand)
                            });
                            
                            response.Result.Message = "❌ Произошла ошибка при обработке команды. Попробуйте позже.";
                        }
                        
                        break;
                    }
                case SoundChatCommand soundCmd:
                    {
                        var cmdResponse = await _mediator.Send(soundCmd, cancellationToken);
                        response.Result.Message = cmdResponse.Result;
                        break;
                    }
                default:
                    {
                        _logger.LogInformation(new { 
                            Method = nameof(Handle),
                            Status = "UnknownCommand",
                            CommandType = chatCmd.GetType().Name
                        });
                        
                        response.Result.Message = "❓ Неизвестная команда.";
                        break;
                    }
            }
        }
        else
        {
            _logger.LogInformation(new { 
                Method = nameof(Handle),
                Status = "NoCommand",
                Message = "Message was not recognized as a command"
            });
            
            // Если команда не распознана, не отправляем сообщение в чат
            response.Result.Message = null;
        }

        return response.Success();
    }
}