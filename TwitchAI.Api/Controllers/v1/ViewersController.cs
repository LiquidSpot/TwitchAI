using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TwitchAI.Application.UseCases.Viewers;
using Common.Packages.Response.Models;
using Microsoft.AspNetCore.Http;
using Common.Packages.Response.Services.Interfaces;
using Common.Packages.Logger.Services.Interfaces;
using TwitchAI.Application.Dto.Response.Activity;
using TwitchAI.Application.UseCases.Viewers;

namespace TwitchAI.Api.Controllers.v1;

/// <summary>
/// Статистика зрителей: активные, молчаливые, общая статистика.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/viewers")]
public class ViewersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILSResponseService _response;
    private readonly IExternalLogger<ViewersController> _logger;

    public ViewersController(IMediator mediator, ILSResponseService response, IExternalLogger<ViewersController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _response = response ?? throw new ArgumentNullException(nameof(response));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Получить статистику по типу: viewers|silent|stats.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(LSResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats([FromQuery] Guid userId, [FromQuery] string type = "stats", CancellationToken ct = default)
    {
        _logger.LogInformation(new { UserId = userId, Type = type });
        var cmd = new ViewerStatsCommand(type, userId);
        var response = await _mediator.Send(cmd, ct);
        _logger.LogInformation(new { Response = response });
        return _response.GetResponse(response);
    }

    /// <summary>
    /// Заглушка данных активности для UI (онлайн-график и команды ИИ).
    /// </summary>
    [HttpGet("activity")]
    [ProducesResponseType(typeof(LSResponse<DashboardActivityResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivity([FromQuery] Guid userId, CancellationToken ct = default)
    {
        _logger.LogInformation(new { UserId = userId });
        var resp = await _mediator.Send(new ViewerActivityQuery(userId), ct);
        return _response.GetResponse(resp);
    }
}


