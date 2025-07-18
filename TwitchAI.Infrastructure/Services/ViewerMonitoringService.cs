using System.Text.Json;
using Common.Packages.HttpClient.Models;
using Common.Packages.HttpClient.Services.Interfaces;
using Common.Packages.Logger.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TwitchAI.Application.Constants;
using TwitchAI.Application.Dto.Response;
using TwitchAI.Application.Interfaces;
using TwitchAI.Application.Interfaces.Infrastructure;
using TwitchAI.Application.Models;
using TwitchAI.Domain.Entites;
using TwitchAI.Domain.Enums.ErrorCodes;

namespace TwitchAI.Infrastructure.Services
{
    /// <summary>
    /// Сервис для мониторинга зрителей в чате
    /// </summary>
    internal class ViewerMonitoringService : IViewerMonitoringService
    {
        private readonly ILSClientService _httpClient;
        private readonly IExternalLogger<ViewerMonitoringService> _logger;
        private readonly TwitchConfiguration _twitchConfig;
        private readonly ITwitchUserService _twitchUserService;
        private readonly IUnitOfWork _uow;
        private readonly IRepository<ViewerPresence, Guid> _viewerPresenceRepository;
        private readonly IRepository<TwitchUser, Guid> _twitchUserRepository;

        private readonly Timer? _monitoringTimer;
        private readonly TimeSpan _monitoringInterval = TimeSpan.FromMinutes(2); // Проверяем каждые 2 минуты
        private bool _isMonitoring = false;

        public ViewerMonitoringService(
            ILSClientService httpClient,
            IExternalLogger<ViewerMonitoringService> logger,
            IOptions<TwitchConfiguration> twitchConfig,
            ITwitchUserService twitchUserService,
            IUnitOfWork uow)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _twitchConfig = twitchConfig?.Value ?? throw new ArgumentNullException(nameof(twitchConfig));
            _twitchUserService = twitchUserService ?? throw new ArgumentNullException(nameof(twitchUserService));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            
            _viewerPresenceRepository = _uow.Factory<ViewerPresence, Guid>() ?? throw new InvalidOperationException("ViewerPresence repository not found");
            _twitchUserRepository = _uow.Factory<TwitchUser, Guid>() ?? throw new InvalidOperationException("TwitchUser repository not found");
        }

        public async Task<List<string>> GetCurrentViewersAsync(string channelName, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(new { 
                    Method = nameof(GetCurrentViewersAsync),
                    ChannelName = channelName
                });

                // Получаем ID канала по имени
                var channelId = await GetChannelIdByNameAsync(channelName, cancellationToken);
                if (string.IsNullOrEmpty(channelId))
                {
                    _logger.LogWarning(new { 
                        Method = nameof(GetCurrentViewersAsync),
                        Message = "Channel ID not found",
                        ChannelName = channelName
                    });
                    return new List<string>();
                }

                // Получаем ID бота (токен принадлежит боту, поэтому moderator_id должен быть ID бота)
                var botId = await GetChannelIdByNameAsync(_twitchConfig.BotUsername, cancellationToken);
                if (string.IsNullOrEmpty(botId))
                {
                    _logger.LogWarning(new { 
                        Method = nameof(GetCurrentViewersAsync),
                        Message = "Bot ID not found",
                        BotUsername = _twitchConfig.BotUsername
                    });
                    return new List<string>();
                }

                // Получаем список зрителей
                var url = $"{Constants.TwitchApis.GetChatters}?broadcaster_id={channelId}&moderator_id={botId}";
                
                _logger.LogInformation(new { 
                    Method = nameof(GetCurrentViewersAsync),
                    ChannelName = channelName,
                    ChannelId = channelId,
                    BotUsername = _twitchConfig.BotUsername,
                    BotId = botId,
                    RequestUrl = url
                });
                
                var request = new RequestBuilder<object>()
                    .WithUrl(url)
                    .WithMethod(HttpMethod.Get)
                    .WithHeaders(new Dictionary<string, string>
                    {
                        ["Client-Id"] = _twitchConfig.ClientId,
                        ["Authorization"] = $"Bearer {_twitchConfig.BotAccessToken}"
                    })
                    .Build();

                var response = await _httpClient.ExecuteRequestAsync<TwitchChattersResponseDto>(request, Constants.TwitchApiClientKey, cancellationToken);

                if (response.Status == Common.Packages.Response.Enums.ResponseStatus.Success && response.Result != null)
                {
                    var viewers = response.Result.Data.Select(c => c.User_Login).ToList();
                    
                    _logger.LogInformation(new { 
                        Method = nameof(GetCurrentViewersAsync),
                        ChannelName = channelName,
                        ViewersCount = viewers.Count
                    });

                    return viewers;
                }
                else
                {
                    _logger.LogError((int)TwitchAI.Domain.Enums.ErrorCodes.BaseErrorCodes.InternalServerError, new { 
                        Method = nameof(GetCurrentViewersAsync),
                        Status = "Error getting viewers",
                        ChannelName = channelName,
                        ErrorCode = response.ErrorCode,
                        ErrorMessage = response.ErrorObjects,
                        BotUsername = _twitchConfig.BotUsername,
                        BotId = botId,
                        Message = "Bot might not be a moderator on this channel. Please add the bot as a moderator or use a different approach."
                    });
                    return new List<string>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError((int)TwitchAI.Domain.Enums.ErrorCodes.BaseErrorCodes.InternalServerError, new { 
                    Method = nameof(GetCurrentViewersAsync),
                    Error = ex.GetType().Name,
                    Message = ex.Message,
                    ChannelName = channelName
                });
                return new List<string>();
            }
        }

        public async Task<int> UpdateViewerPresenceAsync(string channelName, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(new { 
                    Method = nameof(UpdateViewerPresenceAsync),
                    ChannelName = channelName
                });

                var currentViewers = await GetCurrentViewersAsync(channelName, cancellationToken);
                if (!currentViewers.Any())
                {
                    return 0;
                }

                var updatedCount = 0;
                var now = DateTime.UtcNow;

                foreach (var viewerUsername in currentViewers)
                {
                    try
                    {
                        // Получаем или создаем пользователя
                        var twitchUser = await GetOrCreateTwitchUserByUsernameAsync(viewerUsername, cancellationToken);
                        if (twitchUser == null) continue;

                        // Получаем или создаем запись о присутствии
                        var viewerPresence = await _viewerPresenceRepository.Query()
                            .FirstOrDefaultAsync(vp => vp.TwitchUserId == twitchUser.Id && vp.ChannelName == channelName, cancellationToken);

                        if (viewerPresence == null)
                        {
                            // Создаем новую запись о присутствии
                            viewerPresence = new ViewerPresence
                            {
                                TwitchUserId = twitchUser.Id,
                                ChannelName = channelName,
                                LastSeenInChat = now,
                                IsActive = true,
                                IsSilent = true,
                                CurrentSessionStarted = now,
                                SessionCount = 1,
                                TotalPresenceMinutes = 0
                            };

                            await _viewerPresenceRepository.AddAsync(viewerPresence, cancellationToken);
                        }
                        else
                        {
                            // Обновляем существующую запись
                            var timeSinceLastSeen = now - viewerPresence.LastSeenInChat;
                            
                            // Если прошло более 10 минут, считаем это новой сессией
                            if (timeSinceLastSeen > TimeSpan.FromMinutes(10))
                            {
                                viewerPresence.SessionCount++;
                                viewerPresence.CurrentSessionStarted = now;
                            }
                            else
                            {
                                // Добавляем время к общему времени присутствия
                                viewerPresence.TotalPresenceMinutes += (int)timeSinceLastSeen.TotalMinutes;
                            }

                            viewerPresence.LastSeenInChat = now;
                            viewerPresence.IsActive = true;

                            await _viewerPresenceRepository.Update(viewerPresence, cancellationToken);
                        }

                        updatedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError((int)TwitchAI.Domain.Enums.ErrorCodes.BaseErrorCodes.InternalServerError, new { 
                            Method = nameof(UpdateViewerPresenceAsync),
                            Error = ex.GetType().Name,
                            Message = ex.Message,
                            ViewerUsername = viewerUsername,
                            ChannelName = channelName
                        });
                    }
                }

                // Отмечаем неактивных зрителей
                var inactiveViewers = await _viewerPresenceRepository.Query()
                    .Where(vp => vp.ChannelName == channelName && 
                                 vp.IsActive && 
                                 vp.LastSeenInChat < now.AddMinutes(-5))
                    .ToListAsync(cancellationToken);

                foreach (var inactiveViewer in inactiveViewers)
                {
                    inactiveViewer.IsActive = false;
                    // Добавляем время последней сессии к общему времени
                    if (inactiveViewer.CurrentSessionStarted.HasValue)
                    {
                        var sessionDuration = now - inactiveViewer.CurrentSessionStarted.Value;
                        inactiveViewer.TotalPresenceMinutes += (int)sessionDuration.TotalMinutes;
                        inactiveViewer.CurrentSessionStarted = null;
                    }
                    await _viewerPresenceRepository.Update(inactiveViewer, cancellationToken);
                }

                await _uow.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(new { 
                    Method = nameof(UpdateViewerPresenceAsync),
                    ChannelName = channelName,
                    UpdatedCount = updatedCount,
                    InactiveCount = inactiveViewers.Count
                });

                return updatedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError((int)TwitchAI.Domain.Enums.ErrorCodes.BaseErrorCodes.InternalServerError, new { 
                    Method = nameof(UpdateViewerPresenceAsync),
                    Error = ex.GetType().Name,
                    Message = ex.Message,
                    ChannelName = channelName
                });
                return 0;
            }
        }

        public async Task<List<TwitchUser>> GetSilentViewersAsync(string channelName, TimeSpan timeSpan, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(new { 
                    Method = nameof(GetSilentViewersAsync),
                    ChannelName = channelName,
                    TimeSpan = timeSpan
                });

                var cutoffTime = DateTime.UtcNow - timeSpan;

                var silentViewers = await _viewerPresenceRepository.Query()
                    .Include(vp => vp.TwitchUser)
                    .Where(vp => vp.ChannelName == channelName &&
                                 vp.IsActive &&
                                 vp.IsSilent &&
                                 vp.LastSeenInChat >= cutoffTime &&
                                 (vp.LastMessageAt == null || vp.LastMessageAt < cutoffTime))
                    .Select(vp => vp.TwitchUser)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation(new { 
                    Method = nameof(GetSilentViewersAsync),
                    ChannelName = channelName,
                    SilentViewersCount = silentViewers.Count
                });

                return silentViewers;
            }
            catch (Exception ex)
            {
                _logger.LogError((int)TwitchAI.Domain.Enums.ErrorCodes.BaseErrorCodes.InternalServerError, new { 
                    Method = nameof(GetSilentViewersAsync),
                    Error = ex.GetType().Name,
                    Message = ex.Message,
                    ChannelName = channelName
                });
                return new List<TwitchUser>();
            }
        }

        public async Task<TwitchUser> MarkViewerAsActiveAsync(string username, CancellationToken cancellationToken = default)
        {
            try
            {
                var twitchUser = await _twitchUserRepository.Query()
                    .FirstOrDefaultAsync(u => u.UserName == username, cancellationToken);

                if (twitchUser == null)
                {
                    throw new InvalidOperationException($"User {username} not found");
                }

                // Обновляем запись о присутствии
                var viewerPresence = await _viewerPresenceRepository.Query()
                    .FirstOrDefaultAsync(vp => vp.TwitchUserId == twitchUser.Id && vp.ChannelName == _twitchConfig.ChannelName, cancellationToken);

                if (viewerPresence != null)
                {
                    viewerPresence.LastMessageAt = DateTime.UtcNow;
                    viewerPresence.IsSilent = false;
                    await _viewerPresenceRepository.Update(viewerPresence, cancellationToken);
                    await _uow.SaveChangesAsync(cancellationToken);
                }

                return twitchUser;
            }
            catch (Exception ex)
            {
                _logger.LogError((int)TwitchAI.Domain.Enums.ErrorCodes.BaseErrorCodes.InternalServerError, new { 
                    Method = nameof(MarkViewerAsActiveAsync),
                    Error = ex.GetType().Name,
                    Message = ex.Message,
                    Username = username
                });
                throw;
            }
        }

        public async Task StartMonitoringAsync(CancellationToken cancellationToken = default)
        {
            if (_isMonitoring)
            {
                _logger.LogWarning(new { 
                    Method = nameof(StartMonitoringAsync),
                    Message = "Monitoring is already running"
                });
                return;
            }

            _isMonitoring = true;
            _logger.LogInformation(new { 
                Method = nameof(StartMonitoringAsync),
                Message = "Starting viewer monitoring",
                Interval = _monitoringInterval
            });

            // Запускаем периодический мониторинг
            _ = Task.Run(async () =>
            {
                while (_isMonitoring && !cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await UpdateViewerPresenceAsync(_twitchConfig.ChannelName, cancellationToken);
                        await Task.Delay(_monitoringInterval, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError((int)TwitchAI.Domain.Enums.ErrorCodes.BaseErrorCodes.InternalServerError, new { 
                            Method = nameof(StartMonitoringAsync),
                            Error = ex.GetType().Name,
                            Message = ex.Message
                        });
                        await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken); // Ждем минуту при ошибке
                    }
                }
            }, cancellationToken);
        }

        public async Task StopMonitoringAsync()
        {
            if (!_isMonitoring)
            {
                return;
            }

            _isMonitoring = false;
            _logger.LogInformation(new { 
                Method = nameof(StopMonitoringAsync),
                Message = "Stopping viewer monitoring"
            });

            await Task.CompletedTask;
        }

        private async Task<string?> GetChannelIdByNameAsync(string channelName, CancellationToken cancellationToken)
        {
            try
            {
                var url = $"{Constants.TwitchApis.GetUsers}?login={channelName}";
                
                var request = new RequestBuilder<object>()
                    .WithUrl(url)
                    .WithMethod(HttpMethod.Get)
                    .WithHeaders(new Dictionary<string, string>
                    {
                        ["Client-Id"] = _twitchConfig.ClientId,
                        ["Authorization"] = $"Bearer {_twitchConfig.BotAccessToken}"
                    })
                    .Build();

                var response = await _httpClient.ExecuteRequestAsync<TwitchApiResponseDto<TwitchUserDto>>(request, Constants.TwitchApiClientKey, cancellationToken);

                if (response.Status == Common.Packages.Response.Enums.ResponseStatus.Success && 
                    response.Result != null && 
                    response.Result.Data.Any())
                {
                    return response.Result.Data.First().Id;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError((int)TwitchAI.Domain.Enums.ErrorCodes.BaseErrorCodes.InternalServerError, new { 
                    Method = nameof(GetChannelIdByNameAsync),
                    Error = ex.GetType().Name,
                    Message = ex.Message,
                    ChannelName = channelName
                });
                return null;
            }
        }

        private async Task<TwitchUser?> GetOrCreateTwitchUserByUsernameAsync(string username, CancellationToken cancellationToken)
        {
            try
            {
                // Сначала ищем в базе
                var existingUser = await _twitchUserRepository.Query()
                    .FirstOrDefaultAsync(u => u.UserName == username, cancellationToken);

                if (existingUser != null)
                {
                    return existingUser;
                }

                // Получаем информацию о пользователе из Twitch API
                var url = $"{Constants.TwitchApis.GetUsers}?login={username}";
                
                var request = new RequestBuilder<object>()
                    .WithUrl(url)
                    .WithMethod(HttpMethod.Get)
                    .WithHeaders(new Dictionary<string, string>
                    {
                        ["Client-Id"] = _twitchConfig.ClientId,
                        ["Authorization"] = $"Bearer {_twitchConfig.BotAccessToken}"
                    })
                    .Build();

                var response = await _httpClient.ExecuteRequestAsync<TwitchApiResponseDto<TwitchUserDto>>(request, Constants.TwitchApiClientKey, cancellationToken);

                if (response.Status == Common.Packages.Response.Enums.ResponseStatus.Success && 
                    response.Result != null && 
                    response.Result.Data.Any())
                {
                    var twitchUserData = response.Result.Data.First();
                    
                    // Создаем нового пользователя
                    var newUser = new TwitchUser
                    {
                        TwitchId = twitchUserData.Id,
                        UserName = twitchUserData.Login,
                        DisplayName = twitchUserData.Display_Name,
                        ColorHex = "#FFFFFF", // По умолчанию
                        IsVip = false,
                        IsPartner = twitchUserData.Broadcaster_Type == "partner",
                        IsStaff = false,
                        IsBroadcaster = false,
                        IsTurbo = false,
                        BadgesJson = "[]",
                        LastSeen = DateTime.UtcNow
                    };

                    await _twitchUserRepository.AddAsync(newUser, cancellationToken);
                    await _uow.SaveChangesAsync(cancellationToken);

                    return newUser;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError((int)TwitchAI.Domain.Enums.ErrorCodes.BaseErrorCodes.InternalServerError, new { 
                    Method = nameof(GetOrCreateTwitchUserByUsernameAsync),
                    Error = ex.GetType().Name,
                    Message = ex.Message,
                    Username = username
                });
                return null;
            }
        }
    }
} 