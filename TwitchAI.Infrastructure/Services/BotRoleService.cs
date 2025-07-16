using TwitchAI.Application.Interfaces;
using TwitchAI.Domain.Enums;

namespace TwitchAI.Infrastructure.Services
{
    /// <summary>
    /// Сервис для управления глобальной ролью бота
    /// </summary>
    internal class BotRoleService : IBotRoleService
    {
        private Role _currentRole = Role.bot;
        private readonly object _lock = new object();

        public Role GetCurrentRole()
        {
            lock (_lock)
            {
                return _currentRole;
            }
        }

        public void SetRole(Role newRole)
        {
            lock (_lock)
            {
                _currentRole = newRole;
            }
        }

        public bool TryGetRole(string roleName, out Role role)
        {
            return Enum.TryParse(roleName, true, out role);
        }
    }
} 