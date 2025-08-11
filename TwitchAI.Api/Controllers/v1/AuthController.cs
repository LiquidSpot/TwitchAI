using Asp.Versioning;
using Common.Packages.Logger.Services.Interfaces;
using Common.Packages.Response.Models;
using Common.Packages.Response.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using TwitchAI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using TwitchAI.Application.Dto.Request.Auth;

namespace TwitchAI.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _users;
    private readonly IJwtTokenService _jwt;
    private readonly IRefreshTokenService _refreshTokens;
    private readonly IExternalLogger<AuthController> _logger;
    private readonly ILSResponseService _response;

    public AuthController(IUserService users, IJwtTokenService jwt, IRefreshTokenService refreshTokens, IExternalLogger<AuthController> logger, ILSResponseService response)
    {
        _users = users ?? throw new ArgumentNullException(nameof(users));
        _jwt = jwt ?? throw new ArgumentNullException(nameof(jwt));
        _refreshTokens = refreshTokens ?? throw new ArgumentNullException(nameof(refreshTokens));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _response = response ?? throw new ArgumentNullException(nameof(response));
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(LSResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto req, CancellationToken ct)
    {
        _logger.LogInformation(new { Request = new { req.Email } });
        var result = await _users.RegisterAsync(req.Email, req.Password, ct);
        _logger.LogInformation(new { Response = result });
        return _response.GetResponse(result);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LSResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto req, CancellationToken ct)
    {
        _logger.LogInformation(new { Request = new { req.Email } });
        var login = await _users.LoginAsync(req.Email, req.Password, ct);
        if (string.IsNullOrEmpty(login.Result))
        {
            _logger.LogInformation(new { Response = login });
            return _response.GetResponse(login);
        }

        var user = await _users.FindByEmailAsync(req.Email, ct);
        if (user.Status != Common.Packages.Response.Enums.ResponseStatus.Success || user.Result == null)
        {
            _logger.LogInformation(new { Response = login });
            return _response.GetResponse(login);
        }

        var tokens = await _refreshTokens.IssueAsync(user.Result.Id, user.Result.Email, ct);
        _logger.LogInformation(new { Response = tokens });
        return _response.GetResponse(tokens);
    }

    public sealed class RefreshRequest { public string RefreshToken { get; set; } = string.Empty; }

    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LSResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req, CancellationToken ct)
    {
        var result = await _refreshTokens.RefreshAsync(req.RefreshToken, ct);
        return _response.GetResponse(result);
    }
}


