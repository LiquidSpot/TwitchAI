using Common.Packages.Response.Models;

namespace TwitchAI.Application.Interfaces;

public interface IJwtTokenService
{
    LSResponse<string> GenerateAccessToken(Guid userId, string email);
    LSResponse<string> GenerateRefreshToken(Guid userId, string email);
}


