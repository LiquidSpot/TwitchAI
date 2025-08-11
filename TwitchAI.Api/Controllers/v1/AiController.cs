using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TwitchAI.Application.UseCases.OpenAi;
using TwitchAI.Application.UseCases.Translation;
using TwitchAI.Domain.Entites;
using Common.Packages.Response.Models;
using TwitchAI.Api.Contracts.Requests.Ai;
using TwitchAI.Api.Contracts.Requests.Translate;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using TwitchAI.Application.Models;
using TwitchAI.Domain.Enums;
using Common.Packages.Response.Services.Interfaces;
using Common.Packages.Logger.Services.Interfaces;

namespace TwitchAI.Api.Controllers.v1;

/// <summary>
/// Эндпойнты для работы с ИИ: чат, перевод, смена роли и движка, лимиты.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/ai")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class AiController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly AppConfiguration _appConfig;
    private readonly IExternalLogger<AiController> _logger;
    private readonly ILSResponseService _response;

    /// <summary>
    /// Конструктор контроллера AI.
    /// </summary>
    public AiController(IMediator mediator, IOptions<AppConfiguration> appConfig, IExternalLogger<AiController> logger, ILSResponseService response)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _appConfig = appConfig.Value ?? throw new ArgumentNullException(nameof(appConfig));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _response = response ?? throw new ArgumentNullException(nameof(response));
    }

    /// <summary>
    /// Отправляет сообщение в ИИ с опциональным контекстом reply.
    /// </summary>
    [HttpPost("messages")]
    [ProducesResponseType(typeof(LSResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Chat([FromBody] AiChatRequestDto request, CancellationToken ct)
    {
        _logger.LogInformation(new { Request = request });
        var userMessage = new UserMessage
        {
            message = request.Message,
            role = request.Role,
            temp = request.Temperature,
            maxToken = request.MaxTokens
        };

        var command = new AiChatCommand(userMessage, request.UserId, request.ChatMessageId);
        var response = await _mediator.Send(command, ct);
        _logger.LogInformation(new { Response = response });
        return _response.GetResponse(response);
    }

    /// <summary>
    /// Устанавливает движок OpenAI для пользователя.
    /// </summary>
    [HttpPut("engine")]
    [ProducesResponseType(typeof(LSResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetEngine([FromBody] EngineUpdateRequestDto request, CancellationToken ct)
    {
        _logger.LogInformation(new { Request = request });
        var command = new EngineCommand(request.EngineName, request.UserId);
        var response = await _mediator.Send(command, ct);
        _logger.LogInformation(new { Response = response });
        return _response.GetResponse(response);
    }

    /// <summary>
    /// Возвращает список доступных движков из конфигурации.
    /// </summary>
    [HttpGet("engines")]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
    public IActionResult GetEngines()
    {
        var engines = _appConfig.OpenAi?.AvailableEngines ?? Array.Empty<string>();
        _logger.LogInformation(new { Engines = engines });
        var resp = new LSResponse<IEnumerable<string>>().Success(engines);
        _logger.LogInformation(new { Response = resp });
        return _response.GetResponse(resp);
    }

    /// <summary>
    /// Устанавливает роль бота (bot, neko, Toxic, Durka).
    /// </summary>
    [HttpPut("role")]
    [ProducesResponseType(typeof(LSResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetRole([FromBody] ChangeRoleRequestDto request, CancellationToken ct)
    {
        _logger.LogInformation(new { Request = request });
        var command = new ChangeRoleCommand(request.RoleName, request.UserId);
        var response = await _mediator.Send(command, ct);
        _logger.LogInformation(new { Response = response });
        return _response.GetResponse(response);
    }

    /// <summary>
    /// Возвращает список доступных ролей.
    /// </summary>
    [HttpGet("roles")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public IActionResult GetRoles()
    {
        var roles = Enum.GetNames<Role>();
        _logger.LogInformation(new { Roles = roles });
        var resp = new LSResponse<IEnumerable<string>>().Success(roles);
        _logger.LogInformation(new { Response = resp });
        return _response.GetResponse(resp);
    }

    /// <summary>
    /// Устанавливает персональный лимит reply-цепочки для пользователя.
    /// </summary>
    [HttpPut("reply-limit")]
    [ProducesResponseType(typeof(LSResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetReplyLimit([FromBody] ReplyLimitUpdateRequestDto request, CancellationToken ct)
    {
        _logger.LogInformation(new { Request = request });
        var command = new ReplyLimitCommand(request.Limit, request.UserId);
        var response = await _mediator.Send(command, ct);
        _logger.LogInformation(new { Response = response });
        return _response.GetResponse(response);
    }

    /// <summary>
    /// Переводит текст на указанный язык.
    /// </summary>
    [HttpPost("translate")]
    [ProducesResponseType(typeof(LSResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Translate([FromBody] TranslateRequestDto request, CancellationToken ct)
    {
        _logger.LogInformation(new { Request = request });
        var command = new TranslateCommand(request.Language, request.Message, request.UserId);
        var response = await _mediator.Send(command, ct);
        _logger.LogInformation(new { Response = response });
        return _response.GetResponse(response);
    }
}


