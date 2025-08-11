using Common.Packages.Response.Models;

namespace TwitchAI.Application.Interfaces;

public interface IRefreshTokenService
{
    Task<LSResponse<(string access, string refresh)>> IssueAsync(Guid userId, string email, CancellationToken ct);
    Task<LSResponse<(string access, string refresh)>> RefreshAsync(string refreshToken, CancellationToken ct);
    Task<LSResponse<bool>> RevokeAsync(string refreshToken, CancellationToken ct);
}


