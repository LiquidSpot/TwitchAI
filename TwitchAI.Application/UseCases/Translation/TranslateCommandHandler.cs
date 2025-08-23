using Common.Packages.Logger.Services.Interfaces;
using Common.Packages.Response.Behaviors;
using Common.Packages.Response.Models;
using TwitchAI.Application.Interfaces;
using TwitchAI.Domain.Entites;
using TwitchAI.Domain.Enums.ErrorCodes;

namespace TwitchAI.Application.UseCases.Translation
{
    /// <summary>
    /// Обработчик команды перевода сообщений
    /// </summary>
    internal class TranslateCommandHandler : ICommandHandler<TranslateCommand, LSResponse<string>>
    {
        private readonly IOpenAiService _openAiService;
        private readonly IExternalLogger<TranslateCommandHandler> _logger;

        // Словарь поддерживаемых языков
        private static readonly Dictionary<string, string> SupportedLanguages = new()
        {
            ["ru"] = "русский",
            ["en"] = "английский", 
            ["zh"] = "китайский",
            ["ja"] = "японский",
            ["es"] = "испанский"
        };

        public TranslateCommandHandler(
            IOpenAiService openAiService,
            IExternalLogger<TranslateCommandHandler> logger)
        {
            _openAiService = openAiService ?? throw new ArgumentNullException(nameof(openAiService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<LSResponse<string>> Handle(TranslateCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(new { 
                Method = nameof(Handle),
                UserId = request.UserId,
                Language = request.Language,
                Message = request.Message
            });

            var result = new LSResponse<string>();

            try
            {

                // Проверяем поддерживаемые языки
                if (!SupportedLanguages.TryGetValue(request.Language.ToLower(), out var languageName))
                {
                    var supportedLanguagesList = string.Join(", ", SupportedLanguages.Keys);
                    var errorMessage = $"❌ Неподдерживаемый язык '{request.Language}'. Поддерживаемые языки: {supportedLanguagesList}";
                    
                    _logger.LogWarning(new { 
                        Method = nameof(Handle),
                        Status = "UnsupportedLanguage",
                        UserId = request.UserId,
                        RequestedLanguage = request.Language,
                        SupportedLanguages = supportedLanguagesList
                    });

                    return result.Success(errorMessage);
                }

                // Создаем промпт для OpenAI
                var translationPrompt = $"Переведи на язык {languageName} и адаптируй сообщение: {request.Message}";
                
                // Создаем сообщение для OpenAI
                var userMessage = new UserMessage 
                { 
                    message = translationPrompt,
                    role = Domain.Enums.Role.bot, // Используем роль bot для переводов
                    temp = 0.3, // Низкая температура для более точного перевода
                    maxToken = 300 // Достаточно для перевода
                };

                // Отправляем запрос в OpenAI
                var translationResponse = await _openAiService.GenerateUniversalWithContextAsync(
                    userMessage, 
                    new List<ConversationMessage>(), 
                    ct: cancellationToken);

                if (translationResponse.Status == Common.Packages.Response.Enums.ResponseStatus.Success)
                {
                    var translatedText = translationResponse.Result.Trim();
                    
                    _logger.LogInformation(new { 
                        Method = nameof(Handle),
                        Status = "Success",
                        UserId = request.UserId,
                        OriginalLanguage = request.Language,
                        TargetLanguage = languageName,
                        OriginalMessage = request.Message,
                        TranslatedMessage = translatedText
                    });

                    return result.Success($"{languageName}: {translatedText}");
                }
                else
                {
                    _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                        Method = nameof(Handle),
                        Status = "TranslationError",
                        UserId = request.UserId,
                        Language = request.Language,
                        Message = request.Message,
                        ErrorCode = translationResponse.ErrorCode,
                        ErrorMessage = translationResponse.ErrorObjects
                    });

                    return result.Success("❌ Произошла ошибка при переводе сообщения.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                    Method = nameof(Handle),
                    Status = "Exception",
                    UserId = request.UserId,
                    Language = request.Language,
                    Message = request.Message,
                    Error = ex.GetType().Name,
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace
                });

                return result.Error(BaseErrorCodes.OperationProcessError, "Произошла ошибка при обработке команды перевода.");
            }
        }
    }
} 