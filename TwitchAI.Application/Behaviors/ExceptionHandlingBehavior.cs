using System.Net;

using Common.Packages.Logger.Services.Interfaces;
using Common.Packages.Response.Exceptions;
using Common.Packages.Response.Models;

using FluentValidation;

using MediatR;

using Microsoft.OpenApi.Extensions;

using Newtonsoft.Json;

using TwitchAI.Domain.Enums.ErrorCodes;

namespace TwitchAI.Application.Behaviors;

//[DebuggerNonUserCode] // IMPORTANT: This attribute is used to avoid debugging this class
public class ExceptionHandlingBehavior<TRequest, TResponse>: IPipelineBehavior<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
    where TResponse : LSResponse, new()
{
    private readonly IExternalLogger<TRequest> _logger;

    public ExceptionHandlingBehavior(IExternalLogger<TRequest> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try { return await next(cancellationToken); }
        catch (ValidationException validationException)
        {
            return HandleException(BaseErrorCodes.ValidationProcessError, validationException, request);
        }
        catch (LSException iybException) { return HandleException(iybException.ErrorEnum, iybException, request); }
        catch (Exception ex) { return HandleException(BaseErrorCodes.InternalServerError, ex, request); }
    }

    private TResponse HandleException(Enum errorCode, Exception exception, TRequest request)
    {
        var response = new TResponse() { ErrorCode = errorCode };
        response.ErrorObjects = exception switch { LSException ex => ex.ErrorObjects, _ => null };

        _logger.LogError(errorCode.GetHashCode(), new
                         {
                             ExceptionType = exception.GetType().Name,
                             ErrorCode = errorCode.GetDisplayName(),
                             Exception = ExceptionFormatter.Format(exception),
                             //TODO: Request security vulnerable not working attribute's [SensitiveData]/[IgnoreLog]
                             Request = JsonConvert.SerializeObject(request),
                         });

        return (TResponse)response.Error(errorCode, httpStatusCode: (int)HttpStatusCode.OK);
    }
}
