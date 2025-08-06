namespace TwitchAI.Application.Interfaces;

public interface IReplyLimitService
{
    /// <summary>
    /// Получить текущий лимит цепочки reply для пользователя
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Лимит (по умолчанию 3)</returns>
    Task<int> GetReplyLimitAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Установить лимит цепочки reply для пользователя
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    /// <param name="limit">Новый лимит</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task SetReplyLimitAsync(Guid userId, int limit, CancellationToken cancellationToken = default);
}