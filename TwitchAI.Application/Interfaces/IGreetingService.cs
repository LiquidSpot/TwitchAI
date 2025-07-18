using TwitchAI.Domain.Entites;

namespace TwitchAI.Application.Interfaces
{
    /// <summary>
    /// Сервис для генерации приветственных сообщений для новых пользователей
    /// </summary>
    public interface IGreetingService
    {
        /// <summary>
        /// Генерирует случайное приветственное сообщение для нового пользователя
        /// </summary>
        /// <param name="user">Пользователь, для которого генерируется приветствие</param>
        /// <returns>Приветственное сообщение</returns>
        string GenerateGreeting(TwitchUser user);

        /// <summary>
        /// Проверяет, нужно ли здороваться с пользователем
        /// </summary>
        /// <param name="user">Пользователь для проверки</param>
        /// <param name="wasUserCreated">Был ли пользователь только что создан</param>
        /// <returns>True если нужно поздороваться</returns>
        bool ShouldGreetUser(TwitchUser user, bool wasUserCreated);
    }
} 