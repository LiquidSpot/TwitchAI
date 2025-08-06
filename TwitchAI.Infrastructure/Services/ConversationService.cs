using Common.Packages.Logger.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using TwitchAI.Application.Interfaces;
using TwitchAI.Application.Interfaces.Infrastructure;
using TwitchAI.Domain.Entites;
using TwitchAI.Domain.Enums.ErrorCodes;

namespace TwitchAI.Infrastructure.Services;

internal class ConversationService : IConversationService
{
    private readonly IExternalLogger<ConversationService> _logger;
    private readonly IUnitOfWork _uow;
    private readonly IRepository<ConversationMessage, Guid> _repositoryConversation;
    private readonly IRepository<ChatMessage, Guid> _repositoryMsg;

    public ConversationService(IExternalLogger<ConversationService> logger, IUnitOfWork uow)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _repositoryConversation = _uow.Factory<ConversationMessage, Guid>() ?? throw new InvalidOperationException("Failed to create ConversationMessage repository");
        _repositoryMsg = _uow.Factory<ChatMessage, Guid>() ?? throw new InvalidOperationException("Failed to create ChatMessage repository");
    }

    public async Task<ConversationMessage> AddUserMessageToContextAsync(TwitchUser user, string message, Guid? chatMessageId = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(new { 
            Method = nameof(AddUserMessageToContextAsync),
            UserId = user.Id,
            UserName = user.UserName,
            Message = message,
            ChatMessageId = chatMessageId
        });

        var nextOrder = await GetNextMessageOrderAsync(user.Id, cancellationToken);
        
        var conversationMessage = new ConversationMessage
        {
            TwitchUserId = user.Id,
            Role = "user",
            Content = message,
            MessageOrder = nextOrder,
            ChatMessageId = chatMessageId,
            CreatedAt = DateTime.UtcNow
        };

        await _repositoryConversation.AddAsync(conversationMessage, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return conversationMessage;
    }

    public async Task<ConversationMessage> AddGptResponseToContextAsync(TwitchUser user, string gptResponse, string? openAiResponseId = null, string? modelName = null, int? tokenCount = null, Guid? chatMessageId = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(new { 
            Method = nameof(AddGptResponseToContextAsync),
            UserId = user.Id,
            UserName = user.UserName,
            GptResponse = gptResponse,
            OpenAiResponseId = openAiResponseId,
            ModelName = modelName,
            TokenCount = tokenCount,
            ChatMessageId = chatMessageId
        });

        var nextOrder = await GetNextMessageOrderAsync(user.Id, cancellationToken);
        
        var conversationMessage = new ConversationMessage
        {
            TwitchUserId = user.Id,
            Role = "assistant",
            Content = gptResponse,
            MessageOrder = nextOrder,
            OpenAiResponseId = openAiResponseId,
            OpenAiModel = modelName,
            TokenCount = tokenCount,
            ChatMessageId = chatMessageId,
            CreatedAt = DateTime.UtcNow
        };

        await _repositoryConversation.AddAsync(conversationMessage, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return conversationMessage;
    }

    public async Task<List<ConversationMessage>> GetUserConversationContextAsync(Guid userId, int limit = 3, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(new { 
            Method = nameof(GetUserConversationContextAsync),
            UserId = userId,
            Limit = limit
        });

        var messages = await _repositoryConversation.Query()
            .Where(m => m.TwitchUserId == userId)
            .OrderByDescending(m => m.MessageOrder)
            .Take(limit * 2) // Берем больше, чтобы получить полные пары user-assistant
            .OrderBy(m => m.MessageOrder)
            .ToListAsync(cancellationToken);

        _logger.LogInformation(new { 
            Method = nameof(GetUserConversationContextAsync),
            UserId = userId,
            MessagesFound = messages.Count,
            Messages = messages.Select(m => new { m.Role, m.Content, m.MessageOrder })
        });

        return messages;
    }

    public async Task<int> ClearUserConversationContextAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(new { 
            Method = nameof(ClearUserConversationContextAsync),
            UserId = userId
        });

        var messagesToDelete = await _repositoryConversation.Query()
            .Where(m => m.TwitchUserId == userId)
            .ToListAsync(cancellationToken);

        var count = messagesToDelete.Count;
        
        if (count > 0)
        {
            await _repositoryConversation.DeleteMany(messagesToDelete, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(new { 
            Method = nameof(ClearUserConversationContextAsync),
            UserId = userId,
            DeletedCount = count
        });

        return count;
    }

    public async Task<List<ConversationMessage>> GetReplyChainContextAsync(string replyParentMessageId, Guid userId, int limit = 3, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(new { 
            Method = nameof(GetReplyChainContextAsync),
            ReplyParentMessageId = replyParentMessageId,
            UserId = userId,
            Limit = limit
        });

        try
        {
            // Ищем ChatMessage по MessageId, на которое отвечают
            var parentChatMessage = await _repositoryMsg.Query()
                .FirstOrDefaultAsync(cm => cm.MessageId == replyParentMessageId, cancellationToken);

            if (parentChatMessage == null)
            {
                _logger.LogWarning(new { 
                    Method = nameof(GetReplyChainContextAsync),
                    Status = "ParentMessageNotFound",
                    ReplyParentMessageId = replyParentMessageId,
                    UserId = userId
                });
                return new List<ConversationMessage>();
            }

            // Ищем ConversationMessage, связанное с этим ChatMessage
            var parentConversationMessage = await _repositoryConversation.Query()
                .Where(cm => cm.ChatMessageId == parentChatMessage.Id)
                .OrderByDescending(cm => cm.MessageOrder)
                .FirstOrDefaultAsync(cancellationToken);

            if (parentConversationMessage == null)
            {
                _logger.LogWarning(new { 
                    Method = nameof(GetReplyChainContextAsync),
                    Status = "ParentConversationMessageNotFound",
                    ReplyParentMessageId = replyParentMessageId,
                    UserId = userId,
                    ParentChatMessageId = parentChatMessage.Id
                });
                return new List<ConversationMessage>();
            }

            // Получаем контекст диалога начиная от родительского сообщения
            var messages = await _repositoryConversation.Query()
                .Where(m => m.TwitchUserId == userId && m.MessageOrder <= parentConversationMessage.MessageOrder)
                .OrderByDescending(m => m.MessageOrder)
                .Take(limit * 2) // Берем больше, чтобы получить полные пары user-assistant
                .OrderBy(m => m.MessageOrder)
                .ToListAsync(cancellationToken);

            _logger.LogInformation(new { 
                Method = nameof(GetReplyChainContextAsync),
                UserId = userId,
                ReplyParentMessageId = replyParentMessageId,
                MessagesFound = messages.Count,
                ParentMessageOrder = parentConversationMessage.MessageOrder,
                Messages = messages.Select(m => new { m.Role, m.Content, m.MessageOrder })
            });

            return messages;
        }
        catch (Exception ex)
        {
            _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                Method = nameof(GetReplyChainContextAsync),
                Status = "Exception",
                ReplyParentMessageId = replyParentMessageId,
                UserId = userId,
                Error = ex.GetType().Name,
                Message = ex.Message,
                StackTrace = ex.StackTrace
            });

            return new List<ConversationMessage>();
        }
    }

    public async Task<bool> IsReplyToBotMessageAsync(string replyParentMessageId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(new { 
            Method = nameof(IsReplyToBotMessageAsync),
            ReplyParentMessageId = replyParentMessageId
        });

        try
        {
            // Ищем ChatMessage по MessageId
            var parentChatMessage = await _repositoryMsg.Query()
                .FirstOrDefaultAsync(cm => cm.MessageId == replyParentMessageId, cancellationToken);

            if (parentChatMessage == null)
            {
                _logger.LogInformation(new { 
                    Method = nameof(IsReplyToBotMessageAsync),
                    Status = "ParentMessageNotFound",
                    ReplyParentMessageId = replyParentMessageId,
                    Result = false
                });
                return false;
            }

            // Проверяем, есть ли ConversationMessage с ролью "assistant", связанное с этим ChatMessage
            var isBotMessage = await _repositoryConversation.Query()
                .AnyAsync(cm => cm.ChatMessageId == parentChatMessage.Id && cm.Role == "assistant", cancellationToken);

            _logger.LogInformation(new { 
                Method = nameof(IsReplyToBotMessageAsync),
                Status = "Success",
                ReplyParentMessageId = replyParentMessageId,
                ParentChatMessageId = parentChatMessage.Id,
                IsBotMessage = isBotMessage
            });

            return isBotMessage;
        }
        catch (Exception ex)
        {
            _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                Method = nameof(IsReplyToBotMessageAsync),
                Status = "Exception",
                ReplyParentMessageId = replyParentMessageId,
                Error = ex.GetType().Name,
                Message = ex.Message,
                StackTrace = ex.StackTrace
            });

            return false;
        }
    }

    public async Task<bool> LinkConversationWithBotMessageAsync(Guid conversationMessageId, Guid botChatMessageId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(new { 
            Method = nameof(LinkConversationWithBotMessageAsync),
            ConversationMessageId = conversationMessageId,
            BotChatMessageId = botChatMessageId
        });

        try
        {
            var conversationMessage = await _repositoryConversation.Query()
                .FirstOrDefaultAsync(cm => cm.Id == conversationMessageId, cancellationToken);

            if (conversationMessage == null)
            {
                _logger.LogWarning(new { 
                    Method = nameof(LinkConversationWithBotMessageAsync),
                    Status = "ConversationMessageNotFound",
                    ConversationMessageId = conversationMessageId
                });
                return false;
            }

            // Устанавливаем связь с ChatMessage бота
            conversationMessage.ChatMessageId = botChatMessageId;
            
            await _repositoryConversation.Update(conversationMessage, cancellationToken, true);

            _logger.LogInformation(new { 
                Method = nameof(LinkConversationWithBotMessageAsync),
                Status = "Success",
                ConversationMessageId = conversationMessageId,
                BotChatMessageId = botChatMessageId
            });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                Method = nameof(LinkConversationWithBotMessageAsync),
                Status = "Exception",
                ConversationMessageId = conversationMessageId,
                BotChatMessageId = botChatMessageId,
                Error = ex.GetType().Name,
                Message = ex.Message,
                StackTrace = ex.StackTrace
            });

            return false;
        }
    }

    /// <summary>
    /// Получить следующий порядковый номер сообщения для пользователя
    /// </summary>
    private async Task<int> GetNextMessageOrderAsync(Guid userId, CancellationToken cancellationToken)
    {
        var lastOrder = await _repositoryConversation.Query()
            .Where(m => m.TwitchUserId == userId)
            .OrderByDescending(m => m.MessageOrder)
            .Select(m => m.MessageOrder)
            .FirstOrDefaultAsync(cancellationToken);

        return lastOrder + 1;
    }

    public async Task<ConversationMessage?> FindConversationByTempMessageIdAsync(string tempMessageId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(new { 
            Method = nameof(FindConversationByTempMessageIdAsync),
            TempMessageId = tempMessageId
        });

        try
        {
            // Ищем ChatMessage по временному MessageId
            var chatMessage = await _repositoryMsg.Query()
                .FirstOrDefaultAsync(cm => cm.MessageId == tempMessageId, cancellationToken);

            if (chatMessage == null)
            {
                _logger.LogInformation(new { 
                    Method = nameof(FindConversationByTempMessageIdAsync),
                    Status = "ChatMessageNotFound",
                    TempMessageId = tempMessageId
                });
                return null;
            }

            // Ищем ConversationMessage, связанный с этим ChatMessage
            var conversationMessage = await _repositoryConversation.Query()
                .FirstOrDefaultAsync(cm => cm.ChatMessageId == chatMessage.Id, cancellationToken);

            if (conversationMessage != null)
            {
                _logger.LogInformation(new { 
                    Method = nameof(FindConversationByTempMessageIdAsync),
                    Status = "ConversationFound",
                    TempMessageId = tempMessageId,
                    ConversationMessageId = conversationMessage.Id,
                    UserId = conversationMessage.TwitchUserId
                });
            }
            else
            {
                _logger.LogInformation(new { 
                    Method = nameof(FindConversationByTempMessageIdAsync),
                    Status = "ConversationNotFound",
                    TempMessageId = tempMessageId,
                    ChatMessageId = chatMessage.Id
                });
            }

            return conversationMessage;
        }
        catch (Exception ex)
        {
            _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                Method = nameof(FindConversationByTempMessageIdAsync),
                Status = "Exception",
                TempMessageId = tempMessageId,
                Error = ex.GetType().Name,
                Message = ex.Message
            });
            
            return null;
        }
    }
}