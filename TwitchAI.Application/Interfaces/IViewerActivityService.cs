using Common.Packages.Response.Models;
using TwitchAI.Application.Dto.Response.Activity;

namespace TwitchAI.Application.Interfaces
{
    /// <summary>
    /// Сервис предоставления агрегированной активности для UI (онлайн/команды ИИ).
    /// </summary>
    public interface IViewerActivityService
    {
        Task<LSResponse<DashboardActivityResponseDto>> GetActivityAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}


