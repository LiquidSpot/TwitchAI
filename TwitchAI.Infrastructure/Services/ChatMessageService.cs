using Common.Packages.Logger.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TwitchAI.Application.Interfaces;
using TwitchAI.Application.Interfaces.Infrastructure;
using TwitchAI.Application.Models;
using TwitchAI.Domain.Entites;
using TwitchAI.Domain.Enums.ErrorCodes;

namespace TwitchAI.Infrastructure.Services;

internal class ChatMessageService : IChatMessageService
{
    private readonly IExternalLogger<ChatMessageService> _logger;
    private readonly IUnitOfWork _uow;
    private readonly IRepository<ChatMessage, Guid> _repositoryMsg;
    private readonly IRepository<TwitchUser, Guid> _repositoryUser;

    public ChatMessageService(IExternalLogger<ChatMessageService> logger, IUnitOfWork uow)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _repositoryMsg = _uow.Factory<ChatMessage, Guid>() ?? throw new InvalidOperationException("Failed to create ChatMessage repository");
        _repositoryUser = _uow.Factory<TwitchUser, Guid>() ?? throw new InvalidOperationException("Failed to create TwitchUser repository");
    }

    public async Task<ChatMessage> AddMessageAsync(TwitchUser user, TwitchLib.Client.Models.ChatMessage message, CancellationToken cancellationToken = default)
    {
        var entity = NewChatMessageFrom(message, user.Id);

        await _repositoryMsg.AddAsync(entity, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return entity;
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
    
    public async Task<ChatMessage?> GetChatMessageByMessageIdAsync(string messageId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(new { 
            Method = nameof(GetChatMessageByMessageIdAsync),
            MessageId = messageId
        });

        try
        {
            var chatMessage = await _repositoryMsg.Query()
                .FirstOrDefaultAsync(cm => cm.MessageId == messageId, cancellationToken);

            _logger.LogInformation(new { 
                Method = nameof(GetChatMessageByMessageIdAsync),
                MessageId = messageId,
                Found = chatMessage != null,
                IsReply = chatMessage?.IsReply,
                ReplyParentMessageId = chatMessage?.ReplyParentMessageId
            });

            return chatMessage;
        }
        catch (Exception ex)
        {
            _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                Method = nameof(GetChatMessageByMessageIdAsync),
                Status = "Exception",
                MessageId = messageId,
                Error = ex.GetType().Name,
                Message = ex.Message,
                StackTrace = ex.StackTrace
            });

            return null;
        }
    }

    public async Task<ChatMessage> SaveBotMessageAsync(BotSentMessage sentMessage, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(new { 
            Method = nameof(SaveBotMessageAsync),
            Message = sentMessage.Message,
            Channel = sentMessage.Channel
        });

        try
        {
            // Получаем или создаем пользователя бота
            var botUser = await GetOrCreateBotUserAsync(sentMessage.Channel, cancellationToken);

            // Создаем ChatMessage для сообщения бота
            var chatMessage = new ChatMessage
            {
                TwitchUserId = botUser.Id,
                MessageId = Guid.NewGuid().ToString(), // Генерируем уникальный ID для сообщения бота
                Channel = sentMessage.Channel,
                RoomId = "", // Для сообщений бота можем оставить пустым
                Text = sentMessage.Message,
                IsReply = false,
                ReplyParentMessageId = null,
                EmoteReplacedText = null,
                Bits = 0,
                BitsUsd = 0.0,
                IsFirstMessage = false,
                IsHighlighted = false,
                IsMeAction = false,
                IsSkippingSubMode = false,
                IsModerator = false,
                IsSubscriber = false,
                IsBroadcaster = false, // Бот не является broadcaster'ом
                IsTurbo = false,
                RawTagsJson = "{}",
                TmiSentTs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
                CreatedAt = DateTime.UtcNow
            };

            await _repositoryMsg.AddAsync(chatMessage, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(new { 
                Method = nameof(SaveBotMessageAsync),
                Status = "Success",
                ChatMessageId = chatMessage.Id,
                MessageId = chatMessage.MessageId,
                BotUserId = botUser.Id,
                Message = sentMessage.Message
            });

            return chatMessage;
        }
        catch (Exception ex)
        {
            _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                Method = nameof(SaveBotMessageAsync),
                Status = "Exception",
                Message = sentMessage.Message,
                Channel = sentMessage.Channel,
                Error = ex.GetType().Name,
                ErrorMessage = ex.Message,
                StackTrace = ex.StackTrace
            });

            throw;
        }
    }

    /// <summary>
    /// Получить или создать пользователя для бота
    /// </summary>
    private async Task<TwitchUser> GetOrCreateBotUserAsync(string channelName, CancellationToken cancellationToken)
    {
        const string botUsername = "nekotyan_ai"; // Имя бота из конфигурации
        const string botTwitchId = "885482975"; // ID бота в Twitch (можно получить из API)

        var botUser = await _repositoryUser.Query()
            .FirstOrDefaultAsync(u => u.UserName == botUsername, cancellationToken);

        if (botUser == null)
        {
            botUser = new TwitchUser
            {
                TwitchId = botTwitchId,
                UserName = botUsername,
                DisplayName = "NekoTyan_Ai",
                ColorHex = "#9146FF", // Стандартный цвет Twitch
                IsVip = false,
                IsPartner = false,
                IsStaff = false,
                IsBroadcaster = false,
                IsTurbo = false,
                SubscribedMonthCount = 0,
                CheerBits = null,
                BadgesJson = "[]",
                LastSeen = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _repositoryUser.AddAsync(botUser, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(new { 
                Method = nameof(GetOrCreateBotUserAsync),
                Status = "BotUserCreated",
                BotUserId = botUser.Id,
                Username = botUsername
            });
        }
        else
        {
            // Обновляем время последнего сообщения
            botUser.LastSeen = DateTime.UtcNow;
            await _repositoryUser.Update(botUser, cancellationToken, true);
        }

        return botUser;
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
}