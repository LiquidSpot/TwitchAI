using Common.Packages.Logger.Services.Interfaces;
using Common.Packages.Response.Models;

using MediatR;

using TwitchAI.Domain.Enums.ErrorCodes;

namespace TwitchAI.Application.Behaviors;

/// <summary>
///  Прерывает конвейер, если токен отменён.
/// </summary>
public sealed class CancellationTokenBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IExternalLogger<CancellationTokenBehavior<TRequest, TResponse>> _logger;

    public CancellationTokenBehavior(
        IExternalLogger<CancellationTokenBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            _logger.LogError((int)BaseErrorCodes.CancellationTokenRequested, new { Message = "Operation canceled by CancellationToken." });

            if (typeof(TResponse) == typeof(LSResponse))
            {
                object resp = new LSResponse()
                   .Error(BaseErrorCodes.CancellationTokenRequested);
                return (TResponse)resp;
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        return await next(cancellationToken).ConfigureAwait(false);
    }
}