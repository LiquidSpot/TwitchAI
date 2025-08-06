using Common.Packages.Logger.Services.Interfaces;
using TwitchAI.Application.Interfaces;
using TwitchAI.Domain.Enums.ErrorCodes;

namespace TwitchAI.Infrastructure.Services;

public class ReplyLimitService : IReplyLimitService
{
    private readonly IExternalLogger<ReplyLimitService> _logger;
    private static readonly Dictionary<Guid, int> _userLimits = new();
    private const int DefaultLimit = 3;

    public ReplyLimitService(IExternalLogger<ReplyLimitService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<int> GetReplyLimitAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(new { 
            Method = nameof(GetReplyLimitAsync),
            UserId = userId
        });

        try
        {
            if (_userLimits.TryGetValue(userId, out var limit))
            {
                _logger.LogInformation(new { 
                    Method = nameof(GetReplyLimitAsync),
                    Status = "CustomLimitFound",
                    UserId = userId,
                    Limit = limit
                });
                
                return limit;
            }

            _logger.LogInformation(new { 
                Method = nameof(GetReplyLimitAsync),
                Status = "DefaultLimit",
                UserId = userId,
                Limit = DefaultLimit
            });

            return DefaultLimit;
        }
        catch (Exception ex)
        {
            _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                Method = nameof(GetReplyLimitAsync),
                Status = "Exception",
                UserId = userId,
                Error = ex.GetType().Name,
                Message = ex.Message
            });

            // Возвращаем значение по умолчанию в случае ошибки
            return DefaultLimit;
        }
    }

    public async Task SetReplyLimitAsync(Guid userId, int limit, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(new { 
            Method = nameof(SetReplyLimitAsync),
            UserId = userId,
            NewLimit = limit
        });

        try
        {
            _userLimits[userId] = limit;

            _logger.LogInformation(new { 
                Method = nameof(SetReplyLimitAsync),
                Status = "Success",
                UserId = userId,
                NewLimit = limit
            });
        }
        catch (Exception ex)
        {
            _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                Method = nameof(SetReplyLimitAsync),
                Status = "Exception",
                UserId = userId,
                NewLimit = limit,
                Error = ex.GetType().Name,
                Message = ex.Message
            });

            throw; // Пробрасываем исключение для обработки в обработчике команды
        }
    }
}