using Common.Packages.Response.Models;
using Common.Packages.Response.Services;

namespace TwitchAI.Api.Middleware;

public class RequestIdMiddleware
{
    private readonly RequestDelegate _next;

    public RequestIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var requestId = string.Format($"ID:{Guid.NewGuid().ToString("N").Substring(0, 10)}");

        context.Items[LsCommonConstants.Context.RequestId] = requestId;
        context.Response.Headers[LsCommonConstants.Context.RequestId] = requestId;

        LSRequestContext.Id = requestId;

        using (Serilog.Context.LogContext.PushProperty("RequestID", requestId))
        {
            await _next(context);
        }
    }
}