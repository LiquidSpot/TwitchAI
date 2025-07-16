using System.Diagnostics;

using Common.Packages.Logger.Services.Interfaces;
using Common.Packages.Response.Exceptions;

using MediatR;

using TwitchAI.Domain.Enums.ErrorCodes;

public sealed class LoggerPipelineBehavior<TRequest, TResponse>: IPipelineBehavior<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
{
    private readonly IExternalLogger<LoggerPipelineBehavior<TRequest, TResponse>> _logger;

    public LoggerPipelineBehavior(IExternalLogger<LoggerPipelineBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger ?? throw new ArgumentException(nameof(logger));
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        var requestName = typeof(TRequest).Name;
        try
        {
            _logger.LogInformation(new { Name = requestName, Request = request });

            var response = await next();

            _logger.LogInformation(new { Name = requestName, Elapsed = $"{stopwatch.ElapsedMilliseconds} ms", Response = response });

            return response;
        }
        catch (LSException ex)
        {
            _logger.LogError(ex.ErrorEnum.GetHashCode(), new
            {
                Name = requestName,
                Elapsed = $"{stopwatch.ElapsedMilliseconds} ms",
                Error = ex.Message,
                Inner = ex.InnerException?.Message
            });

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError((int)BaseErrorCodes.InternalServerError, new
            {
                Name = requestName,
                Elapsed = $"{stopwatch.ElapsedMilliseconds} ms",
                Error = ex.Message,
                Inner = ex.InnerException?.Message
            });

            throw;
        }
        finally
        {
            stopwatch.Stop();
        }
    }
}