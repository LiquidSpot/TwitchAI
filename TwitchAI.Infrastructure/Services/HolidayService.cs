using System.Text.Json;
using Common.Packages.HttpClient.Models;
using Common.Packages.HttpClient.Services.Interfaces;
using Common.Packages.Logger.Services.Interfaces;
using TwitchAI.Application.Constants;
using TwitchAI.Application.Dto.Response;
using TwitchAI.Application.Interfaces;
using TwitchAI.Domain.Entites;
using TwitchAI.Domain.Enums;
using TwitchAI.Domain.Enums.ErrorCodes;

namespace TwitchAI.Infrastructure.Services
{
    /// <summary>
    /// Сервис для работы с праздниками через OpenHolidays API
    /// </summary>
    internal class HolidayService : IHolidayService
    {
        private readonly ILSClientService _httpClient;
        private readonly IExternalLogger<HolidayService> _logger;
        private readonly IOpenAiService _openAiService;

        public HolidayService(
            ILSClientService httpClient,
            IExternalLogger<HolidayService> logger,
            IOpenAiService openAiService)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _openAiService = openAiService ?? throw new ArgumentNullException(nameof(openAiService));
        }

        public async Task<List<OpenHolidayDto>> GetHolidaysAsync(string countryCode, DateTime date, string languageCode = "EN", CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(new { 
                    Method = nameof(GetHolidaysAsync),
                    CountryCode = countryCode,
                    Date = date,
                    LanguageCode = languageCode
                });

                var validFrom = date.ToString("yyyy-MM-dd");
                var validTo = date.ToString("yyyy-MM-dd");

                var url = $"{Constants.OpenHolidaysApis.GetPublicHolidays}?countryIsoCode={countryCode}&languageIsoCode={languageCode}&validFrom={validFrom}&validTo={validTo}";

                var request = new RequestBuilder<object>()
                    .WithUrl(url)
                    .WithMethod(HttpMethod.Get)
                    .WithHeaders(new Dictionary<string, string>
                    {
                        ["Accept"] = "application/json"
                    })
                    .Build();

                var response = await _httpClient.ExecuteRequestAsync<List<OpenHolidayDto>>(request, Constants.OpenHolidaysApiClientKey, cancellationToken);

                if (response.Status == Common.Packages.Response.Enums.ResponseStatus.Success && response.Result != null)
                {
                    _logger.LogInformation(new { 
                        Method = nameof(GetHolidaysAsync),
                        CountryCode = countryCode,
                        Date = date,
                        HolidaysCount = response.Result.Count
                    });

                    return response.Result;
                }
                else
                {
                    _logger.LogError((int)BaseErrorCodes.InternalServerError, new { 
                        Method = nameof(GetHolidaysAsync),
                        Status = "Error getting holidays",
                        CountryCode = countryCode,
                        Date = date,
                        ErrorCode = response.ErrorCode,
                        ErrorMessage = response.ErrorObjects
                    });

                    return new List<OpenHolidayDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BaseErrorCodes.InternalServerError, new { 
                    Method = nameof(GetHolidaysAsync),
                    Error = ex.GetType().Name,
                    Message = ex.Message,
                    CountryCode = countryCode,
                    Date = date
                });

                return new List<OpenHolidayDto>();
            }
        }

        public async Task<string?> GetTodayHolidayTranslatedAsync(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var targetDate = date ?? DateTime.Today;
                _logger.LogInformation(new { 
                    Method = nameof(GetTodayHolidayTranslatedAsync),
                    Date = targetDate
                });

                // Поддерживаемые страны в OpenHolidays API
                var supportedCountries = new[] 
                { 
                    "DE", "FR", "IT", "ES", "AT", "CH", "BE", "NL", "PL", "CZ", 
                    "HU", "RO", "PT", "SE", "HR", "BG", "EE", "LV", "LT", "SI", 
                    "SK", "IE", "MT", "LU", "LI", "MC", "SM", "VA", "BR", "MX", 
                    "ZA", "AD", "AL", "BY", "MD", "RS"
                };

                _logger.LogInformation(new { 
                    Method = nameof(GetTodayHolidayTranslatedAsync),
                    Date = targetDate,
                    Message = "Searching holidays in multiple countries",
                    CountriesCount = supportedCountries.Length
                });

                // Ищем праздники во всех поддерживаемых странах
                var allHolidays = new List<(OpenHolidayDto Holiday, string CountryCode)>();
                var countriesWithHolidays = new HashSet<string>();
                
                foreach (var countryCode in supportedCountries)
                {
                    try
                    {
                        var countryHolidays = await GetHolidaysAsync(countryCode, targetDate, "EN", cancellationToken);
                        
                        // Добавляем праздники с кодом страны
                        foreach (var holidayItem in countryHolidays)
                        {
                            allHolidays.Add((holidayItem, countryCode));
                        }
                        
                        if (countryHolidays.Any())
                        {
                            countriesWithHolidays.Add(countryCode);
                            _logger.LogInformation(new { 
                                Method = nameof(GetTodayHolidayTranslatedAsync),
                                Date = targetDate,
                                CountryCode = countryCode,
                                HolidaysFound = countryHolidays.Count,
                                Message = "Found holidays in country"
                            });
                            
                            // Если найдено 3 страны с праздниками, можем остановиться
                            if (countriesWithHolidays.Count >= 3)
                            {
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(new { 
                            Method = nameof(GetTodayHolidayTranslatedAsync),
                            Date = targetDate,
                            CountryCode = countryCode,
                            Error = ex.GetType().Name,
                            Message = ex.Message
                        });
                        // Продолжаем поиск в других странах
                        continue;
                    }
                }

                // Если найдено 3 или более праздников из разных стран, показываем 3 из них
                if (countriesWithHolidays.Count >= 3)
                {
                    _logger.LogInformation(new { 
                        Method = nameof(GetTodayHolidayTranslatedAsync),
                        Date = targetDate,
                        CountriesWithHolidays = countriesWithHolidays.Count,
                        Message = "Found holidays in 3+ countries, showing 3 holidays"
                    });

                    return await FormatMultipleHolidaysAsync(allHolidays, targetDate, 3, cancellationToken);
                }

                // Если меньше 3 праздников, используем расширенную логику поиска
                if (!allHolidays.Any() || countriesWithHolidays.Count < 3)
                {
                    _logger.LogInformation(new { 
                        Method = nameof(GetTodayHolidayTranslatedAsync),
                        Date = targetDate,
                        CurrentHolidays = allHolidays.Count,
                        CountriesWithHolidays = countriesWithHolidays.Count,
                        Message = "Not enough holidays found for today, searching in nearby dates (±7 days)"
                    });

                    // Ищем праздники в диапазоне ±7 дней
                    for (int dayOffset = 1; dayOffset <= 7; dayOffset++)
                    {
                        // Проверяем будущие даты
                        var futureDate = targetDate.AddDays(dayOffset);
                        var futureHolidays = await SearchHolidaysInCountriesForDate(supportedCountries, futureDate, cancellationToken);
                        if (futureHolidays.Any())
                        {
                            allHolidays.Clear();
                            allHolidays.AddRange(futureHolidays);
                            return await FormatSingleHolidayAsync(allHolidays.First(), futureDate, cancellationToken);
                        }

                        // Проверяем прошедшие даты
                        var pastDate = targetDate.AddDays(-dayOffset);
                        var pastHolidays = await SearchHolidaysInCountriesForDate(supportedCountries, pastDate, cancellationToken);
                        if (pastHolidays.Any())
                        {
                            allHolidays.Clear();
                            allHolidays.AddRange(pastHolidays);
                            return await FormatSingleHolidayAsync(allHolidays.First(), pastDate, cancellationToken);
                        }
                    }

                    if (!allHolidays.Any())
                    {
                        _logger.LogInformation(new { 
                            Method = nameof(GetTodayHolidayTranslatedAsync),
                            Date = targetDate,
                            Message = "No holidays found in any country within ±7 days"
                        });
                        return null;
                    }
                }

                // Если найдено 1-2 праздника на текущую дату, показываем их
                return await FormatMultipleHolidaysAsync(allHolidays, targetDate, allHolidays.Count, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BaseErrorCodes.InternalServerError, new { 
                    Method = nameof(GetTodayHolidayTranslatedAsync),
                    Error = ex.GetType().Name,
                    Message = ex.Message,
                    Date = date
                });

                return null;
            }
        }

        public async Task<List<OpenHolidaysCountryDto>> GetCountriesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(new { 
                    Method = nameof(GetCountriesAsync)
                });

                var url = Constants.OpenHolidaysApis.GetCountries;

                var request = new RequestBuilder<object>()
                    .WithUrl(url)
                    .WithMethod(HttpMethod.Get)
                    .WithHeaders(new Dictionary<string, string>
                    {
                        ["Accept"] = "application/json"
                    })
                    .Build();

                var response = await _httpClient.ExecuteRequestAsync<List<OpenHolidaysCountryDto>>(request, Constants.OpenHolidaysApiClientKey, cancellationToken);

                if (response.Status == Common.Packages.Response.Enums.ResponseStatus.Success && response.Result != null)
                {
                    _logger.LogInformation(new { 
                        Method = nameof(GetCountriesAsync),
                        CountriesCount = response.Result.Count
                    });

                    return response.Result;
                }
                else
                {
                    _logger.LogError((int)BaseErrorCodes.InternalServerError, new { 
                        Method = nameof(GetCountriesAsync),
                        Status = "Error getting countries",
                        ErrorCode = response.ErrorCode,
                        ErrorMessage = response.ErrorObjects
                    });

                    return new List<OpenHolidaysCountryDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BaseErrorCodes.InternalServerError, new { 
                    Method = nameof(GetCountriesAsync),
                    Error = ex.GetType().Name,
                    Message = ex.Message
                });

                return new List<OpenHolidaysCountryDto>();
            }
        }

        public async Task<List<OpenHolidaysLanguageDto>> GetLanguagesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(new { 
                    Method = nameof(GetLanguagesAsync)
                });

                var url = Constants.OpenHolidaysApis.GetLanguages;

                var request = new RequestBuilder<object>()
                    .WithUrl(url)
                    .WithMethod(HttpMethod.Get)
                    .WithHeaders(new Dictionary<string, string>
                    {
                        ["Accept"] = "application/json"
                    })
                    .Build();

                var response = await _httpClient.ExecuteRequestAsync<List<OpenHolidaysLanguageDto>>(request, Constants.OpenHolidaysApiClientKey, cancellationToken);

                if (response.Status == Common.Packages.Response.Enums.ResponseStatus.Success && response.Result != null)
                {
                    _logger.LogInformation(new { 
                        Method = nameof(GetLanguagesAsync),
                        LanguagesCount = response.Result.Count
                    });

                    return response.Result;
                }
                else
                {
                    _logger.LogError((int)BaseErrorCodes.InternalServerError, new { 
                        Method = nameof(GetLanguagesAsync),
                        Status = "Error getting languages",
                        ErrorCode = response.ErrorCode,
                        ErrorMessage = response.ErrorObjects
                    });

                    return new List<OpenHolidaysLanguageDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BaseErrorCodes.InternalServerError, new { 
                    Method = nameof(GetLanguagesAsync),
                    Error = ex.GetType().Name,
                    Message = ex.Message
                });

                return new List<OpenHolidaysLanguageDto>();
            }
        }

        /// <summary>
        /// Переводит текст на русский язык через OpenAI
        /// </summary>
        /// <param name="text">Текст для перевода</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Переведенный текст</returns>
        private static readonly Dictionary<string, string> CountryNames = new()
        {
            ["DE"] = "Германия",
            ["FR"] = "Франция", 
            ["IT"] = "Италия",
            ["ES"] = "Испания",
            ["AT"] = "Австрия",
            ["CH"] = "Швейцария",
            ["BE"] = "Бельгия",
            ["NL"] = "Нидерланды",
            ["PL"] = "Польша",
            ["CZ"] = "Чехия",
            ["HU"] = "Венгрия",
            ["RO"] = "Румыния",
            ["PT"] = "Португалия",
            ["SE"] = "Швеция",
            ["HR"] = "Хорватия",
            ["BG"] = "Болгария",
            ["EE"] = "Эстония",
            ["LV"] = "Латвия",
            ["LT"] = "Литва",
            ["SI"] = "Словения",
            ["SK"] = "Словакия",
            ["IE"] = "Ирландия",
            ["MT"] = "Мальта",
            ["LU"] = "Люксембург",
            ["LI"] = "Лихтенштейн",
            ["MC"] = "Монако",
            ["SM"] = "Сан-Марино",
            ["VA"] = "Ватикан",
            ["BR"] = "Бразилия",
            ["MX"] = "Мексика",
            ["ZA"] = "Южная Африка",
            ["AD"] = "Андорра",
            ["AL"] = "Албания",
            ["BY"] = "Беларусь",
            ["MD"] = "Молдова",
            ["RS"] = "Сербия"
        };

        private static string GetCountryNameInRussian(string countryCode)
        {
            return CountryNames.TryGetValue(countryCode, out var countryName) ? countryName : countryCode;
        }

        private async Task<string?> TranslateToRussianAsync(string text, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(new { 
                    Method = nameof(TranslateToRussianAsync),
                    Text = text
                });

                // Создаем запрос на перевод
                var translationRequest = new UserMessage
                {
                    message = $"Переведи на русский язык следующее сообщение: {text}",
                    role = Role.bot, // Используем роль бота для простого перевода
                    temp = 0.3, // Низкая температура для более точного перевода
                    maxToken = 100 // Короткий ответ
                };

                // Отправляем запрос в OpenAI
                var response = await _openAiService.GenerateUniversalWithContextAsync(
                    translationRequest, 
                    new List<ConversationMessage>(), // Пустой контекст для перевода
                    ct: cancellationToken);

                if (response.Status == Common.Packages.Response.Enums.ResponseStatus.Success && !string.IsNullOrEmpty(response.Result))
                {
                    var translatedText = response.Result.Trim();
                    
                    _logger.LogInformation(new { 
                        Method = nameof(TranslateToRussianAsync),
                        OriginalText = text,
                        TranslatedText = translatedText
                    });

                    return translatedText;
                }
                else
                {
                    _logger.LogError((int)BaseErrorCodes.InternalServerError, new { 
                        Method = nameof(TranslateToRussianAsync),
                        Status = "Error translating text",
                        Text = text,
                        ErrorCode = response.ErrorCode,
                        ErrorMessage = response.ErrorObjects
                    });

                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BaseErrorCodes.InternalServerError, new { 
                    Method = nameof(TranslateToRussianAsync),
                    Error = ex.GetType().Name,
                    Message = ex.Message,
                    Text = text
                });

                return null;
            }
        }

        private async Task<List<(OpenHolidayDto Holiday, string CountryCode)>> SearchHolidaysInCountriesForDate(
            string[] supportedCountries, 
            DateTime searchDate, 
            CancellationToken cancellationToken)
        {
            var holidays = new List<(OpenHolidayDto Holiday, string CountryCode)>();
            
            foreach (var countryCode in supportedCountries)
            {
                try
                {
                    var countryHolidays = await GetHolidaysAsync(countryCode, searchDate, "EN", cancellationToken);
                    
                    // Добавляем праздники с кодом страны
                    foreach (var holidayItem in countryHolidays)
                    {
                        holidays.Add((holidayItem, countryCode));
                    }
                    
                    // Если нашли праздники, прерываем поиск для оптимизации
                    if (countryHolidays.Any())
                    {
                        _logger.LogInformation(new { 
                            Method = nameof(SearchHolidaysInCountriesForDate),
                            Date = searchDate,
                            CountryCode = countryCode,
                            HolidaysFound = countryHolidays.Count,
                            Message = "Found holidays in country"
                        });
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(new { 
                        Method = nameof(SearchHolidaysInCountriesForDate),
                        Date = searchDate,
                        CountryCode = countryCode,
                        Error = ex.GetType().Name,
                        Message = ex.Message
                    });
                    // Продолжаем поиск в других странах
                    continue;
                }
            }

            return holidays;
        }

        private async Task<string> FormatMultipleHolidaysAsync(
            List<(OpenHolidayDto Holiday, string CountryCode)> holidays, 
            DateTime targetDate, 
            int maxCount, 
            CancellationToken cancellationToken)
        {
            var uniqueCountries = new HashSet<string>();
            var selectedHolidays = new List<(OpenHolidayDto Holiday, string CountryCode)>();
            
            // Выбираем праздники из разных стран
            foreach (var (holiday, countryCode) in holidays)
            {
                if (!uniqueCountries.Contains(countryCode) && selectedHolidays.Count < maxCount)
                {
                    uniqueCountries.Add(countryCode);
                    selectedHolidays.Add((holiday, countryCode));
                }
            }

            var holidayTexts = new List<string>();
            
            foreach (var (holiday, countryCode) in selectedHolidays)
            {
                var holidayName = holiday.GetEnglishName();
                var translatedHoliday = await TranslateToRussianAsync(holidayName, cancellationToken);
                
                var finalHolidayName = !string.IsNullOrEmpty(translatedHoliday) ? translatedHoliday : holidayName;
                var countryName = GetCountryNameInRussian(countryCode);
                
                holidayTexts.Add($"{finalHolidayName} ({countryName})");
            }

            var dateInfo = targetDate.Date == DateTime.Today ? "сегодня" : targetDate.ToString("dd.MM.yyyy");
            var emoji = selectedHolidays.Count > 1 ? "🎉" : "🎊";
            
            if (selectedHolidays.Count == 1)
            {
                return $"{emoji} Праздник дня ({dateInfo}): {holidayTexts[0]}";
            }
            else
            {
                return $"{emoji} Праздники дня ({dateInfo}): {string.Join(", ", holidayTexts)}";
            }
        }

        private async Task<string> FormatSingleHolidayAsync(
            (OpenHolidayDto Holiday, string CountryCode) holiday, 
            DateTime targetDate, 
            CancellationToken cancellationToken)
        {
            var (holidayDto, countryCode) = holiday;
            var holidayName = holidayDto.GetEnglishName();
            
            _logger.LogInformation(new { 
                Method = nameof(FormatSingleHolidayAsync),
                Date = targetDate,
                HolidayName = holidayName,
                CountryCode = countryCode,
                Message = "Found holiday, translating to Russian"
            });

            // Переводим через OpenAI
            var translatedHoliday = await TranslateToRussianAsync(holidayName, cancellationToken);

            if (!string.IsNullOrEmpty(translatedHoliday))
            {
                _logger.LogInformation(new { 
                    Method = nameof(FormatSingleHolidayAsync),
                    Date = targetDate,
                    OriginalHoliday = holidayName,
                    TranslatedHoliday = translatedHoliday,
                    CountryCode = countryCode
                });

                var dateInfo = targetDate.Date == DateTime.Today ? "сегодня" : targetDate.ToString("dd.MM.yyyy");
                return $"🎉 Праздник дня ({dateInfo}): {translatedHoliday} ({GetCountryNameInRussian(countryCode)})";
            }
            else
            {
                // Если перевод не удался, возвращаем оригинальное название
                _logger.LogWarning(new { 
                    Method = nameof(FormatSingleHolidayAsync),
                    Date = targetDate,
                    Message = "Translation failed, returning original name",
                    HolidayName = holidayName
                });

                var dateInfo = targetDate.Date == DateTime.Today ? "сегодня" : targetDate.ToString("dd.MM.yyyy");
                return $"🎉 Праздник дня ({dateInfo}): {holidayName} ({GetCountryNameInRussian(countryCode)})";
            }
        }
    }
} 