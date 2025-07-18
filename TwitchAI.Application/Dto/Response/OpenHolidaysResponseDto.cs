namespace TwitchAI.Application.Dto.Response
{
    /// <summary>
    /// Праздник из OpenHolidays API
    /// </summary>
    public class OpenHolidayDto
    {
        /// <summary>
        /// Уникальный идентификатор праздника
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Дата начала праздника
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Дата окончания праздника
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Тип праздника
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Названия праздника на разных языках
        /// </summary>
        public List<OpenHolidayNameDto> Name { get; set; } = new();

        /// <summary>
        /// Региональный охват
        /// </summary>
        public string RegionalScope { get; set; } = string.Empty;

        /// <summary>
        /// Временной охват
        /// </summary>
        public string TemporalScope { get; set; } = string.Empty;

        /// <summary>
        /// Является ли праздник общенациональным
        /// </summary>
        public bool Nationwide { get; set; }

        /// <summary>
        /// Получить название праздника на английском языке
        /// </summary>
        public string GetEnglishName()
        {
            return Name?.FirstOrDefault(n => n.Language == "EN")?.Text ?? 
                   Name?.FirstOrDefault()?.Text ?? 
                   "Unknown Holiday";
        }
    }

    /// <summary>
    /// Название праздника на определенном языке
    /// </summary>
    public class OpenHolidayNameDto
    {
        /// <summary>
        /// Код языка
        /// </summary>
        public string Language { get; set; } = string.Empty;

        /// <summary>
        /// Текст названия
        /// </summary>
        public string Text { get; set; } = string.Empty;
    }

    /// <summary>
    /// Информация о стране из OpenHolidays API
    /// </summary>
    public class OpenHolidaysCountryDto
    {
        /// <summary>
        /// Код страны ISO 3166-1 alpha-2
        /// </summary>
        public string IsoCode { get; set; } = string.Empty;

        /// <summary>
        /// Название страны
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Официальные языки
        /// </summary>
        public List<string>? OfficialLanguages { get; set; }
    }

    /// <summary>
    /// Информация о языке из OpenHolidays API
    /// </summary>
    public class OpenHolidaysLanguageDto
    {
        /// <summary>
        /// Код языка ISO 639-1
        /// </summary>
        public string IsoCode { get; set; } = string.Empty;

        /// <summary>
        /// Название языка
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
} 