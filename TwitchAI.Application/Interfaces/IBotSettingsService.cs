using Common.Packages.Response.Models;
using TwitchAI.Domain.Entites;

namespace TwitchAI.Application.Interfaces;

public interface IBotSettingsService
{
    Task<LSResponse<BotSettings>> GetAsync(Guid userId, CancellationToken ct);
    Task<LSResponse<BotSettings>> UpsertAsync(BotSettings settings, CancellationToken ct);
}


