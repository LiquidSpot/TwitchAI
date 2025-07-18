﻿using System.Security.Cryptography;
using System.Text.Json;
using Common.Packages.Logger.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using TwitchAI.Application.Interfaces;
using TwitchAI.Application.Interfaces.Infrastructure;
using TwitchAI.Domain.Entites;
using TwitchAI.Domain.Enums.ErrorCodes;

namespace TwitchAI.Infrastructure.Services
{
    internal class TwitchUserService : ITwitchUserService
    {
        private readonly IExternalLogger<TwitchUserService> _logger;
        private readonly IUnitOfWork _uow;
        private readonly IRepository<TwitchUser, Guid>? _repository;
        private readonly IRepository<ChatMessage, Guid>? _repositoryMsg;
        private readonly IRepository<ConversationMessage, Guid>? _repositoryConversation;

        public TwitchUserService(IExternalLogger<TwitchUserService> logger, IUnitOfWork uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _repository = _uow.Factory<TwitchUser, Guid>() ?? default;
            _repositoryMsg = _uow.Factory<ChatMessage, Guid>() ?? default;
            _repositoryConversation = _uow.Factory<ConversationMessage, Guid>() ?? default;
        }

        // TODO: not like this do Segregate agregation
        public async Task<TwitchUser> GetOrCreateUserAsync(TwitchLib.Client.Models.ChatMessage message, CancellationToken ct = default)
        {
            var (user, _) = await GetOrCreateUserWithStatusAsync(message, ct);
            return user;
        }

        public async Task<(TwitchUser User, bool WasCreated)> GetOrCreateUserWithStatusAsync(TwitchLib.Client.Models.ChatMessage message, CancellationToken ct = default)
        {
            var id = message.UserId;
            var user = await _repository
                            .Query()
                            .FirstOrDefaultAsync(u => u.TwitchId == id, ct);

            bool wasCreated = false;

            if (user is null)
            {
                user = NewUserFrom(message);
                wasCreated = true;
                
                try
                {
                    await _repository.AddAsync(user, ct);
                    return (user, wasCreated);
                }
                catch (DbUpdateException)
                {
                    // Если произошла ошибка при создании (например, race condition), 
                    // получаем существующего пользователя
                    user = await _repository.Query()
                                            .FirstAsync(u => u.TwitchId == id, ct);
                    wasCreated = false;
                    return (user, wasCreated);
                }
            }

            user.UserName = message.Username;
            user.DisplayName = message.DisplayName;
            user.ColorHex = message.ColorHex;
            user.IsVip = message.IsVip;
            user.IsPartner = message.IsPartner;
            user.IsStaff = message.IsStaff;
            user.IsBroadcaster = message.IsBroadcaster;
            user.IsTurbo = message.IsTurbo;
            user.SubscribedMonthCount = message.SubscribedMonthCount;
            user.BadgesJson = JsonSerializer.Serialize(message.Badges ?? new());
            user.CheerBits = message.CheerBadge?.CheerAmount;
            user.LastSeen = DateTime.UtcNow;

            await _repository.Update(user, ct, true);

            return (user, wasCreated);
        }

        public async Task<ChatMessage> AddMessage(TwitchUser user, TwitchLib.Client.Models.ChatMessage message, CancellationToken cancellationToken)
        {
            var entity = NewChatMessageFrom(message, user.Id);

            await _repositoryMsg.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await _uow.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return entity;
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

        public async Task<ConversationMessage> AddGptResponseToContextAsync(TwitchUser user, string gptResponse, string? openAiResponseId = null, string? modelName = null, int? tokenCount = null, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(new { 
                Method = nameof(AddGptResponseToContextAsync),
                UserId = user.Id,
                UserName = user.UserName,
                GptResponse = gptResponse,
                OpenAiResponseId = openAiResponseId,
                ModelName = modelName,
                TokenCount = tokenCount
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

        public async Task<ChatMessage?> GetChatMessageByIdAsync(Guid chatMessageId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(new { 
                Method = nameof(GetChatMessageByIdAsync),
                ChatMessageId = chatMessageId
            });

            try
            {
                var chatMessage = await _repositoryMsg.Query()
                    .FirstOrDefaultAsync(cm => cm.Id == chatMessageId, cancellationToken);

                _logger.LogInformation(new { 
                    Method = nameof(GetChatMessageByIdAsync),
                    ChatMessageId = chatMessageId,
                    Found = chatMessage != null,
                    IsReply = chatMessage?.IsReply,
                    ReplyParentMessageId = chatMessage?.ReplyParentMessageId
                });

                return chatMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                    Method = nameof(GetChatMessageByIdAsync),
                    Status = "Exception",
                    ChatMessageId = chatMessageId,
                    Error = ex.GetType().Name,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace
                });

                return null;
            }
        }

        private static ChatMessage NewChatMessageFrom(TwitchLib.Client.Models.ChatMessage m, Guid userId)
        {
            // Извлекаем информацию о reply
            string? replyParentMessageId = null;
            bool isReply = false;

            try
            {
                // Попробуем найти reply информацию в RawIrcMessage (строка)
                var rawMessage = m.RawIrcMessage;
                if (!string.IsNullOrEmpty(rawMessage) && rawMessage.Contains("reply-parent-msg-id="))
                {
                    // Парсим reply-parent-msg-id из строки IRC сообщения
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
                            replyParentMessageId = rawMessage.Substring(valueStart, valueEnd - valueStart);
                            isReply = !string.IsNullOrEmpty(replyParentMessageId);
                        }
                    }
                }
            }
            catch
            {
                // Если не удалось извлечь информацию о reply, продолжаем без неё
            }

            return new ChatMessage
            {
                TwitchUserId = userId,
                MessageId = m.Id,
                Channel = m.Channel,
                RoomId = m.RoomId,
                IsReply = isReply,
                ReplyParentMessageId = replyParentMessageId,
                Text = m.Message,
                EmoteReplacedText = m.EmoteReplacedMessage,
                Bits = m.Bits,
                BitsUsd = m.BitsInDollars,
                IsFirstMessage = m.IsFirstMessage,
                IsHighlighted = m.IsHighlighted,
                IsMeAction = m.IsMe,
                IsSkippingSubMode = m.IsSkippingSubMode,
                IsModerator = m.IsModerator,
                IsSubscriber = m.IsSubscriber,
                IsBroadcaster = m.IsBroadcaster,
                IsTurbo = m.IsTurbo,
                RawTagsJson = JsonSerializer.Serialize(m.RawIrcMessage),
                TmiSentTs = m.TmiSentTs
            };
        }

        private static TwitchUser NewUserFrom(TwitchLib.Client.Models.ChatMessage m) => new()
        {
            TwitchId = m.UserId,
            UserName = m.Username,
            DisplayName = m.DisplayName,
            ColorHex = m.ColorHex,
            IsVip = m.IsVip,
            IsPartner = m.IsPartner,
            IsStaff = m.IsStaff,
            IsBroadcaster = m.IsBroadcaster,
            IsTurbo = m.IsTurbo,
            SubscribedMonthCount = m.SubscribedMonthCount,
            BadgesJson = JsonSerializer.Serialize(m.Badges ?? new()),
            CheerBits = m.CheerBadge?.CheerAmount,
        };

        /// <returns>true – если что-то поменяли</returns>
        private static void UpdateUserFrom(TwitchLib.Client.Models.ChatMessage src, TwitchUser dst)
        {
            
        }

        public async Task<TwitchUser?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(new { 
                Method = nameof(GetUserByIdAsync),
                UserId = userId
            });

            var user = await _repository.Query()
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            _logger.LogInformation(new { 
                Method = nameof(GetUserByIdAsync),
                UserId = userId,
                Found = user != null,
                UserName = user?.UserName
            });

            return user;
        }
    }
}
