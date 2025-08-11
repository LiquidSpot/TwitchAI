using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TwitchAI.Application.UseCases.Holidays;
using TwitchAI.Application.UseCases.Facts;
using TwitchAI.Application.UseCases.Compliment;
using Common.Packages.Response.Models;
using TwitchAI.Api.Contracts.Requests.Utility;
using Microsoft.AspNetCore.Http;
using Common.Packages.Response.Services.Interfaces;
using Common.Packages.Logger.Services.Interfaces;

namespace TwitchAI.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/utility")]
[Microsoft.AspNetCore.Authorization.Authorize]
/// <summary>
/// Утилитарные команды: праздники, факты, комплименты.
/// </summary>
public class UtilityController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILSResponseService _response;
    private readonly IExternalLogger<UtilityController> _logger;

    public UtilityController(IMediator mediator, ILSResponseService response, IExternalLogger<UtilityController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _response = response ?? throw new ArgumentNullException(nameof(response));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Получить праздник дня (с переводом).
    /// </summary>
    [HttpGet("holiday")]
    [ProducesResponseType(typeof(LSResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHoliday([FromQuery] Guid userId, [FromQuery] DateTime? date, CancellationToken ct)
    {
        _logger.LogInformation(new { UserId = userId, Date = date });
        var cmd = new HolidayCommand(userId, date);
        var response = await _mediator.Send(cmd, ct);
        _logger.LogInformation(new { Response = response });
        return _response.GetResponse(response);
    }

    /// <summary>
    /// Случайный интересный факт (с учетом кулдауна и кеша).
    /// </summary>
    [HttpGet("fact")]
    [ProducesResponseType(typeof(LSResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFact([FromQuery] Guid userId, CancellationToken ct)
    {
        _logger.LogInformation(new { UserId = userId });
        var cmd = new FactCommand(userId);
        var response = await _mediator.Send(cmd, ct);
        _logger.LogInformation(new { Response = response });
        return _response.GetResponse(response);
    }

    /// <summary>
    /// Сгенерировать комплимент целевому пользователю или себе.
    /// </summary>
    [HttpPost("compliment")]
    [ProducesResponseType(typeof(LSResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Compliment([FromBody] ComplimentRequestDto request, CancellationToken ct)
    {
        _logger.LogInformation(new { Request = request });
        var cmd = new ComplimentCommand(request.UserId, request.TargetUsername);
        var response = await _mediator.Send(cmd, ct);
        _logger.LogInformation(new { Response = response });
        return _response.GetResponse(response);
    }
}


