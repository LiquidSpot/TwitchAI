using Asp.Versioning;
using Common.Packages.Logger.Services.Interfaces;
using Common.Packages.Response.Models;
using Common.Packages.Response.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TwitchAI.Application.Interfaces;
using TwitchAI.Domain.Entites;
using TwitchAI.Application.Dto.Request.Bot;

namespace TwitchAI.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/bot/config")]
[Authorize]
public class BotConfigController : ControllerBase
{
    private readonly IBotSettingsService _settings;
    private readonly IExternalLogger<BotConfigController> _logger;
    private readonly ILSResponseService _response;

    public BotConfigController(IBotSettingsService settings, IExternalLogger<BotConfigController> logger, ILSResponseService response)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _response = response ?? throw new ArgumentNullException(nameof(response));
    }

    [HttpGet]
    [ProducesResponseType(typeof(LSResponse<BotSettings>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery, Required] Guid userId, CancellationToken ct)
    {
        _logger.LogInformation(new { UserId = userId });
        var result = await _settings.GetAsync(userId, ct);
        _logger.LogInformation(new { Response = result });
        return _response.GetResponse(result);
    }

    [HttpPut]
    [ProducesResponseType(typeof(LSResponse<BotSettings>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromBody] UpdateBotSettingsRequestDto req, CancellationToken ct)
    {
        _logger.LogInformation(new { Request = req });
        var entity = new BotSettings
        {
            AppUserId = req.UserId,
            DefaultRole = req.DefaultRole,
            CooldownSeconds = req.CooldownSeconds,
            ReplyLimit = req.ReplyLimit,
            EnableAi = req.EnableAi,
            EnableCompliment = req.EnableCompliment,
            EnableFact = req.EnableFact,
            EnableHoliday = req.EnableHoliday,
            EnableTranslation = req.EnableTranslation,
            EnableSoundAlerts = req.EnableSoundAlerts,
            EnableViewersStats = req.EnableViewersStats
        };

        var result = await _settings.UpsertAsync(entity, ct);
        _logger.LogInformation(new { Response = result });
        return _response.GetResponse(result);
    }
}


