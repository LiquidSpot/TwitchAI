using Asp.Versioning;
using Common.Packages.Logger.Services.Interfaces;
using Common.Packages.Response.Models;
using Common.Packages.Response.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TwitchAI.Application.Interfaces;
using TwitchAI.Domain.Entites;
using Microsoft.AspNetCore.Authorization;
using TwitchAI.Application.Dto.Request.UserSettings;

namespace TwitchAI.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/user-settings")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class UserSettingsController : ControllerBase
{
    private readonly IUserSettingsService _settings;
    private readonly IExternalLogger<UserSettingsController> _logger;
    private readonly ILSResponseService _response;
    private readonly ICredentialProtector _protector;
    private readonly IIntegrationCheckService _integrationCheck;

    public UserSettingsController(IUserSettingsService settings, IExternalLogger<UserSettingsController> logger, ILSResponseService response, ICredentialProtector protector, IIntegrationCheckService integrationCheck)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _response = response ?? throw new ArgumentNullException(nameof(response));
        _protector = protector ?? throw new ArgumentNullException(nameof(protector));
        _integrationCheck = integrationCheck ?? throw new ArgumentNullException(nameof(integrationCheck));
    }

    [HttpGet]
    [ProducesResponseType(typeof(LSResponse<UserIntegrationSettings>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery, Required] Guid userId, CancellationToken ct)
    {
        _logger.LogInformation(new { UserId = userId });
        var result = await _settings.GetAsync(userId, ct);
        _logger.LogInformation(new { Response = result });
        return _response.GetResponse(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(LSResponse<UserIntegrationSettings>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Upsert([FromBody] UpsertUserSettingsRequestDto req, CancellationToken ct)
    {
        _logger.LogInformation(new { Request = new { req.UserId, req.TwitchChannelName, req.TwitchBotUsername, req.TwitchClientId, req.OpenAiOrganizationId, req.OpenAiProjectId } });

        var entity = new UserIntegrationSettings
        {
            AppUserId = req.UserId,
            TwitchChannelName = req.TwitchChannelName,
            TwitchBotUsername = req.TwitchBotUsername,
            TwitchAccessTokenEncrypted = _protector.Protect(req.TwitchAccessToken),
            TwitchRefreshTokenEncrypted = _protector.Protect(req.TwitchRefreshToken),
            TwitchClientId = req.TwitchClientId,
            OpenAiOrganizationId = req.OpenAiOrganizationId,
            OpenAiProjectId = req.OpenAiProjectId,
            OpenAiApiKeyEncrypted = _protector.Protect(req.OpenAiApiKey)
        };

        var result = await _settings.UpsertAsync(entity, ct);
        _logger.LogInformation(new { Response = result });
        return _response.GetResponse(result);
    }

    [HttpPost("check/twitch")]
    [ProducesResponseType(typeof(LSResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckTwitch([FromBody] CheckTwitchRequestDto req, CancellationToken ct)
    {
        _logger.LogInformation(new { Request = new { req.ClientId } });
        var result = await _integrationCheck.CheckTwitchAsync(req.ClientId, req.AccessToken, ct);
        return _response.GetResponse(result);
    }

    [HttpPost("check/openai")]
    [ProducesResponseType(typeof(LSResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckOpenAi([FromBody] CheckOpenAiRequestDto req, CancellationToken ct)
    {
        _logger.LogInformation(new { Request = new { req.OrganizationId, req.ProjectId } });
        var result = await _integrationCheck.CheckOpenAiAsync(req.ApiKey, req.OrganizationId, req.ProjectId, ct);
        return _response.GetResponse(result);
    }

    // Decrypt can be added in a GET-by-id scenario if you plan to return secrets back to UI
}


