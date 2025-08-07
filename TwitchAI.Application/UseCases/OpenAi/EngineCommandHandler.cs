using Common.Packages.Logger.Services.Interfaces;
using Common.Packages.Response.Behaviors;
using Common.Packages.Response.Models;
using Microsoft.Extensions.Options;
using TwitchAI.Application.Interfaces;
using TwitchAI.Application.Models;
using TwitchAI.Domain.Enums.ErrorCodes;

namespace TwitchAI.Application.UseCases.OpenAi;

internal class EngineCommandHandler : ICommandHandler<EngineCommand, LSResponse<string>>
{
    private readonly IEngineService _engineService;
    private readonly IExternalLogger<EngineCommandHandler> _logger;
    private readonly AppConfiguration _appConfig;

    public EngineCommandHandler(
        IEngineService engineService,
        IExternalLogger<EngineCommandHandler> logger,
        IOptions<AppConfiguration> appConfig)
    {
        _engineService = engineService ?? throw new ArgumentNullException(nameof(engineService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _appConfig = appConfig?.Value ?? throw new ArgumentNullException(nameof(appConfig));
    }

    public async Task<LSResponse<string>> Handle(EngineCommand request, CancellationToken cancellationToken)
    {
        var response = new LSResponse<string>();

        _logger.LogInformation(new { 
            Method = nameof(Handle),
            UserId = request.UserId,
            NewEngine = request.EngineName
        });

        try
        {
            // Валидация названия движка из конфигурации
            var validEngines = _appConfig.OpenAi?.AvailableEngines ?? new[] { "gpt-4o-2024-11-20", "gpt-4.1-2025-04-14", "chatgpt-4o-latest", "o4-mini-2025-04-16", "o3-2025-04-16" };
            var availableEngines = string.Join(", ", validEngines);
            
            // Проверка на пустое название (неправильный формат команды)
            if (string.IsNullOrWhiteSpace(request.EngineName))
            {
                response.Result = $"❌ Неправильный формат команды. Используйте: !engine <название>. Доступные движки: {availableEngines}";
                return response.Success();
            }
            
            if (!validEngines.Contains(request.EngineName, StringComparer.OrdinalIgnoreCase))
            {
                response.Result = $"❌ Неизвестный движок '{request.EngineName}'. Доступные движки: {availableEngines}";
                return response.Success();
            }

            // Устанавливаем новый движок
            await _engineService.SetEngineAsync(request.UserId, request.EngineName, cancellationToken);

            response.Result = $"✅ Движок OpenAI изменен на: {request.EngineName}";

            _logger.LogInformation(new { 
                Method = nameof(Handle),
                Status = "Success",
                UserId = request.UserId,
                NewEngine = request.EngineName
            });

            return response.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                Method = nameof(Handle),
                Status = "Error",
                UserId = request.UserId,
                NewEngine = request.EngineName,
                Error = ex.Message,
                StackTrace = ex.StackTrace
            });

            response.Result = "❌ Произошла ошибка при смене движка. Попробуйте позже.";
            return response.Success(); // Возвращаем успех, чтобы сообщение об ошибке отправилось в чат
        }
    }
}