using Common.Packages.Response.Models;
using TwitchAI.Application.Interfaces;
using TwitchAI.Application.Interfaces.Infrastructure;
using TwitchAI.Domain.Entites;

namespace TwitchAI.Infrastructure.Services;

internal class BotSettingsService : IBotSettingsService
{
    private readonly IUnitOfWork _uow;
    private readonly IRepository<BotSettings, Guid> _repo;

    public BotSettingsService(IUnitOfWork uow)
    {
        _uow = uow;
        _repo = uow.Factory<BotSettings, Guid>()!;
    }

    public async Task<LSResponse<BotSettings>> GetAsync(Guid userId, CancellationToken ct)
    {
        var resp = new LSResponse<BotSettings>();
        var val = await _repo.SingleOrDefaultAsync(x => x!.AppUserId == userId, ct);
        return resp.Success(val!);
    }

    public async Task<LSResponse<BotSettings>> UpsertAsync(BotSettings settings, CancellationToken ct)
    {
        var resp = new LSResponse<BotSettings>();
        var existing = await _repo.SingleOrDefaultAsync(x => x!.AppUserId == settings.AppUserId, ct);
        if (existing == null)
        {
            await _repo.AddAsync(settings, ct, saveChanges: true);
            return resp.Success(settings);
        }

        existing.DefaultRole = settings.DefaultRole;
        existing.CooldownSeconds = settings.CooldownSeconds;
        existing.ReplyLimit = settings.ReplyLimit;
        existing.EnableAi = settings.EnableAi;
        existing.EnableCompliment = settings.EnableCompliment;
        existing.EnableFact = settings.EnableFact;
        existing.EnableHoliday = settings.EnableHoliday;
        existing.EnableTranslation = settings.EnableTranslation;
        existing.EnableSoundAlerts = settings.EnableSoundAlerts;
        existing.EnableViewersStats = settings.EnableViewersStats;
        await _repo.Update(existing, ct, saveChanges: true);
        return resp.Success(existing);
    }
}


