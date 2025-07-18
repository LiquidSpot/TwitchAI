using TwitchAI.Domain.Entites;

namespace TwitchAI.Application.Interfaces
{
    /// <summary>
    /// Сервис для мониторинга зрителей в чате
    /// </summary>
    public interface IViewerMonitoringService
    {
        /// <summary>
        /// Получает список текущих зрителей в чате
        /// </summary>
        /// <param name="channelName">Название канала</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список имен зрителей</returns>
        Task<List<string>> GetCurrentViewersAsync(string channelName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Обновляет информацию о присутствии зрителей
        /// </summary>
        /// <param name="channelName">Название канала</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Количество обновленных зрителей</returns>
        Task<int> UpdateViewerPresenceAsync(string channelName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получает список зрителей, которые присутствуют в чате, но не писали сообщения
        /// </summary>
        /// <param name="channelName">Название канала</param>
        /// <param name="timeSpan">Период времени для поиска</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список пользователей-зрителей</returns>
        Task<List<TwitchUser>> GetSilentViewersAsync(string channelName, TimeSpan timeSpan, CancellationToken cancellationToken = default);

        /// <summary>
        /// Отмечает зрителя как активного (присутствующего в чате)
        /// </summary>
        /// <param name="username">Имя пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Информация о зрителе</returns>
        Task<TwitchUser> MarkViewerAsActiveAsync(string username, CancellationToken cancellationToken = default);

        /// <summary>
        /// Запускает мониторинг зрителей
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        Task StartMonitoringAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Останавливает мониторинг зрителей
        /// </summary>
        Task StopMonitoringAsync();
    }
} 