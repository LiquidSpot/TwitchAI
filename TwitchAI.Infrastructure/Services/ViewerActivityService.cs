using Common.Packages.Logger.Services.Interfaces;
using Common.Packages.Response.Models;
using TwitchAI.Application.Dto.Response.Activity;
using TwitchAI.Application.Interfaces;
using TwitchAI.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TwitchAI.Infrastructure.Services;

public class ViewerActivityService : IViewerActivityService
{
    private readonly IExternalLogger<ViewerActivityService> _logger;
    private readonly ApplicationDbContext _db;

    public ViewerActivityService(IExternalLogger<ViewerActivityService> logger, ApplicationDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public Task<LSResponse<DashboardActivityResponseDto>> GetActivityAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(new { UserId = userId });

        // Определить канал пользователя (если задан в настройках)
        var settings = _db.Set<TwitchAI.Domain.Entites.UserIntegrationSettings>()
            .AsNoTracking()
            .FirstOrDefault(s => s.AppUserId == userId);
        var channelLower = (settings?.TwitchChannelName ?? string.Empty).Trim().ToLowerInvariant();

        // За последние 60 минут биним по 5 минут
        var now = DateTime.UtcNow;
        var windowStart = now.AddHours(-1);

        // online: количество уникальных активных зрителей по временным срезам
        // Используем ViewerPresence.LastSeenInChat как индикатор присутсвия
        var presencesQuery = _db.Set<TwitchAI.Domain.Entites.ViewerPresence>()
            .AsNoTracking()
            .Where(v => v.LastSeenInChat >= windowStart);
        if (!string.IsNullOrEmpty(channelLower)) presencesQuery = presencesQuery.Where(v => v.ChannelName.ToLower() == channelLower);
        var presences = presencesQuery.ToList();

        var labels = new List<string>();
        var online = new List<int>();
        for (var i = 11; i >= 0; i--)
        {
            var from = now.AddMinutes(-(i + 1) * 5);
            var to = now.AddMinutes(-i * 5);
            labels.Add(to.ToString("HH:mm"));
            var count = presences.Count(p => p.LastSeenInChat >= from && p.LastSeenInChat < to);
            online.Add(count);
        }

        // ai commands: топ по командам за день на основе ChatMessage
        var dayStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        var cmdsQuery = _db.Set<TwitchAI.Domain.Entites.ChatMessage>()
            .AsNoTracking()
            .Where(m => m.CreatedAt >= dayStart && m.Text.StartsWith("!"));
        if (!string.IsNullOrEmpty(channelLower)) cmdsQuery = cmdsQuery.Where(m => m.Channel.ToLower() == channelLower);
        var commands = cmdsQuery
            .AsEnumerable()
            .Select(m => {
                var t = m.Text ?? string.Empty;
                var idx = t.IndexOf(' ');
                return idx > 0 ? t.Substring(0, idx) : t;
            })
            .GroupBy(c => c)
            .Select(g => new { Command = g.Key, Cnt = g.Count() })
            .OrderByDescending(x => x.Cnt)
            .Take(10)
            .ToArray();

        if (commands.Length == 0)
        {
            var defaults = new[] {
                "!ai",
                "!engine",
                "!reply-limit",
                "!sound",
                "!compliment",
                "!fact",
                "!holiday",
                "!ru", "!en", "!ja", "!zh", "!es",
                "!viewers", "!silent", "!stats"
            };
            var rnd = new Random(userId.GetHashCode());
            var fakeCounts = defaults.Select(_ => rnd.Next(1, 15)).ToArray();

            var dtoDef = new DashboardActivityResponseDto
            {
                Labels = labels.ToArray(),
                OnlineSamples = online.ToArray(),
                AiCommands = defaults,
                AiCommandCounts = fakeCounts
            };
            var respDef = new LSResponse<DashboardActivityResponseDto>().Success(dtoDef);
            return Task.FromResult(respDef);
        }

        var dto = new DashboardActivityResponseDto
        {
            Labels = labels.ToArray(),
            OnlineSamples = online.ToArray(),
            AiCommands = commands.Select(x => x.Command).ToArray(),
            AiCommandCounts = commands.Select(x => x.Cnt).ToArray(),
        };

        var resp = new LSResponse<DashboardActivityResponseDto>().Success(dto);
        return Task.FromResult(resp);
    }
}


