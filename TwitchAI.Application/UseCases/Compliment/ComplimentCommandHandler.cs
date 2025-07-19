using Common.Packages.Logger.Services.Interfaces;
using Common.Packages.Response.Behaviors;
using Common.Packages.Response.Models;
using TwitchAI.Application.Interfaces;
using TwitchAI.Domain.Entites;
using TwitchAI.Domain.Enums.ErrorCodes;

namespace TwitchAI.Application.UseCases.Compliment
{
    /// <summary>
    /// Обработчик команды для генерации комплиментов
    /// </summary>
    internal class ComplimentCommandHandler : ICommandHandler<ComplimentCommand, LSResponse<string>>
    {
        private readonly IOpenAiService _openAiService;
        private readonly ITwitchUserService _twitchUserService;
        private readonly IExternalLogger<ComplimentCommandHandler> _logger;
        private static readonly Random _random = new();
        
        // Резервные комплименты на случай сбоя OpenAI
        private static readonly string[] BackupCompliments = 
        {
            "Ты просто космос! 🚀✨",
            "У тебя аура главного героя! 🌟",
            "Ты как WiFi - без тебя жизнь не та! 📶💕",
            "Ты круче чем пицца в пятницу! 🍕😎",
            "У тебя энергия как у солнца! ☀️🔥"
        };

        public ComplimentCommandHandler(
            IOpenAiService openAiService,
            ITwitchUserService twitchUserService,
            IExternalLogger<ComplimentCommandHandler> logger)
        {
            _openAiService = openAiService ?? throw new ArgumentNullException(nameof(openAiService));
            _twitchUserService = twitchUserService ?? throw new ArgumentNullException(nameof(twitchUserService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<LSResponse<string>> Handle(ComplimentCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(new { 
                Method = nameof(Handle),
                UserId = request.UserId,
                TargetUsername = request.TargetUsername
            });

            try
            {
                var result = new LSResponse<string>();
                string targetName;
                
                // Определяем цель комплимента
                if (string.IsNullOrWhiteSpace(request.TargetUsername))
                {
                    // Комплимент автору команды
                    var user = await _twitchUserService.GetUserByIdAsync(request.UserId, cancellationToken);
                    if (user == null)
                    {
                        return result.Success("❌ Не удалось найти пользователя для комплимента.");
                    }
                    targetName = user.DisplayName;
                }
                else
                {
                    // Убираем @ если он есть
                    targetName = request.TargetUsername.TrimStart('@');
                }

                // Всегда используем OpenAI для генерации веселых комплиментов
                var compliment = await GenerateAIComplimentAsync(targetName, cancellationToken);
                
                // Если AI не смог сгенерировать, используем резервный комплимент
                if (string.IsNullOrEmpty(compliment))
                {
                    compliment = GetRandomBackupCompliment();
                    
                    _logger.LogWarning(new { 
                        Method = nameof(Handle),
                        Status = "AI_Failed_Using_Backup",
                        UserId = request.UserId,
                        TargetName = targetName
                    });
                }

                var message = $"💕 {targetName}, {compliment}";

                _logger.LogInformation(new { 
                    Method = nameof(Handle),
                    Status = "Success",
                    UserId = request.UserId,
                    TargetUsername = targetName,
                    Compliment = compliment
                });

                return result.Success(message);
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                    Method = nameof(Handle),
                    Status = "Exception",
                    UserId = request.UserId,
                    TargetUsername = request.TargetUsername,
                    Error = ex.GetType().Name,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace
                });

                return new LSResponse<string>().Error(BaseErrorCodes.OperationProcessError, "Произошла ошибка при генерации комплимента.");
            }
        }

        /// <summary>
        /// Генерирует комплимент с помощью OpenAI
        /// </summary>
        private async Task<string?> GenerateAIComplimentAsync(string targetName, CancellationToken cancellationToken)
        {
            try
            {
                var prompt = $"Создай веселый, креативный и остроумный комплимент для пользователя по имени {targetName}. " +
                            "Комплимент должен быть смешным, неожиданным и запоминающимся! Используй интернет-сленг, мемы, " +
                            "сравнения с едой, играми, фильмами или забавные метафоры. Не более 100 символов. " +
                            "Не используй имя в комплименте, только сам комплимент с яркими эмодзи в конце! " +
                            "Примеры стиля: 'Ты как редкий лут в игре - все хотят тебя получить!', " +
                            "'У тебя больше харизмы чем у главного героя аниме!'";
                
                var userMessage = new UserMessage 
                { 
                    message = prompt,
                    role = Domain.Enums.Role.neko, // Используем роль neko для более игривых комплиментов
                    temp = 0.6, // Максимальная креативность
                    maxToken = 150 // Больше токенов для креативности
                };

                var response = await _openAiService.GenerateUniversalWithContextAsync(
                    userMessage, 
                    new List<ConversationMessage>(), 
                    ct: cancellationToken);

                if (response.Status == Common.Packages.Response.Enums.ResponseStatus.Success)
                {
                    return response.Result?.Trim();
                }

                _logger.LogWarning(new { 
                    Method = nameof(GenerateAIComplimentAsync),
                    Status = "AI_Generation_Failed",
                    TargetName = targetName,
                    ErrorCode = response.ErrorCode
                });

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                    Method = nameof(GenerateAIComplimentAsync),
                    Status = "Exception",
                    TargetName = targetName,
                    Error = ex.GetType().Name,
                    Message = ex.Message
                });

                return null;
            }
        }

        /// <summary>
        /// Получает случайный резервный комплимент
        /// </summary>
        private static string GetRandomBackupCompliment()
        {
            return BackupCompliments[_random.Next(BackupCompliments.Length)];
        }
    }
} 