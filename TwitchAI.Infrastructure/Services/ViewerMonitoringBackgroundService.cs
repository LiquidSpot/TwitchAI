using Common.Packages.Logger.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TwitchAI.Application.Interfaces;
using TwitchAI.Application.Models;

namespace TwitchAI.Infrastructure.Services
{
    /// <summary>
    /// Фоновый сервис для мониторинга зрителей
    /// </summary>
    internal class ViewerMonitoringBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly TwitchConfiguration _twitchConfig;
        private readonly TimeSpan _monitoringInterval = TimeSpan.FromMinutes(2);

        public ViewerMonitoringBackgroundService(
            IServiceScopeFactory serviceScopeFactory,
            IOptions<TwitchConfiguration> twitchConfig)
        {
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _twitchConfig = twitchConfig?.Value ?? throw new ArgumentNullException(nameof(twitchConfig));
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            // Используем Console.WriteLine для логирования в StartAsync, так как это синглтон
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Starting viewer monitoring background service. Interval: {_monitoringInterval}");

            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            // Используем Console.WriteLine для логирования в StopAsync, так как это синглтон
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Stopping viewer monitoring background service");

            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Ждем 30 секунд перед началом мониторинга, чтобы дать время другим сервисам запуститься
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Starting viewer monitoring execution for channel: {_twitchConfig.ChannelName}");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using var scope = _serviceScopeFactory.CreateAsyncScope();
                    var viewerMonitoringService = scope.ServiceProvider.GetRequiredService<IViewerMonitoringService>();
                    var logger = scope.ServiceProvider.GetRequiredService<IExternalLogger<ViewerMonitoringBackgroundService>>();

                    // Обновляем информацию о зрителях
                    var updatedCount = await viewerMonitoringService.UpdateViewerPresenceAsync(
                        _twitchConfig.ChannelName, 
                        stoppingToken);

                    logger.LogInformation(new { 
                        Method = nameof(ExecuteAsync),
                        Message = "Viewer monitoring cycle completed",
                        UpdatedViewers = updatedCount,
                        ChannelName = _twitchConfig.ChannelName
                    });

                    // Получаем статистику о молчаливых зрителях каждые 10 минут
                    if (DateTime.UtcNow.Minute % 10 == 0)
                    {
                        var silentViewers = await viewerMonitoringService.GetSilentViewersAsync(
                            _twitchConfig.ChannelName,
                            TimeSpan.FromHours(1),
                            stoppingToken);

                        logger.LogInformation(new { 
                            Method = nameof(ExecuteAsync),
                            Message = "Silent viewers statistics",
                            SilentViewersCount = silentViewers.Count,
                            ChannelName = _twitchConfig.ChannelName
                        });
                    }
                }
                catch (OperationCanceledException)
                {
                    // Нормальная остановка
                    break;
                }
                catch (Exception ex)
                {
                    // Для ошибок используем Console.WriteLine, так как scope может быть недоступен
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Error in ViewerMonitoringBackgroundService: {ex.GetType().Name} - {ex.Message}");

                    // Ждем дольше при ошибке
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                    continue;
                }

                // Ждем до следующего цикла
                await Task.Delay(_monitoringInterval, stoppingToken);
            }
        }
    }
} 