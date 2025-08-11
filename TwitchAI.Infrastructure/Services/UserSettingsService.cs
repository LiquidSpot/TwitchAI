using Common.Packages.Logger.Services.Interfaces;
using Common.Packages.Response.Models;
using Microsoft.EntityFrameworkCore;
using TwitchAI.Application.Interfaces;
using TwitchAI.Application.Interfaces.Infrastructure;
using TwitchAI.Domain.Entites;

namespace TwitchAI.Infrastructure.Services;

internal class UserSettingsService : IUserSettingsService
{
    private readonly IUnitOfWork _uow;
    private readonly IRepository<UserIntegrationSettings, Guid> _repo;
    private readonly IExternalLogger<UserSettingsService> _logger;

    public UserSettingsService(IUnitOfWork uow, IExternalLogger<UserSettingsService> logger)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _repo = uow.Factory<UserIntegrationSettings, Guid>() ?? throw new ArgumentNullException(nameof(uow));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LSResponse<UserIntegrationSettings>> GetAsync(Guid userId, CancellationToken ct)
    {
        var resp = new LSResponse<UserIntegrationSettings>();
        _logger.LogInformation(new { UserId = userId });
        var val = await _repo.SingleOrDefaultAsync(x => x!.AppUserId == userId, ct);
        _logger.LogInformation(new { Result = val is null ? "null" : "found" });
        return resp.Success(val!);
    }

    public async Task<LSResponse<UserIntegrationSettings>> UpsertAsync(UserIntegrationSettings settings, CancellationToken ct)
    {
        var resp = new LSResponse<UserIntegrationSettings>();
        _logger.LogInformation(new { Request = new { settings.AppUserId, settings.TwitchChannelName, settings.TwitchBotUsername, settings.TwitchClientId, settings.OpenAiOrganizationId, settings.OpenAiProjectId } });
        var existing = await _repo.SingleOrDefaultAsync(x => x!.AppUserId == settings.AppUserId, ct);
        if (existing == null)
        {
            await _repo.AddAsync(settings, ct);
        }
        else
        {
            existing.TwitchChannelName = settings.TwitchChannelName;
            existing.TwitchBotUsername = settings.TwitchBotUsername;
            existing.TwitchAccessTokenEncrypted = settings.TwitchAccessTokenEncrypted;
            existing.TwitchRefreshTokenEncrypted = settings.TwitchRefreshTokenEncrypted;
            existing.TwitchClientId = settings.TwitchClientId;
            existing.OpenAiOrganizationId = settings.OpenAiOrganizationId;
            existing.OpenAiProjectId = settings.OpenAiProjectId;
            existing.OpenAiApiKeyEncrypted = settings.OpenAiApiKeyEncrypted;
            await _repo.Update(existing, ct);
        }

        await _uow.SaveChangesAsync(ct, saveChanges: true);
        _logger.LogInformation(new { Saved = true });
        return resp.Success(settings);
    }
}


