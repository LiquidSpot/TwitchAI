using TwitchAI.Domain.Enums;

namespace TwitchAI.Application.Interfaces
{
    /// <summary>
    /// Сервис для управления глобальной ролью бота
    /// </summary>
    public interface IBotRoleService
    {
        /// <summary>
        /// Получить текущую роль бота
        /// </summary>
        Role GetCurrentRole();

        /// <summary>
        /// Установить новую роль бота
        /// </summary>
        /// <param name="newRole">Новая роль</param>
        void SetRole(Role newRole);

        /// <summary>
        /// Проверить, существует ли роль
        /// </summary>
        /// <param name="roleName">Название роли</param>
        /// <param name="role">Найденная роль</param>
        /// <returns>true, если роль существует</returns>
        bool TryGetRole(string roleName, out Role role);
    }
} 