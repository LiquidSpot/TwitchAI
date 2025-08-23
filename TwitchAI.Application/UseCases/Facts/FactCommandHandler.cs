using Common.Packages.Logger.Services.Interfaces;
using Common.Packages.Response.Behaviors;
using Common.Packages.Response.Models;
using Microsoft.Extensions.Options;
using TwitchAI.Application.Interfaces;
using TwitchAI.Application.Models;
using TwitchAI.Domain.Enums.ErrorCodes;

namespace TwitchAI.Application.UseCases.Facts
{
    /// <summary>
    /// Обработчик команды для получения фактов
    /// </summary>
    internal class FactCommandHandler : ICommandHandler<FactCommand, LSResponse<string>>
    {
        private readonly IExternalLogger<FactCommandHandler> _logger;
        private readonly AppConfiguration _appConfig;
        private static readonly Dictionary<string, DateTime> _lastUsage = new();
        private static readonly object _lockObject = new();
        private static readonly TimeSpan _cooldown = TimeSpan.FromMinutes(1);
        // Используем потокобезопасный общий генератор случайных чисел
        
        // Статическое хранилище фактов в памяти
        private static List<FactItem>? _cachedFacts = null;
        private static string? _cachedFilePath = null;
        private static DateTime? _lastFileWriteTime = null;
        private static readonly object _factsLockObject = new();

        public FactCommandHandler(
            IExternalLogger<FactCommandHandler> logger,
            IOptions<AppConfiguration> appConfig)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appConfig = appConfig?.Value ?? throw new ArgumentNullException(nameof(appConfig));
        }

        /// <summary>
        /// Загружает факты из файла в память (выполняется один раз)
        /// </summary>
        private async Task<List<FactItem>?> LoadFactsAsync(CancellationToken cancellationToken)
        {
            // Проверяем, нужно ли обновлять кэш
            var shouldReload = false;
            lock (_factsLockObject)
            {
                if (_cachedFacts == null || _cachedFilePath != _appConfig.Facts)
                {
                    shouldReload = true;
                }
                else if (!string.IsNullOrEmpty(_appConfig.Facts) && File.Exists(_appConfig.Facts))
                {
                    var currentFileWriteTime = File.GetLastWriteTimeUtc(_appConfig.Facts);
                    if (_lastFileWriteTime == null || currentFileWriteTime > _lastFileWriteTime)
                    {
                        shouldReload = true;
                    }
                }
            }

            if (!shouldReload)
            {
                _logger.LogDebug(new { 
                    Method = nameof(LoadFactsAsync),
                    Status = "LoadedFromCache",
                    FactsCount = _cachedFacts?.Count ?? 0
                });
                return _cachedFacts;
            }

            // Проверяем существование файла
            if (string.IsNullOrEmpty(_appConfig.Facts) || !File.Exists(_appConfig.Facts))
            {
                _logger.LogError((int)BaseErrorCodes.DataNotFound, new { 
                    Method = nameof(LoadFactsAsync),
                    Status = "FileNotFound",
                    FilePath = _appConfig.Facts
                });
                return null;
            }

            try
            {
                // Читаем все строки из файла
                var allLines = await File.ReadAllLinesAsync(_appConfig.Facts, cancellationToken);
                var factTexts = allLines.Where(line => !string.IsNullOrWhiteSpace(line.Trim())).Select(line => line.Trim()).ToList();
                
                // Создаем FactItem объекты из текстов
                var facts = factTexts.Select(text => new FactItem(text)).ToList();

                // Если есть уже загруженные факты, пытаемся сохранить информацию об использовании
                if (_cachedFacts != null && _cachedFilePath == _appConfig.Facts)
                {
                    // Создаем словарь старых фактов для быстрого поиска
                    var oldFactsDict = _cachedFacts.ToDictionary(f => f.Text, f => f);
                    
                    // Обновляем статус использования для совпадающих фактов
                    foreach (var newFact in facts)
                    {
                        if (oldFactsDict.TryGetValue(newFact.Text, out var oldFact))
                        {
                            newFact.IsUsed = oldFact.IsUsed;
                            newFact.LastUsedAt = oldFact.LastUsedAt;
                        }
                    }
                }

                // Обновляем кэш в потокобезопасном режиме
                lock (_factsLockObject)
                {
                    var isReload = _cachedFacts != null;
                    _cachedFacts = facts;
                    _cachedFilePath = _appConfig.Facts;
                    _lastFileWriteTime = File.GetLastWriteTimeUtc(_appConfig.Facts);
                    
                    _logger.LogInformation(new { 
                        Method = nameof(LoadFactsAsync),
                        Status = isReload ? "FactsReloaded" : "FactsLoadedFirstTime",
                        FilePath = _appConfig.Facts,
                        FactsCount = facts.Count,
                        UsedCount = facts.Count(f => f.IsUsed),
                        AvailableCount = facts.Count(f => !f.IsUsed),
                        FileLastModified = File.GetLastWriteTimeUtc(_appConfig.Facts).ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }

                return facts;
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                    Method = nameof(LoadFactsAsync),
                    Status = "Exception",
                    FilePath = _appConfig.Facts,
                    Error = ex.GetType().Name,
                    Message = ex.Message
                });
                return null;
            }
        }

        /// <summary>
        /// Сбрасывает кэш фактов, заставляя перезагрузить файл при следующем запросе
        /// </summary>
        public static void ClearFactsCache()
        {
            lock (_factsLockObject)
            {
                _cachedFacts = null;
                _cachedFilePath = null;
                _lastFileWriteTime = null;
            }
        }

        /// <summary>
        /// Сбрасывает флаги использования всех фактов
        /// </summary>
        public static void ResetAllFactsUsage()
        {
            lock (_factsLockObject)
            {
                if (_cachedFacts != null)
                {
                    foreach (var fact in _cachedFacts)
                    {
                        fact.ResetUsage();
                    }
                }
            }
        }

        /// <summary>
        /// Получает статистику использования фактов
        /// </summary>
        public static (int total, int used, int available) GetFactsStatistics()
        {
            lock (_factsLockObject)
            {
                if (_cachedFacts == null)
                {
                    return (0, 0, 0);
                }

                var total = _cachedFacts.Count;
                var used = _cachedFacts.Count(f => f.IsUsed);
                var available = total - used;

                return (total, used, available);
            }
        }

        public async Task<LSResponse<string>> Handle(FactCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(new { 
                Method = nameof(Handle),
                UserId = request.UserId
            });

            var result = new LSResponse<string>();

            try
            {
                var cooldownKey = "facts"; // Общий кулдаун для всех пользователей

                // Проверяем кулдаун
                lock (_lockObject)
                {
                    if (_lastUsage.TryGetValue(cooldownKey, out var lastUsage))
                    {
                        var timeSinceLastUse = DateTime.UtcNow - lastUsage;
                        if (timeSinceLastUse < _cooldown)
                        {
                            var remainingCooldown = _cooldown - timeSinceLastUse;
                            var message = $"⏰ Команда !факт доступна через {remainingCooldown.Seconds} секунд";
                            
                            _logger.LogInformation(new { 
                                Method = nameof(Handle),
                                Status = "Cooldown",
                                UserId = request.UserId,
                                RemainingSeconds = remainingCooldown.Seconds
                            });

                            return result.Success(message);
                        }
                    }
                }

                // Загружаем факты из кэша или файла
                var facts = await LoadFactsAsync(cancellationToken);

                if (facts == null)
                {
                    return result.Success("❌ Файл с фактами не найден. Обратитесь к администратору.");
                }

                if (facts.Count == 0)
                {
                    _logger.LogWarning(new { 
                        Method = nameof(Handle),
                        Status = "NoFactsFound",
                        FilePath = _appConfig.Facts
                    });

                    return result.Success("📖 Файл с фактами пуст. Добавьте факты для использования этой команды.");
                }

                // Получаем список неиспользованных фактов
                var availableFacts = facts.Where(f => !f.IsUsed).ToList();

                // Если все факты использованы, сбрасываем флаги
                if (availableFacts.Count == 0)
                {
                    lock (_factsLockObject)
                    {
                        foreach (var fact in facts)
                        {
                            fact.ResetUsage();
                        }
                        availableFacts = facts.ToList();
                    }

                    _logger.LogInformation(new { 
                        Method = nameof(Handle),
                        Status = "AllFactsUsed_Reset",
                        UserId = request.UserId,
                        FactsCount = facts.Count
                    });
                }

                // Выбираем случайный факт из доступных
                var selectedFactItem = availableFacts[Random.Shared.Next(availableFacts.Count)];
                
                // Отмечаем факт как использованный
                selectedFactItem.MarkAsUsed();

                // Обновляем время последнего использования команды
                lock (_lockObject)
                {
                    _lastUsage[cooldownKey] = DateTime.UtcNow;
                }

                var remainingFacts = facts.Count(f => !f.IsUsed);
                _logger.LogInformation(new { 
                    Method = nameof(Handle),
                    Status = "Success",
                    UserId = request.UserId,
                    TotalFactsCount = facts.Count,
                    RemainingFactsCount = remainingFacts,
                    SelectedFact = selectedFactItem.Text,
                    FactMarkedUsedAt = selectedFactItem.LastUsedAt
                });

                return result.Success($"🧠 Интересный факт: {selectedFactItem.Text}");
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                    Method = nameof(Handle),
                    Status = "Exception",
                    UserId = request.UserId,
                    Error = ex.GetType().Name,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace
                });

                return result.Error(BaseErrorCodes.OperationProcessError, "Произошла ошибка при получении факта.");
            }
        }
    }
} 