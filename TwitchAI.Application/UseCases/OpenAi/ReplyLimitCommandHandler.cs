using Common.Packages.Logger.Services.Interfaces;
using Common.Packages.Response.Behaviors;
using Common.Packages.Response.Models;
using TwitchAI.Application.Interfaces;
using TwitchAI.Domain.Enums.ErrorCodes;

namespace TwitchAI.Application.UseCases.OpenAi;

internal class ReplyLimitCommandHandler : ICommandHandler<ReplyLimitCommand, LSResponse<string>>
{
    private readonly IReplyLimitService _replyLimitService;
    private readonly IExternalLogger<ReplyLimitCommandHandler> _logger;

    public ReplyLimitCommandHandler(
        IReplyLimitService replyLimitService,
        IExternalLogger<ReplyLimitCommandHandler> logger)
    {
        _replyLimitService = replyLimitService ?? throw new ArgumentNullException(nameof(replyLimitService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LSResponse<string>> Handle(ReplyLimitCommand request, CancellationToken cancellationToken)
    {
        var response = new LSResponse<string>();

        _logger.LogInformation(new { 
            Method = nameof(Handle),
            UserId = request.UserId,
            NewLimit = request.Limit
        });

        try
        {
            // Валидация лимита
            if (request.Limit < 1 || request.Limit > 10)
            {
                response.Result = "❌ Лимит должен быть от 1 до 10 сообщений.";
                return response.Success();
            }

            // Устанавливаем новый лимит
            await _replyLimitService.SetReplyLimitAsync(request.UserId, request.Limit, cancellationToken);

            response.Result = $"✅ Лимит цепочки reply установлен: {request.Limit} сообщений.";

            _logger.LogInformation(new { 
                Method = nameof(Handle),
                Status = "Success",
                UserId = request.UserId,
                NewLimit = request.Limit
            });

            return response.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                Method = nameof(Handle),
                Status = "Error",
                UserId = request.UserId,
                NewLimit = request.Limit,
                Error = ex.Message,
                StackTrace = ex.StackTrace
            });

            response.Result = "❌ Произошла ошибка при установке лимита. Попробуйте позже.";
            return response.Success(); // Возвращаем успех, чтобы сообщение об ошибке отправилось в чат
        }
    }
}