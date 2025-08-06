using Common.Packages.Logger.Services.Interfaces;
using Microsoft.Extensions.Options;
using TwitchAI.Application.Interfaces;
using TwitchAI.Application.Models;
using TwitchAI.Domain.Enums.ErrorCodes;

namespace TwitchAI.Infrastructure.Services;

public class EngineService : IEngineService
{
    private readonly IExternalLogger<EngineService> _logger;
    private readonly AppConfiguration _appConfig;
    private static readonly Dictionary<Guid, string> _userEngines = new();

    public EngineService(
        IExternalLogger<EngineService> logger,
        IOptions<AppConfiguration> appConfig)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _appConfig = appConfig?.Value ?? throw new ArgumentNullException(nameof(appConfig));
    }

    public async Task<string> GetEngineAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(new { 
            Method = nameof(GetEngineAsync),
            UserId = userId
        });

        try
        {
            if (_userEngines.TryGetValue(userId, out var engine))
            {
                _logger.LogInformation(new { 
                    Method = nameof(GetEngineAsync),
                    Status = "CustomEngineFound",
                    UserId = userId,
                    Engine = engine
                });
                
                return engine;
            }

            // Возвращаем движок по умолчанию из конфигурации
            var defaultEngine = _appConfig.OpenAi?.Model ?? "o4-mini-2025-04-16";
            
            _logger.LogInformation(new { 
                Method = nameof(GetEngineAsync),
                Status = "DefaultEngine",
                UserId = userId,
                Engine = defaultEngine
            });

            return defaultEngine;
        }
        catch (Exception ex)
        {
            _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                Method = nameof(GetEngineAsync),
                Status = "Exception",
                UserId = userId,
                Error = ex.GetType().Name,
                Message = ex.Message
            });

            // Возвращаем значение по умолчанию в случае ошибки
            return "o4-mini-2025-04-16";
        }
    }

    public async Task SetEngineAsync(Guid userId, string engineName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(new { 
            Method = nameof(SetEngineAsync),
            UserId = userId,
            NewEngine = engineName
        });

        try
        {
            _userEngines[userId] = engineName;

            _logger.LogInformation(new { 
                Method = nameof(SetEngineAsync),
                Status = "Success",
                UserId = userId,
                NewEngine = engineName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                Method = nameof(SetEngineAsync),
                Status = "Exception",
                UserId = userId,
                NewEngine = engineName,
                Error = ex.GetType().Name,
                Message = ex.Message
            });

            throw; // Пробрасываем исключение для обработки в обработчике команды
        }
    }
}