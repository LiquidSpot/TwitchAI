using AutoMapper;
using Common.Packages.Logger.Services.Interfaces;
using Common.Packages.Response.Behaviors;
using Common.Packages.Response.Enums;
using Common.Packages.Response.Models;

using TwitchAI.Application.Interfaces;
using TwitchAI.Application.Interfaces.Infrastructure;
using TwitchAI.Domain.Entites;
using TwitchAI.Domain.Enums.ErrorCodes;

namespace TwitchAI.Application.UseCases.OpenAi
{
    internal class AiChatCommandHandler : ICommandHandler<AiChatCommand, LSResponse<string>>
    {
        private readonly IOpenAiService _aiService;
        private readonly ITwitchUserService _twitchUserService;
        private readonly IReplyLimitService _replyLimitService;
        private readonly IExternalLogger<AiChatCommandHandler> _logger;

        public AiChatCommandHandler(
            IOpenAiService aiService,
            ITwitchUserService twitchUserService,
            IReplyLimitService replyLimitService,
            IExternalLogger<AiChatCommandHandler> logger)
        {
            _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
            _twitchUserService = twitchUserService ?? throw new ArgumentNullException(nameof(twitchUserService));
            _replyLimitService = replyLimitService ?? throw new ArgumentNullException(nameof(replyLimitService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<LSResponse<string>> Handle(AiChatCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(new { 
                Method = nameof(Handle),
                UserId = request.userId,
                Message = request.Message.message,
                Role = request.Message.role,
                ChatMessageId = request.chatMessageId
            });

            try
            {
                // Получаем пользователя для сохранения в контекст
                var user = await _twitchUserService.GetUserByIdAsync(request.userId, cancellationToken);
                if (user == null)
                {
                    _logger.LogError((int)BaseErrorCodes.DataNotFound, new { 
                        Method = nameof(Handle),
                        Status = "Error",
                        Message = "User not found",
                        UserId = request.userId
                    });

                    return new LSResponse<string>().Error(BaseErrorCodes.DataNotFound, "Пользователь не найден.");
                }

                // Определяем, какой контекст использовать
                List<ConversationMessage> conversationContext;
                
                // Если есть ChatMessageId, проверяем, является ли это reply на сообщение бота
                if (request.chatMessageId.HasValue)
                {
                    var chatMessage = await _twitchUserService.GetChatMessageByIdAsync(request.chatMessageId.Value, cancellationToken);
                    if (chatMessage?.IsReply == true && !string.IsNullOrEmpty(chatMessage.ReplyParentMessageId))
                    {
                        // Если это reply сообщение и команда дошла сюда, значит это reply на сообщение бота
                        // (проверка уже выполнена в ParseChatMessageQueryHandler)
                        // Получаем персональный лимит пользователя для reply-цепочки
                        var replyLimit = await _replyLimitService.GetReplyLimitAsync(user.Id, cancellationToken);
                        
                        // Используем reply-цепочку для контекста с персональным лимитом
                        conversationContext = await _twitchUserService.GetReplyChainContextAsync(
                            chatMessage.ReplyParentMessageId, 
                            user.Id, 
                            limit: replyLimit, 
                            cancellationToken);
                            
                        _logger.LogInformation(new { 
                            Method = nameof(Handle),
                            Status = "UsingReplyContext",
                            UserId = request.userId,
                            ReplyParentMessageId = chatMessage.ReplyParentMessageId,
                            ReplyLimit = replyLimit,
                            ContextMessagesCount = conversationContext.Count
                        });
                    }
                    else
                    {
                        // Обычный контекст диалога пользователя
                        conversationContext = await _twitchUserService.GetUserConversationContextAsync(user.Id, limit: 3, cancellationToken);
                    }
                }
                else
                {
                    // Получаем обычный контекст диалога пользователя (последние 3 сообщения)
                    conversationContext = await _twitchUserService.GetUserConversationContextAsync(user.Id, limit: 3, cancellationToken);
                }

                // Сохраняем сообщение пользователя в контекст диалога перед отправкой в OpenAI
                await _twitchUserService.AddUserMessageToContextAsync(
                    user, 
                    request.Message.message, 
                    request.chatMessageId, 
                    cancellationToken);

                // Используем универсальный метод с контекстом
                var response = await _aiService.GenerateUniversalWithContextAsync(request.Message, conversationContext, ct: cancellationToken);

                if (response.Status == ResponseStatus.Success && !string.IsNullOrWhiteSpace(response.Result))
                {
                    // Сохраняем ответ GPT в контекст диалога
                    await _twitchUserService.AddGptResponseToContextAsync(
                        user, 
                        response.Result, 
                        modelName: "o4-mini-2025-04-16", // Текущая модель из сервиса
                        cancellationToken: cancellationToken);

                    _logger.LogInformation(new { 
                        Method = nameof(Handle),
                        Status = "Success",
                        UserId = request.userId,
                        Content = response.Result
                    });
                }
                else
                {
                    _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                        Method = nameof(Handle),
                        Status = "Error",
                        UserId = request.userId,
                        ErrorCode = response.ErrorCode,
                        Message = response.ErrorObjects
                    });
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                    Method = nameof(Handle),
                    Status = "Exception",
                    UserId = request.userId,
                    Error = ex.GetType().Name,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace
                });

                return new LSResponse<string>().Error(BaseErrorCodes.OperationProcessError, "Произошла ошибка при обработке запроса.");
            }
        }
    }
}
