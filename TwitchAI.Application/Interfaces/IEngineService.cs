namespace TwitchAI.Application.Interfaces;

public interface IEngineService
{
    /// <summary>
    /// Получить текущий движок OpenAI для пользователя
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Название движка (по умолчанию из конфигурации)</returns>
    Task<string> GetEngineAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Установить движок OpenAI для пользователя
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    /// <param name="engineName">Название движка</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task SetEngineAsync(Guid userId, string engineName, CancellationToken cancellationToken = default);
}