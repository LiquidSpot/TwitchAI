using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Common.Packages.Response.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TwitchAI.Application.Interfaces;
using TwitchAI.Application.Models;

namespace TwitchAI.Infrastructure.Services;

internal class JwtTokenService : IJwtTokenService
{
    private readonly JwtConfiguration _cfg;

    public JwtTokenService(IOptions<JwtConfiguration> cfg)
    {
        _cfg = cfg.Value;
    }

    public LSResponse<string> GenerateAccessToken(Guid userId, string email)
    {
        var resp = new LSResponse<string>();
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _cfg.Issuer,
            audience: _cfg.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_cfg.AccessTokenLifetimeMinutes),
            signingCredentials: creds);

        return resp.Success(new JwtSecurityTokenHandler().WriteToken(token));
    }

    public LSResponse<string> GenerateRefreshToken(Guid userId, string email)
    {
        var resp = new LSResponse<string>();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _cfg.Issuer,
            audience: _cfg.Audience,
            claims: new[] { new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()), new Claim(JwtRegisteredClaimNames.Email, email) },
            expires: DateTime.UtcNow.AddDays(_cfg.RefreshTokenLifetimeDays),
            signingCredentials: creds);

        return resp.Success(new JwtSecurityTokenHandler().WriteToken(token));
    }
}


