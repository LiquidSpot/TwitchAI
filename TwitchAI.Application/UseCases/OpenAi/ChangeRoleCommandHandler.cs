using Common.Packages.Logger.Services.Interfaces;
using Common.Packages.Response.Behaviors;
using Common.Packages.Response.Models;
using TwitchAI.Application.Interfaces;
using TwitchAI.Domain.Enums;
using TwitchAI.Domain.Enums.ErrorCodes;

namespace TwitchAI.Application.UseCases.OpenAi
{
    internal class ChangeRoleCommandHandler : ICommandHandler<ChangeRoleCommand, LSResponse<string>>
    {
        private readonly IBotRoleService _botRoleService;
        private readonly IExternalLogger<ChangeRoleCommandHandler> _logger;

        public ChangeRoleCommandHandler(
            IBotRoleService botRoleService,
            IExternalLogger<ChangeRoleCommandHandler> logger)
        {
            _botRoleService = botRoleService ?? throw new ArgumentNullException(nameof(botRoleService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<LSResponse<string>> Handle(ChangeRoleCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(new { 
                Method = nameof(Handle),
                UserId = request.UserId,
                RequestedRole = request.RoleName
            });

            var result = new LSResponse<string>();

            try
            {
                if (_botRoleService.TryGetRole(request.RoleName, out var newRole))
                {
                    var oldRole = _botRoleService.GetCurrentRole();
                    _botRoleService.SetRole(newRole);

                    // Получаем названия ролей в нижнем регистре для отображения в чате
                    var oldRoleName = GetRoleDisplayName(oldRole);
                    var newRoleName = GetRoleDisplayName(newRole);

                    var message = $"✅ Роль бота изменена с {oldRoleName} на {newRoleName}!";
                    
                    // Добавляем эмодзи в зависимости от роли
                    message += newRole switch
                    {
                        Role.bot => " 🤖",
                        Role.neko => " 🐱✨😘💕",
                        Role.Toxic => " 🔥💀❌",
                        Role.Durka => " 🤪🐒😜",
                        _ => " 🎭"
                    };

                    _logger.LogInformation(new { 
                        Method = nameof(Handle),
                        Status = "Success",
                        UserId = request.UserId,
                        OldRole = oldRole,
                        NewRole = newRole,
                        Message = message
                    });

                    return result.Success(message);
                }
                else
                {
                    var availableRoles = string.Join(", ", Enum.GetNames<Role>().Select(GetRoleDisplayName));
                    var errorMessage = $"❌ Неизвестная роль '{request.RoleName}'. Доступные роли: {availableRoles}";
                    
                    _logger.LogWarning(new { 
                        Method = nameof(Handle),
                        Status = "InvalidRole",
                        UserId = request.UserId,
                        RequestedRole = request.RoleName,
                        AvailableRoles = availableRoles
                    });

                    return result.Success(errorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                    Method = nameof(Handle),
                    Status = "Exception",
                    UserId = request.UserId,
                    RequestedRole = request.RoleName,
                    Error = ex.GetType().Name,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace
                });

                return result.Error(BaseErrorCodes.OperationProcessError, "Произошла ошибка при смене роли.");
            }
        }

        /// <summary>
        /// Получает название роли для отображения в чате (в нижнем регистре)
        /// </summary>
        private static string GetRoleDisplayName(Role role)
        {
            return role switch
            {
                Role.bot => "bot",
                Role.neko => "neko",
                Role.Toxic => "toxic",
                Role.Durka => "durka",
                _ => role.ToString().ToLower()
            };
        }

        /// <summary>
        /// Получает название роли для отображения в чате (перегрузка для string)
        /// </summary>
        private static string GetRoleDisplayName(string roleName)
        {
            return roleName.ToLower();
        }
    }
} 