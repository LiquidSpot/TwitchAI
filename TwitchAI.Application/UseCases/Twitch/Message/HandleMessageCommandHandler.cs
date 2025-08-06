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
using TwitchAI.Application.UseCases.Viewers;
using TwitchAI.Application.UseCases.Holidays;
using TwitchAI.Application.UseCases.Translation;
using TwitchAI.Application.UseCases.Facts;
using TwitchAI.Application.UseCases.Compliment;
using TwitchAI.Domain.Entites;
using TwitchAI.Domain.Enums.ErrorCodes;

namespace TwitchAI.Application.UseCases.Twitch.Message;

internal class HandleMessageCommandHandler : ICommandHandler<HandleMessageCommand, LSResponse<ChatMessageDto>>
{
    private readonly IMediator _mediator;
    private readonly IExternalLogger<HandleMessageCommandHandler> _logger;
    public readonly ITwitchUserService _twitchUserService;
    private readonly ITwitchIntegrationService _twitch;
    private readonly IGreetingService _greetingService;
    private readonly IViewerMonitoringService _viewerMonitoringService;

    public HandleMessageCommandHandler(IMediator mediator, 
        IExternalLogger<HandleMessageCommandHandler> logger, 
        ITwitchUserService twitchUserService, 
        ITwitchIntegrationService twitch,
        IGreetingService greetingService,
        IViewerMonitoringService viewerMonitoringService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _twitch = twitch ?? throw new ArgumentNullException(nameof(twitch));
        _twitchUserService = twitchUserService ?? throw new ArgumentNullException(nameof(twitchUserService));
        _greetingService = greetingService ?? throw new ArgumentNullException(nameof(greetingService));
        _viewerMonitoringService = viewerMonitoringService ?? throw new ArgumentNullException(nameof(viewerMonitoringService));
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

        // Используем новый метод с информацией о создании пользователя
        var (user, wasUserCreated) = await _twitchUserService.GetOrCreateUserWithStatusAsync(request.message, cancellationToken).ConfigureAwait(false);
        response.Result.TwitchUser = user;
        
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

        // Проверяем, нужно ли поздороваться с новым пользователем
        if (_greetingService.ShouldGreetUser(response.Result.TwitchUser, wasUserCreated))
        {
            var greeting = _greetingService.GenerateGreeting(response.Result.TwitchUser);
            
            _logger.LogInformation(new { 
                Method = nameof(Handle),
                Status = "NewUserGreeting",
                UserId = response.Result.TwitchUser.Id,
                Username = response.Result.TwitchUser.UserName,
                Greeting = greeting
            });
            
            // Устанавливаем приветствие как сообщение для отправки
            response.Result.Message = greeting;
        }
        
        // Сохраняем сообщение из чата в базу данных
        var chatMessage = await _twitchUserService.AddMessage(response.Result.TwitchUser, request.message, cancellationToken).ConfigureAwait(false);

        // Отмечаем пользователя как активного (написавшего сообщение)
        try
        {
            await _viewerMonitoringService.MarkViewerAsActiveAsync(response.Result.TwitchUser.UserName, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Логируем ошибку, но не прерываем обработку сообщения
            _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                Method = nameof(Handle),
                Status = "Error marking viewer as active",
                Username = response.Result.TwitchUser.UserName,
                Error = ex.GetType().Name,
                Message = ex.Message
            });
        }

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
                            // Если у нас уже есть приветствие, объединяем его с ответом AI
                            if (!string.IsNullOrEmpty(response.Result.Message))
                            {
                                response.Result.Message += " " + cmdResponse.Result;
                            }
                            else
                            {
                                response.Result.Message = cmdResponse.Result;
                            }
                            
                            // Передаем ConversationMessageId для последующей связи с ChatMessage бота
                            response.Result.ConversationMessageId = ConversationContext.ConversationMessageId;
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
                            
                            // Если у нас есть приветствие, но команда AI не удалась, все равно отправляем приветствие
                            if (string.IsNullOrEmpty(response.Result.Message))
                            {
                                response.Result.Message = "❌ Произошла ошибка при обработке команды. Попробуйте позже.";
                            }
                        }
                        
                        break;
                    }
                case ChangeRoleCommand changeRoleCmd:
                    {
                        var cmdResponse = await _mediator.Send(changeRoleCmd, cancellationToken);
                        
                        if (cmdResponse.Status == Common.Packages.Response.Enums.ResponseStatus.Success)
                        {
                            // Если у нас уже есть приветствие, объединяем его с ответом команды
                            if (!string.IsNullOrEmpty(response.Result.Message))
                            {
                                response.Result.Message += " " + cmdResponse.Result;
                            }
                            else
                            {
                                response.Result.Message = cmdResponse.Result;
                            }
                        }
                        else
                        {
                            _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                                Method = nameof(Handle),
                                Status = "Error",
                                ErrorCode = cmdResponse.ErrorCode,
                                Message = cmdResponse.ErrorObjects,
                                Command = nameof(ChangeRoleCommand)
                            });
                            
                            // Если у нас есть приветствие, но команда смены роли не удалась, все равно отправляем приветствие
                            if (string.IsNullOrEmpty(response.Result.Message))
                            {
                                response.Result.Message = "❌ Произошла ошибка при смене роли. Попробуйте позже.";
                            }
                        }
                        
                        break;
                    }
                case ReplyLimitCommand replyLimitCmd:
                    {
                        var cmdResponse = await _mediator.Send(replyLimitCmd, cancellationToken);
                        
                        if (cmdResponse.Status == Common.Packages.Response.Enums.ResponseStatus.Success)
                        {
                            // Если у нас уже есть приветствие, объединяем его с ответом команды
                            if (!string.IsNullOrEmpty(response.Result.Message))
                            {
                                response.Result.Message += " " + cmdResponse.Result;
                            }
                            else
                            {
                                response.Result.Message = cmdResponse.Result;
                            }
                        }
                        else
                        {
                            _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                                Method = nameof(Handle),
                                Status = "Error",
                                ErrorCode = cmdResponse.ErrorCode,
                                Message = cmdResponse.ErrorObjects,
                                Command = nameof(ReplyLimitCommand)
                            });
                            
                            // Если у нас есть приветствие, но команда не удалась, все равно отправляем приветствие
                            if (string.IsNullOrEmpty(response.Result.Message))
                            {
                                response.Result.Message = "❌ Произошла ошибка при установке лимита reply. Попробуйте позже.";
                            }
                        }
                        
                        break;
                    }
                case EngineCommand engineCmd:
                    {
                        var cmdResponse = await _mediator.Send(engineCmd, cancellationToken);
                        
                        if (cmdResponse.Status == Common.Packages.Response.Enums.ResponseStatus.Success)
                        {
                            // Если у нас уже есть приветствие, объединяем его с ответом команды
                            if (!string.IsNullOrEmpty(response.Result.Message))
                            {
                                response.Result.Message += " " + cmdResponse.Result;
                            }
                            else
                            {
                                response.Result.Message = cmdResponse.Result;
                            }
                        }
                        else
                        {
                            _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                                Method = nameof(Handle),
                                Status = "Error",
                                ErrorCode = cmdResponse.ErrorCode,
                                Message = cmdResponse.ErrorObjects,
                                Command = nameof(EngineCommand)
                            });
                            
                            // Если у нас есть приветствие, но команда не удалась, все равно отправляем приветствие
                            if (string.IsNullOrEmpty(response.Result.Message))
                            {
                                response.Result.Message = "❌ Произошла ошибка при смене движка. Попробуйте позже.";
                            }
                        }
                        
                        break;
                    }
                case SoundChatCommand soundCmd:
                    {
                        var cmdResponse = await _mediator.Send(soundCmd, cancellationToken);
                        
                        // Если у нас уже есть приветствие, объединяем его с ответом команды
                        if (!string.IsNullOrEmpty(response.Result.Message))
                        {
                            response.Result.Message += " " + cmdResponse.Result;
                        }
                        else
                        {
                            response.Result.Message = cmdResponse.Result;
                        }
                        break;
                    }
                case ViewerStatsCommand viewerStatsCmd:
                    {
                        var cmdResponse = await _mediator.Send(viewerStatsCmd, cancellationToken);
                        
                        if (cmdResponse.Status == Common.Packages.Response.Enums.ResponseStatus.Success)
                        {
                            // Если у нас уже есть приветствие, объединяем его с ответом команды
                            if (!string.IsNullOrEmpty(response.Result.Message))
                            {
                                response.Result.Message += " " + cmdResponse.Result;
                            }
                            else
                            {
                                response.Result.Message = cmdResponse.Result;
                            }
                        }
                        else
                        {
                            _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                                Method = nameof(Handle),
                                Status = "Error",
                                ErrorCode = cmdResponse.ErrorCode,
                                Message = cmdResponse.ErrorObjects,
                                Command = nameof(ViewerStatsCommand)
                            });
                            
                            // Если у нас есть приветствие, но команда статистики не удалась, все равно отправляем приветствие
                            if (string.IsNullOrEmpty(response.Result.Message))
                            {
                                response.Result.Message = "❌ Произошла ошибка при получении статистики зрителей.";
                            }
                        }
                        
                        break;
                    }
                case HolidayCommand holidayCmd:
                    {
                        var cmdResponse = await _mediator.Send(holidayCmd, cancellationToken);
                        
                        if (cmdResponse.Status == Common.Packages.Response.Enums.ResponseStatus.Success)
                        {
                            // Если у нас уже есть приветствие, объединяем его с ответом команды
                            if (!string.IsNullOrEmpty(response.Result.Message))
                            {
                                response.Result.Message += " " + cmdResponse.Result;
                            }
                            else
                            {
                                response.Result.Message = cmdResponse.Result;
                            }
                        }
                        else
                        {
                            _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                                Method = nameof(Handle),
                                Status = "Error",
                                ErrorCode = cmdResponse.ErrorCode,
                                Message = cmdResponse.ErrorObjects,
                                Command = nameof(HolidayCommand)
                            });
                            
                            // Если у нас есть приветствие, но команда праздника не удалась, все равно отправляем приветствие
                            if (string.IsNullOrEmpty(response.Result.Message))
                            {
                                response.Result.Message = "❌ Произошла ошибка при получении праздника дня.";
                            }
                        }
                        
                        break;
                    }
                case TranslateCommand translateCmd:
                    {
                        var cmdResponse = await _mediator.Send(translateCmd, cancellationToken);
                        
                        if (cmdResponse.Status == Common.Packages.Response.Enums.ResponseStatus.Success)
                        {
                            // Если у нас уже есть приветствие, объединяем его с ответом команды
                            if (!string.IsNullOrEmpty(response.Result.Message))
                            {
                                response.Result.Message += " " + cmdResponse.Result;
                            }
                            else
                            {
                                response.Result.Message = cmdResponse.Result;
                            }
                        }
                        else
                        {
                            _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                                Method = nameof(Handle),
                                Status = "Error",
                                ErrorCode = cmdResponse.ErrorCode,
                                Message = cmdResponse.ErrorObjects,
                                Command = nameof(TranslateCommand),
                                Language = translateCmd.Language,
                                OriginalMessage = translateCmd.Message
                            });
                            
                            // Если у нас есть приветствие, но команда перевода не удалась, все равно отправляем приветствие
                            if (string.IsNullOrEmpty(response.Result.Message))
                            {
                                response.Result.Message = "❌ Произошла ошибка при переводе сообщения.";
                            }
                        }
                        
                        break;
                    }
                case FactCommand factCmd:
                    {
                        var cmdResponse = await _mediator.Send(factCmd, cancellationToken);
                        
                        if (cmdResponse.Status == Common.Packages.Response.Enums.ResponseStatus.Success)
                        {
                            // Если у нас уже есть приветствие, объединяем его с ответом команды
                            if (!string.IsNullOrEmpty(response.Result.Message))
                            {
                                response.Result.Message += " " + cmdResponse.Result;
                            }
                            else
                            {
                                response.Result.Message = cmdResponse.Result;
                            }
                        }
                        else
                        {
                            _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                                Method = nameof(Handle),
                                Status = "Error",
                                ErrorCode = cmdResponse.ErrorCode,
                                Message = cmdResponse.ErrorObjects,
                                Command = nameof(FactCommand)
                            });
                            
                            // Если у нас есть приветствие, но команда фактов не удалась, все равно отправляем приветствие
                            if (string.IsNullOrEmpty(response.Result.Message))
                            {
                                response.Result.Message = "❌ Произошла ошибка при получении факта.";
                            }
                        }
                        
                        break;
                    }
                case ComplimentCommand complimentCmd:
                    {
                        var cmdResponse = await _mediator.Send(complimentCmd, cancellationToken);
                        
                        if (cmdResponse.Status == Common.Packages.Response.Enums.ResponseStatus.Success)
                        {
                            // Если у нас уже есть приветствие, объединяем его с ответом команды
                            if (!string.IsNullOrEmpty(response.Result.Message))
                            {
                                response.Result.Message += " " + cmdResponse.Result;
                            }
                            else
                            {
                                response.Result.Message = cmdResponse.Result;
                            }
                        }
                        else
                        {
                            _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                                Method = nameof(Handle),
                                Status = "Error",
                                ErrorCode = cmdResponse.ErrorCode,
                                Message = cmdResponse.ErrorObjects,
                                Command = nameof(ComplimentCommand)
                            });
                            
                            // Если у нас есть приветствие, но команда комплимента не удалась, все равно отправляем приветствие
                            if (string.IsNullOrEmpty(response.Result.Message))
                            {
                                response.Result.Message = "❌ Произошла ошибка при генерации комплимента.";
                            }
                        }
                        
                        break;
                    }

                default:
                    {
                        _logger.LogInformation(new { 
                            Method = nameof(Handle),
                            Status = "UnknownCommand",
                            CommandType = chatCmd.GetType().Name
                        });
                        
                        // Если у нас есть приветствие, но команда неизвестна, все равно отправляем приветствие
                        if (string.IsNullOrEmpty(response.Result.Message))
                        {
                            response.Result.Message = "❓ Неизвестная команда.";
                        }
                        break;
                    }
            }
        }
        else
        {
            _logger.LogInformation(new { 
                Method = nameof(Handle),
                Status = "NoCommand",
                Message = "Message was not recognized as a command",
                HasGreeting = !string.IsNullOrEmpty(response.Result.Message)
            });
            
            // Если команда не распознана, но есть приветствие - отправляем его
            // Если команды и приветствия нет - не отправляем ничего
            if (string.IsNullOrEmpty(response.Result.Message))
            {
                response.Result.Message = null;
            }
        }

        return response.Success();
    }
}