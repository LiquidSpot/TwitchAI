using Common.Packages.Logger.Services.Interfaces;
using Common.Packages.Response.Behaviors;
using Common.Packages.Response.Models;
using Microsoft.Extensions.Options;
using TwitchAI.Application.Interfaces;
using TwitchAI.Application.Models;
using TwitchAI.Domain.Enums.ErrorCodes;

namespace TwitchAI.Application.UseCases.Viewers
{
    /// <summary>
    /// Обработчик команды для получения статистики зрителей
    /// </summary>
    internal class ViewerStatsCommandHandler : ICommandHandler<ViewerStatsCommand, LSResponse<string>>
    {
        private readonly IViewerMonitoringService _viewerMonitoringService;
        private readonly IExternalLogger<ViewerStatsCommandHandler> _logger;
        private readonly TwitchConfiguration _twitchConfig;

        public ViewerStatsCommandHandler(
            IViewerMonitoringService viewerMonitoringService,
            IExternalLogger<ViewerStatsCommandHandler> logger,
            IOptions<TwitchConfiguration> twitchConfig)
        {
            _viewerMonitoringService = viewerMonitoringService ?? throw new ArgumentNullException(nameof(viewerMonitoringService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _twitchConfig = twitchConfig?.Value ?? throw new ArgumentNullException(nameof(twitchConfig));
        }

        public async Task<LSResponse<string>> Handle(ViewerStatsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(new { 
                Method = nameof(Handle),
                UserId = request.UserId,
                CommandType = request.CommandType
            });

            var result = new LSResponse<string>();

            try
            {
                var channelName = _twitchConfig.ChannelName;

                switch (request.CommandType.ToLower())
                {
                    case "viewers":
                        var currentViewers = await _viewerMonitoringService.GetCurrentViewersAsync(channelName, cancellationToken);
                        var message = $"🎯 В чате сейчас {currentViewers.Count} зрителей";
                        
                        return result.Success(message);

                    case "silent":
                        var silentViewers = await _viewerMonitoringService.GetSilentViewersAsync(channelName, TimeSpan.FromHours(1), cancellationToken);
                        var silentMessage = $"🤫 Молчаливых зрителей за последний час: {silentViewers.Count}";
                        
                        return result.Success(silentMessage);

                    case "stats":
                        var viewers = await _viewerMonitoringService.GetCurrentViewersAsync(channelName, cancellationToken);
                        var silent = await _viewerMonitoringService.GetSilentViewersAsync(channelName, TimeSpan.FromHours(1), cancellationToken);
                        var active = viewers.Count - silent.Count;
                        
                        var statsMessage = $"📊 Статистика чата:\n" +
                                         $"👥 Всего зрителей: {viewers.Count}\n" +
                                         $"💬 Активных (писали): {active}\n" +
                                         $"🤫 Молчаливых: {silent.Count}";
                        
                        return result.Success(statsMessage);

                    default:
                        return result.Success("❓ Неизвестная команда статистики. Доступные: !viewers, !silent, !stats");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                    Method = nameof(Handle),
                    Status = "Exception",
                    UserId = request.UserId,
                    CommandType = request.CommandType,
                    Error = ex.GetType().Name,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace
                });

                return result.Error(BaseErrorCodes.OperationProcessError, "Произошла ошибка при получении статистики зрителей.");
            }
        }
    }
} 