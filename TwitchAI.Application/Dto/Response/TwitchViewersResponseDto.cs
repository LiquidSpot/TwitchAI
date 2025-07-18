namespace TwitchAI.Application.Dto.Response
{
    /// <summary>
    /// Ответ Twitch API для получения списка зрителей в чате
    /// </summary>
    public class TwitchChattersResponseDto
    {
        /// <summary>
        /// Данные о зрителях
        /// </summary>
        public List<TwitchChatterDto> Data { get; set; } = new();

        /// <summary>
        /// Пагинация
        /// </summary>
        public TwitchPaginationDto? Pagination { get; set; }

        /// <summary>
        /// Общее количество зрителей
        /// </summary>
        public int Total { get; set; }
    }

    /// <summary>
    /// Информация о зрителе в чате
    /// </summary>
    public class TwitchChatterDto
    {
        /// <summary>
        /// ID пользователя
        /// </summary>
        public string User_Id { get; set; } = string.Empty;

        /// <summary>
        /// Логин пользователя
        /// </summary>
        public string User_Login { get; set; } = string.Empty;

        /// <summary>
        /// Отображаемое имя пользователя
        /// </summary>
        public string User_Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Пагинация для Twitch API
    /// </summary>
    public class TwitchPaginationDto
    {
        /// <summary>
        /// Курсор для следующей страницы
        /// </summary>
        public string? Cursor { get; set; }
    }

    /// <summary>
    /// Общий ответ Twitch API
    /// </summary>
    public class TwitchApiResponseDto<T>
    {
        /// <summary>
        /// Данные ответа
        /// </summary>
        public List<T> Data { get; set; } = new();

        /// <summary>
        /// Пагинация
        /// </summary>
        public TwitchPaginationDto? Pagination { get; set; }
    }

    /// <summary>
    /// Информация о стриме
    /// </summary>
    public class TwitchStreamDto
    {
        /// <summary>
        /// ID стрима
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// ID пользователя
        /// </summary>
        public string User_Id { get; set; } = string.Empty;

        /// <summary>
        /// Логин пользователя
        /// </summary>
        public string User_Login { get; set; } = string.Empty;

        /// <summary>
        /// Отображаемое имя пользователя
        /// </summary>
        public string User_Name { get; set; } = string.Empty;

        /// <summary>
        /// Название стрима
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Количество зрителей
        /// </summary>
        public int Viewer_Count { get; set; }

        /// <summary>
        /// Дата начала стрима
        /// </summary>
        public DateTime Started_At { get; set; }

        /// <summary>
        /// Тип стрима
        /// </summary>
        public string Type { get; set; } = "live";
    }

    /// <summary>
    /// Информация о пользователе Twitch
    /// </summary>
    public class TwitchUserDto
    {
        /// <summary>
        /// ID пользователя
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Логин пользователя
        /// </summary>
        public string Login { get; set; } = string.Empty;

        /// <summary>
        /// Отображаемое имя
        /// </summary>
        public string Display_Name { get; set; } = string.Empty;

        /// <summary>
        /// Тип пользователя
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Тип роли
        /// </summary>
        public string Broadcaster_Type { get; set; } = string.Empty;

        /// <summary>
        /// Описание профиля
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// URL аватара
        /// </summary>
        public string Profile_Image_Url { get; set; } = string.Empty;

        /// <summary>
        /// URL офлайн изображения
        /// </summary>
        public string Offline_Image_Url { get; set; } = string.Empty;

        /// <summary>
        /// Количество просмотров
        /// </summary>
        public int View_Count { get; set; }

        /// <summary>
        /// Email пользователя
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Дата создания аккаунта
        /// </summary>
        public DateTime Created_At { get; set; }
    }
} 