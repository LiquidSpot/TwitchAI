using Common.Packages.Response.Models;
using TwitchAI.Domain.Entites;

namespace TwitchAI.Application.Interfaces;

public interface IUserSettingsService
{
    Task<LSResponse<UserIntegrationSettings>> GetAsync(Guid userId, CancellationToken ct);
    Task<LSResponse<UserIntegrationSettings>> UpsertAsync(UserIntegrationSettings settings, CancellationToken ct);
}


