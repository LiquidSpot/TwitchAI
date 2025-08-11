using Common.Packages.Response.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TwitchAI.Application.Interfaces;
using TwitchAI.Application.Interfaces.Infrastructure;
using TwitchAI.Application.Models;
using TwitchAI.Domain.Entites;

namespace TwitchAI.Infrastructure.Services;

internal class RefreshTokenService : IRefreshTokenService
{
    private readonly IUnitOfWork _uow;
    private readonly IRepository<AppUser, Guid> _users;
    private readonly IRepository<RefreshToken, Guid> _tokens;
    private readonly IJwtTokenService _jwt;
    private readonly JwtConfiguration _cfg;

    public RefreshTokenService(IUnitOfWork uow, IJwtTokenService jwt, IOptions<JwtConfiguration> cfg)
    {
        _uow = uow;
        _users = uow.Factory<AppUser, Guid>()!;
        _tokens = uow.Factory<RefreshToken, Guid>()!;
        _jwt = jwt;
        _cfg = cfg.Value;
    }

    public async Task<LSResponse<(string access, string refresh)>> IssueAsync(Guid userId, string email, CancellationToken ct)
    {
        var resp = new LSResponse<(string, string)>();
        var access = _jwt.GenerateAccessToken(userId, email).Result;
        var refresh = _jwt.GenerateRefreshToken(userId, email).Result;

        await _tokens.AddAsync(new RefreshToken
        {
            AppUserId = userId,
            Token = refresh,
            ExpiresAt = DateTime.UtcNow.AddDays(_cfg.RefreshTokenLifetimeDays),
            IsRevoked = false
        }, ct, saveChanges: true);

        return resp.Success((access, refresh));
    }

    public async Task<LSResponse<(string access, string refresh)>> RefreshAsync(string refreshToken, CancellationToken ct)
    {
        var resp = new LSResponse<(string, string)>();
        var token = await _tokens.SingleOrDefaultAsync(x => x!.Token == refreshToken && !x.IsRevoked, ct);
        if (token == null || token.ExpiresAt <= DateTime.UtcNow)
            return resp.Success((string.Empty, string.Empty));

        var user = await _users.GetAsync(token.AppUserId, ct);
        if (user == null) return resp.Success((string.Empty, string.Empty));

        token.IsRevoked = true;
        await _uow.SaveChangesAsync(ct, saveChanges: true);

        return await IssueAsync(user.Id, user.Email, ct);
    }

    public async Task<LSResponse<bool>> RevokeAsync(string refreshToken, CancellationToken ct)
    {
        var resp = new LSResponse<bool>();
        var token = await _tokens.SingleOrDefaultAsync(x => x!.Token == refreshToken && !x.IsRevoked, ct);
        if (token == null) return resp.Success(false);
        token.IsRevoked = true;
        await _uow.SaveChangesAsync(ct, saveChanges: true);
        return resp.Success(true);
    }
}


