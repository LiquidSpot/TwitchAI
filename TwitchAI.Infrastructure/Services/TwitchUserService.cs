using System.Text.Json;
using Common.Packages.Logger.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using TwitchAI.Application.Interfaces;
using TwitchAI.Application.Interfaces.Infrastructure;
using TwitchAI.Application.Models;
using TwitchAI.Domain.Entites;
using TwitchAI.Domain.Enums.ErrorCodes;

namespace TwitchAI.Infrastructure.Services
{
    internal class TwitchUserService : ITwitchUserService
    {
        private readonly IExternalLogger<TwitchUserService> _logger;
        private readonly IUnitOfWork _uow;
        private readonly IRepository<TwitchUser, Guid> _repository;
        private readonly IChatMessageService _chatMessageService;
        private readonly IConversationService _conversationService;

        public TwitchUserService(
            IExternalLogger<TwitchUserService> logger, 
            IUnitOfWork uow,
            IChatMessageService chatMessageService,
            IConversationService conversationService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _repository = _uow.Factory<TwitchUser, Guid>() ?? throw new InvalidOperationException("Failed to create TwitchUser repository");
            _chatMessageService = chatMessageService ?? throw new ArgumentNullException(nameof(chatMessageService));
            _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
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
            return await _chatMessageService.AddMessageAsync(user, message, cancellationToken);
        }

        public async Task<ConversationMessage> AddUserMessageToContextAsync(TwitchUser user, string message, Guid? chatMessageId = null, CancellationToken cancellationToken = default)
        {
            return await _conversationService.AddUserMessageToContextAsync(user, message, chatMessageId, cancellationToken);
        }

        public async Task<ConversationMessage> AddGptResponseToContextAsync(TwitchUser user, string gptResponse, string? openAiResponseId = null, string? modelName = null, int? tokenCount = null, Guid? chatMessageId = null, CancellationToken cancellationToken = default)
        {
            return await _conversationService.AddGptResponseToContextAsync(user, gptResponse, openAiResponseId, modelName, tokenCount, chatMessageId, cancellationToken);
        }

        public async Task<List<ConversationMessage>> GetUserConversationContextAsync(Guid userId, int limit = 3, CancellationToken cancellationToken = default)
        {
            return await _conversationService.GetUserConversationContextAsync(userId, limit, cancellationToken);
        }

        public async Task<int> ClearUserConversationContextAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _conversationService.ClearUserConversationContextAsync(userId, cancellationToken);
        }

        public async Task<List<ConversationMessage>> GetReplyChainContextAsync(string replyParentMessageId, Guid userId, int limit = 3, CancellationToken cancellationToken = default)
        {
            return await _conversationService.GetReplyChainContextAsync(replyParentMessageId, userId, limit, cancellationToken);
        }

        public async Task<bool> IsReplyToBotMessageAsync(string replyParentMessageId, CancellationToken cancellationToken = default)
        {
            return await _conversationService.IsReplyToBotMessageAsync(replyParentMessageId, cancellationToken);
        }

        public async Task<ChatMessage?> GetChatMessageByIdAsync(Guid chatMessageId, CancellationToken cancellationToken = default)
        {
            return await _chatMessageService.GetChatMessageByIdAsync(chatMessageId, cancellationToken);
        }
        
        public async Task<ChatMessage?> GetChatMessageByMessageIdAsync(string messageId, CancellationToken cancellationToken = default)
        {
            return await _chatMessageService.GetChatMessageByMessageIdAsync(messageId, cancellationToken);
        }

        public async Task<ChatMessage> SaveBotMessageAsync(BotSentMessage sentMessage, CancellationToken cancellationToken = default)
        {
            return await _chatMessageService.SaveBotMessageAsync(sentMessage, cancellationToken);
        }

        public async Task<bool> LinkConversationWithBotMessageAsync(Guid conversationMessageId, Guid botChatMessageId, CancellationToken cancellationToken = default)
        {
            return await _conversationService.LinkConversationWithBotMessageAsync(conversationMessageId, botChatMessageId, cancellationToken);
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

    public async Task<ConversationMessage?> FindConversationByTempMessageIdAsync(string tempMessageId, CancellationToken cancellationToken = default)
    {
        return await _conversationService.FindConversationByTempMessageIdAsync(tempMessageId, cancellationToken);
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
    }
}
