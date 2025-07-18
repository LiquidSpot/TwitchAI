using TwitchAI.Application.Dto.Response;

namespace TwitchAI.Application.Interfaces
{
    /// <summary>
    /// Сервис для работы с праздниками
    /// </summary>
    public interface IHolidayService
    {
        /// <summary>
        /// Получить праздники для конкретной страны на определенную дату
        /// </summary>
        /// <param name="countryCode">Код страны (ISO 3166-1 alpha-2)</param>
        /// <param name="date">Дата для поиска праздников</param>
        /// <param name="languageCode">Код языка (ISO 639-1)</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список праздников</returns>
        Task<List<OpenHolidayDto>> GetHolidaysAsync(string countryCode, DateTime date, string languageCode = "EN", CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить праздник дня для России с переводом на русский язык
        /// </summary>
        /// <param name="date">Дата для поиска праздника (по умолчанию - сегодня)</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Переведенное название праздника или null если праздников нет</returns>
        Task<string?> GetTodayHolidayTranslatedAsync(DateTime? date = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить список доступных стран
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список стран</returns>
        Task<List<OpenHolidaysCountryDto>> GetCountriesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить список доступных языков
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список языков</returns>
        Task<List<OpenHolidaysLanguageDto>> GetLanguagesAsync(CancellationToken cancellationToken = default);
    }
} 