using System.Reflection;
using System.Runtime.ExceptionServices;

using Castle.DynamicProxy;

using Common.Packages.Logger.Services.Interfaces;

using TwitchAI.Domain.Enums.ErrorCodes;

namespace TwitchAI.Application.Behaviors;

/// <summary>
/// Интерцептор для логирования входа/выхода/исключений 
/// c использованием IAsyncInterceptor (удобнее для async методов).
/// </summary>
public class LoggingInterceptor : IAsyncInterceptor
{
    private readonly IExternalLogger<LoggingInterceptor> _logger;

    public LoggingInterceptor(IExternalLogger<LoggingInterceptor> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void InterceptSynchronous(IInvocation invocation) { invocation.ReturnValue = Sync(invocation); }

    /// <summary>
    /// Для синхронных методов (возвращают не Task).
    /// </summary>
    /// <param name="invocation">Информация о вызове.</param>
    public object Sync(IInvocation invocation)
    {
        var methodName = $"{invocation.TargetType.Name}.{invocation.Method.Name}";
        try
        {
            if (IsIQueryableOrTaskIQueryable(invocation.Method.ReturnType))
            {
                invocation.Proceed();
                return invocation.ReturnValue;
            }

            _logger.LogInformation(new { Start = methodName, Args = invocation.Arguments });
            invocation.Proceed();
            var result = invocation.ReturnValue;
            _logger.LogInformation(new { Complete = methodName, Result = result });

            return result;
        }
        catch (TargetInvocationException tie)
        {
            _logger.LogError((int)BaseErrorCodes.InternalServerError, new { Message = $"{methodName} {tie.Message}" });

            var exceptionToThrow = ExceptionDispatchInfo.Capture(tie.InnerException ?? tie);
            exceptionToThrow.Throw();
        }
        catch (Exception ex)
        {
            _logger.LogError((int)BaseErrorCodes.InternalServerError, new { Message = $"{methodName} {ex.Message}" });
            ExceptionDispatchInfo.Capture(ex).Throw();
        }

        return null;
    }

    /// <summary>
    /// Для асинхронных методов, возвращающих Task (но не Task&lt;T&gt;).
    /// </summary>
    /// <param name="invocation">Информация о вызове.</param>
    public void InterceptAsynchronous(IInvocation invocation) { invocation.ReturnValue = Async(invocation); }

    /// <summary>
    /// Для асинхронных методов, возвращающих Task&lt;T&gt;.
    /// </summary>
    /// <typeparam name="TResult">Тип, который возвращается в Task&lt;T&gt;.</typeparam>
    /// <param name="invocation">Информация о вызове.</param>
    public void InterceptAsynchronous<TResult>(IInvocation invocation) { invocation.ReturnValue = Async<TResult>(invocation); }

    /// <summary>
    /// Обработка методов, возвращающих Task (без результата).
    /// </summary>
    private async Task Async(IInvocation invocation)
    {
        var methodName = $"{invocation.TargetType.Name}.{invocation.Method.Name}";
        try
        {
            _logger.LogInformation(new { Start = methodName, Args = invocation.Arguments });
            invocation.Proceed();
            var task = (Task)invocation.ReturnValue;
            await task;
            _logger.LogInformation(new { Complete = methodName, IsCompleted = task.IsCompleted });
        }
        catch (TargetInvocationException tie)
        {
            _logger.LogError((int)BaseErrorCodes.InternalServerError, new { Message = $"{methodName} {tie.Message}" });

            var exceptionToThrow = ExceptionDispatchInfo.Capture(tie.InnerException ?? tie);
            exceptionToThrow.Throw();
        }
        catch (Exception ex)
        {
            _logger.LogError((int)BaseErrorCodes.InternalServerError, new { Message = $"{methodName} {ex.Message}" });
            throw;
        }
    }

    /// <summary>
    /// Обработка методов, возвращающих Task&lt;T&gt;.
    /// </summary>
    private async Task<TResult> Async<TResult>(IInvocation invocation)
    {
        var methodName = $"{invocation.TargetType.Name}.{invocation.Method.Name}";
        var returnType = invocation.Method.ReturnType;

        // Skip logging for methods that return IQueryable<AnyType> or Task<IQueryable<AnyType>>
        if (IsIQueryableOrTaskIQueryable(returnType))
        {
            // Just proceed with the method without logging
            invocation.Proceed();

            // Return the result
            return await (Task<TResult>)invocation.ReturnValue;
        }

        try
        {
            _logger.LogInformation(new { Start = methodName, Args = invocation.Arguments });
            invocation.Proceed();
            var task = (Task<TResult>)invocation.ReturnValue;
            var result = await task;
            _logger.LogInformation(new { Complete = methodName, Result = result });

            return result;
        }
        catch (TargetInvocationException tie)
        {
            _logger.LogError((int)BaseErrorCodes.InternalServerError, new { Message = $"{methodName} {tie.Message}" });

            var exceptionToThrow = ExceptionDispatchInfo.Capture(tie.InnerException ?? tie);
            exceptionToThrow.Throw();
        }
        catch (Exception ex)
        {
            _logger.LogError((int)BaseErrorCodes.InternalServerError, new { Message = $"{methodName} {ex.Message}" });
            throw;
        }

        return default;
    }

    private static bool IsIQueryableOrTaskIQueryable(Type returnType)
    {
        if (returnType.IsGenericType || returnType.IsArray)
        {
            var genericType = returnType.GetGenericTypeDefinition();

            if (genericType == typeof(IQueryable<>) || genericType == typeof(Task<>) || genericType == typeof(ICollection<>))
            {
                var taskGenericArg = returnType.GetGenericArguments().FirstOrDefault();
                if (taskGenericArg is { IsGenericType: true } &&
                    taskGenericArg.GetGenericTypeDefinition() == typeof(IQueryable<>)) { return true; }

                return true;
            }
        }

        return false;
    }
}