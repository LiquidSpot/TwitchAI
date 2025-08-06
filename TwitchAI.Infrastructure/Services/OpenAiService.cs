using Common.Packages.Extensions.Extensions;
using Common.Packages.HttpClient.Models;
using Common.Packages.HttpClient.Services.Interfaces;
using Common.Packages.Logger.Services.Interfaces;
using Common.Packages.Response.Models;
using Microsoft.Extensions.Options;
using System.Net;
using TwitchAI.Application.Constants;
using TwitchAI.Application.Dto.Request;
using TwitchAI.Application.Dto.Response;
using TwitchAI.Application.Interfaces;
using TwitchAI.Application.Models;
using TwitchAI.Domain.Entites;
using TwitchAI.Domain.Enums;
using TwitchAI.Domain.Enums.ErrorCodes;
#pragma warning disable CS0649

namespace TwitchAI.Infrastructure.Services;

internal class OpenAiService : IOpenAiService
{
    private readonly ILSClientService _httpClient;
    private readonly IExternalLogger<OpenAiService> _logger;
    private readonly IEngineService _engineService;
    private readonly IOptions<AppConfiguration> _appConfig;
    // Настройки берутся из конфигурации
    private readonly string _model;
    private readonly int _maxTokens;
    private readonly double _temp;
    private RequestBuilder<object> _builder;

    public OpenAiService(ILSClientService httpClient, IExternalLogger<OpenAiService> logger, IEngineService engineService, IOptions<AppConfiguration> appConfig)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _engineService = engineService ?? throw new ArgumentNullException(nameof(engineService));
        _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));

        // Инициализируем настройки из конфигурации
        _model = _appConfig.Value.OpenAi.Model;
        _maxTokens = _appConfig.Value.OpenAi.MaxTokens;
        _temp = _appConfig.Value.OpenAi.Temperature;

        _builder = new RequestBuilder<object>()
           .WithHeaders(new Dictionary<string, string>
        {
            ["OpenAI-Organization"] = _appConfig.Value.OpenAi.OrganizationId,
            ["OpenAI-Project"] = _appConfig.Value.OpenAi.ProjectId
        });
    }



    public async Task<LSResponse<string>> GenerateUniversalWithContextAsync(UserMessage message, List<ConversationMessage> conversationContext, OpenAiApiVersion? apiVersion = null, CancellationToken cancellationToken = default)
    {
        var selectedApiVersion = apiVersion ?? _appConfig.Value.OpenAiApiVersion;

        _logger.LogInformation(new { 
            Method = nameof(GenerateUniversalWithContextAsync),
            SelectedApiVersion = selectedApiVersion,
            Message = message.message,
            Role = message.role,
            MaxTokens = _maxTokens,
            ContextMessagesCount = conversationContext.Count
        });

        return selectedApiVersion switch
        {
            OpenAiApiVersion.Responses => await GenerateUniversalFromResponsesApiWithContextAndTokens(message, conversationContext, _maxTokens, cancellationToken),
            OpenAiApiVersion.ChatCompletions => await GenerateUniversalFromChatCompletionsApiWithContextAndTokens(message, conversationContext, _maxTokens, cancellationToken),
            _ => await GenerateUniversalFromResponsesApiWithContextAndTokens(message, conversationContext, _maxTokens, cancellationToken) // По умолчанию используем новый API
        };
    }

    public async Task<LSResponse<string>> GenerateUniversalWithContextAsync(UserMessage message, List<ConversationMessage> conversationContext, Guid userId, OpenAiApiVersion? apiVersion = null, CancellationToken cancellationToken = default)
    {
        var selectedApiVersion = apiVersion ?? _appConfig.Value.OpenAiApiVersion;
        
        // Получаем персональный движок пользователя
        var userEngine = await _engineService.GetEngineAsync(userId, cancellationToken);

        _logger.LogInformation(new { 
            Method = nameof(GenerateUniversalWithContextAsync),
            SelectedApiVersion = selectedApiVersion,
            Message = message.message,
            Role = message.role,
            MaxTokens = _maxTokens,
            ContextMessagesCount = conversationContext.Count,
            UserId = userId,
            UserEngine = userEngine
        });

        return selectedApiVersion switch
        {
            OpenAiApiVersion.Responses => await GenerateUniversalFromResponsesApiWithContextAndTokensAndEngine(message, conversationContext, _maxTokens, userEngine, cancellationToken),
            OpenAiApiVersion.ChatCompletions => await GenerateUniversalFromChatCompletionsApiWithContextAndTokensAndEngine(message, conversationContext, _maxTokens, userEngine, cancellationToken),
            _ => await GenerateUniversalFromResponsesApiWithContextAndTokensAndEngine(message, conversationContext, _maxTokens, userEngine, cancellationToken) // По умолчанию используем новый API
        };
    }

    /// <summary>
    /// Универсальный метод для получения ответа от Responses API с контекстом и настраиваемым количеством токенов
    /// </summary>
    private async Task<LSResponse<string>> GenerateUniversalFromResponsesApiWithContextAndTokens(UserMessage message, List<ConversationMessage> conversationContext, int maxTokens, CancellationToken cancellationToken)
    {
        var result = new LSResponse<string>();
        var response = await GenerateResponseWithContextAsync(message, conversationContext, maxTokens, cancellationToken);
        
        if (response.Status != Common.Packages.Response.Enums.ResponseStatus.Success || response.Result == null)
        {
            return result.From(response);
        }

        // Извлекаем текст из output массива
        var content = response.Result.output?.FirstOrDefault(o => o.type == "message")
                              ?.content?.FirstOrDefault(c => c.type == "output_text")
                              ?.text?.Trim();

        // Проверяем статус ответа
        if (response.Result.status == "incomplete")
        {
            _logger.LogWarning(new { 
                Method = nameof(GenerateUniversalFromResponsesApiWithContextAndTokens),
                Status = "Incomplete response",
                Reason = response.Result.incomplete_details?.ToString(),
                ResponseId = response.Result.id,
                ContextMessagesCount = conversationContext.Count,
                HasContent = !string.IsNullOrWhiteSpace(content)
            });
            
            // Если есть хоть какой-то текст, возвращаем его как успешный результат
            if (!string.IsNullOrWhiteSpace(content))
            {
                _logger.LogInformation(new { 
                    Method = nameof(GenerateUniversalFromResponsesApiWithContextAndTokens),
                    Status = "Returning partial content from incomplete response",
                    ResponseId = response.Result.id,
                    ContentLength = content.Length,
                    ContextMessagesCount = conversationContext.Count
                });
                
                return result.Success(content);
            }
            
            // Если нет текста, возвращаем ошибку
            return result.Error(OpenAiErrorCodes.EmptyResponse, "Получен неполный ответ от OpenAI без текста.");
        }

        // Если нет основного сообщения, но есть reasoning, попробуем использовать его
        if (string.IsNullOrWhiteSpace(content))
        {
            var reasoningOutput = response.Result.output?.FirstOrDefault(o => o.type == "reasoning");
            if (reasoningOutput != null)
            {
                _logger.LogInformation(new { 
                    Method = nameof(GenerateUniversalFromResponsesApiWithContextAndTokens),
                    Status = "No main message, found reasoning output",
                    ResponseId = response.Result.id,
                    ContextMessagesCount = conversationContext.Count
                });
                
                return result.Error(OpenAiErrorCodes.EmptyResponse, "Модель обработала запрос, но не смогла сформулировать текстовый ответ.");
            }
            
            return result.Error(OpenAiErrorCodes.EmptyResponse, "Получен пустой ответ от OpenAI.");
        }

        return result.Success(content);
    }

    /// <summary>
    /// Универсальный метод для получения ответа от Chat Completions API с контекстом и настраиваемым количеством токенов
    /// </summary>
    private async Task<LSResponse<string>> GenerateUniversalFromChatCompletionsApiWithContextAndTokens(UserMessage message, List<ConversationMessage> conversationContext, int maxTokens, CancellationToken cancellationToken)
    {
        var result = new LSResponse<string>();
        var response = await GenerateWithContextAsync(message, conversationContext, maxTokens, cancellationToken);
        
        if (response.Status != Common.Packages.Response.Enums.ResponseStatus.Success || response.Result == null)
        {
            return result.From(response);
        }

        var content = response.Result.choices?.FirstOrDefault()?.message?.content?.Trim();
        if (string.IsNullOrWhiteSpace(content))
        {
            return result.Error(OpenAiErrorCodes.EmptyResponse);
        }

        return result.Success(content);
    }


    /// <summary>
    /// Генерация ответа с использованием нового Responses API с контекстом
    /// </summary>
    public async Task<LSResponse<ResponsesApiResponseDto?>> GenerateResponseWithContextAsync(UserMessage message, List<ConversationMessage> conversationContext, int maxTokens, CancellationToken cancellationToken = default)
    {
        var result = new LSResponse<ResponsesApiResponseDto?>();
        try
        {
            _logger.LogInformation(new { 
                Method = nameof(GenerateResponseWithContextAsync),
                ApiType = "Responses API",
                Message = message.message, 
                Role = message.role,
                MaxTokens = maxTokens,
                IsCustomTokens = maxTokens != _maxTokens,
                Temperature = message.temp,
                Model = _model,
                ContextMessagesCount = conversationContext.Count
            });

            var body = new ResponsesRequestDto()
            {
                model = _model,
                input = BuildInputForResponsesApiWithContext(message, conversationContext),
                max_output_tokens = maxTokens,
                store = false // По умолчанию не сохраняем для простоты
            };

            // Добавляем temperature только если модель его поддерживает
            // o4-mini и другие reasoning модели не поддерживают temperature
            if (!_model.StartsWith("o4") && !_model.StartsWith("o3") && !_model.StartsWith("o1"))
            {
                body.temperature = message.temp > 0 ? message.temp : _temp;
            }

            var url = Constants.OpenApiApis.responses;

            var request = _builder
                         .WithUrl(url)
                         .WithMethod(HttpMethod.Post)
                         .WithBody(body)
                         .Build();

            var response = await _httpClient.ExecuteRequestAsync<ResponsesApiResponseDto?>(request, Constants.OpenAiClientKey, cancellationToken).ConfigureAwait(false);

            if (response.Status == Common.Packages.Response.Enums.ResponseStatus.Success)
            {
                _logger.LogInformation(new { 
                    Method = nameof(GenerateResponseWithContextAsync),
                    ApiType = "Responses API",
                    Status = "Success",
                    ResponseId = response.Result?.id,
                    Model = response.Result?.model,
                    TokensUsed = response.Result?.usage?.total_tokens,
                    MaxTokensUsed = maxTokens,
                    OutputText = response.Result?.output?.FirstOrDefault(o => o.type == "message")
                                      ?.content?.FirstOrDefault(c => c.type == "output_text")
                                      ?.text
                });
            }
            else
            {
                _logger.LogError((int)OpenAiErrorCodes.ApiCallError, new { 
                    Method = nameof(GenerateResponseWithContextAsync),
                    ApiType = "Responses API",
                    Status = "Error",
                    ErrorCode = response.ErrorCode,
                    Message = response.ErrorObjects
                });
            }

            return response;
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError((int)OpenAiErrorCodes.ApiCallError, new { 
                Method = nameof(GenerateResponseWithContextAsync),
                ApiType = "Responses API",
                Error = "HttpRequestException",
                Message = httpEx.Message,
                StatusCode = httpEx.Data["StatusCode"]?.ToString()
            });

            return MapHttpErrorToResponse<ResponsesApiResponseDto?>(httpEx);
        }
        catch (TaskCanceledException tcEx) when (tcEx.CancellationToken == cancellationToken)
        {
            _logger.LogError((int)OpenAiErrorCodes.ApiTimeout, new { 
                Method = nameof(GenerateResponseWithContextAsync),
                ApiType = "Responses API",
                Error = "RequestCancelled",
                Message = tcEx.Message
            });

            return result.Error(OpenAiErrorCodes.ApiTimeout);
        }
        catch (Exception ex)
        {
            _logger.LogError((int)OpenAiErrorCodes.ApiCallError, new { 
                Method = nameof(GenerateResponseWithContextAsync),
                ApiType = "Responses API",
                Error = ex.GetType().Name,
                Message = ex.Message,
                StackTrace = ex.StackTrace
            });

            return result.Error(OpenAiErrorCodes.ApiCallError);
        }
    }

    /// <summary>
    /// Генерация ответа с использованием старого Chat Completions API с контекстом
    /// </summary>
    public async Task<LSResponse<TextCompletionDto?>> GenerateWithContextAsync(UserMessage message, List<ConversationMessage> conversationContext, int maxTokens, CancellationToken cancellationToken = default)
    {
        var result = new LSResponse<TextCompletionDto?>();
        try
        {
            _logger.LogInformation(new { 
                Method = nameof(GenerateWithContextAsync),
                ApiType = "Chat Completions API",
                Message = message.message, 
                Role = message.role,
                MaxTokens = maxTokens,
                IsCustomTokens = maxTokens != _maxTokens,
                Temperature = message.temp,
                Model = _model,
                ContextMessagesCount = conversationContext.Count
            });

            var body = new RequestModelDto()
            {
                model = _model,
                max_tokens = maxTokens,
                messages = BuildMessagesWithContext(message, conversationContext)
            };

            // Добавляем temperature только если модель его поддерживает
            // o4-mini и другие reasoning модели не поддерживают temperature
            if (!_model.StartsWith("o4") && !_model.StartsWith("o3") && !_model.StartsWith("o1"))
            {
                body.temperature = message.temp > 0 ? message.temp : _temp;
            }

            var url = Constants.OpenApiApis.completions;

            var request = _builder
                         .WithUrl(url)
                         .WithMethod(HttpMethod.Post)
                         .WithBody(body)
                         .Build();

            var response = await _httpClient.ExecuteRequestAsync<TextCompletionDto?>(request, Constants.OpenAiClientKey, cancellationToken).ConfigureAwait(false);

            if (response.Status == Common.Packages.Response.Enums.ResponseStatus.Success)
            {
                _logger.LogInformation(new { 
                    Method = nameof(GenerateWithContextAsync),
                    ApiType = "Chat Completions API",
                    Status = "Success",
                    ResponseId = response.Result?.id,
                    Model = response.Result?.model,
                    TokensUsed = response.Result?.usage?.total_tokens,
                    MaxTokensUsed = maxTokens
                });
            }
            else
            {
                _logger.LogError((int)OpenAiErrorCodes.ApiCallError, new { 
                    Method = nameof(GenerateWithContextAsync),
                    ApiType = "Chat Completions API",
                    Status = "Error",
                    ErrorCode = response.ErrorCode,
                    Message = response.ErrorObjects
                });
            }

            return response;
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError((int)OpenAiErrorCodes.ApiCallError, new { 
                Method = nameof(GenerateWithContextAsync),
                ApiType = "Chat Completions API",
                Error = "HttpRequestException",
                Message = httpEx.Message,
                StatusCode = httpEx.Data["StatusCode"]?.ToString()
            });

            return MapHttpErrorToResponse<TextCompletionDto?>(httpEx);
        }
        catch (TaskCanceledException tcEx) when (tcEx.CancellationToken == cancellationToken)
        {
            _logger.LogError((int)OpenAiErrorCodes.ApiTimeout, new { 
                Method = nameof(GenerateWithContextAsync),
                ApiType = "Chat Completions API",
                Error = "RequestCancelled",
                Message = tcEx.Message
            });

            return result.Error(OpenAiErrorCodes.ApiTimeout);
        }
        catch (Exception ex)
        {
            _logger.LogError((int)OpenAiErrorCodes.ApiCallError, new { 
                Method = nameof(GenerateWithContextAsync),
                ApiType = "Chat Completions API",
                Error = ex.GetType().Name,
                Message = ex.Message,
                StackTrace = ex.StackTrace
            });

            return result.Error(OpenAiErrorCodes.ApiCallError);
        }
    }

    /// <summary>
    /// Строит массив сообщений с контекстом для Chat Completions API
    /// </summary>
    private MessageGptDto[] BuildMessagesWithContext(UserMessage message, List<ConversationMessage> conversationContext)
    {
        var messages = new List<MessageGptDto>();

        // Добавляем системный промпт
        messages.Add(new MessageGptDto { role = "system", content = message.role.GetDescription() });

        // Добавляем контекст диалога
        foreach (var contextMessage in conversationContext)
        {
            messages.Add(new MessageGptDto { role = contextMessage.Role, content = contextMessage.Content });
        }

        // Добавляем текущее сообщение пользователя
        messages.Add(new MessageGptDto { role = "user", content = message.message });

        return messages.ToArray();
    }

    /// <summary>
    /// Строит входные данные с контекстом для Responses API
    /// </summary>
    private object BuildInputForResponsesApiWithContext(UserMessage message, List<ConversationMessage> conversationContext)
    {
        var inputMessages = new List<object>();

        // Добавляем системный промпт
        if (!string.IsNullOrEmpty(message.role.GetDescription()))
        {
            inputMessages.Add(new 
            { 
                role = "system", 
                content = new[] 
                { 
                    new { type = "input_text", text = message.role.GetDescription() } 
                } 
            });
        }

        // Добавляем контекст диалога
        foreach (var contextMessage in conversationContext)
        {
            // Для assistant сообщений используем output_text, для user - input_text
            var contentType = contextMessage.Role == "assistant" ? "output_text" : "input_text";
            
            inputMessages.Add(new 
            { 
                role = contextMessage.Role, 
                content = new[] 
                { 
                    new { type = contentType, text = contextMessage.Content } 
                } 
            });
        }

        // Добавляем текущее сообщение пользователя
        inputMessages.Add(new 
        { 
            role = "user", 
            content = new[] 
            { 
                new { type = "input_text", text = message.message } 
            } 
        });

        return inputMessages.ToArray();
    }

    /// <summary>
    /// Универсальный метод для получения ответа от Responses API с контекстом, настраиваемым количеством токенов и движком
    /// </summary>
    private async Task<LSResponse<string>> GenerateUniversalFromResponsesApiWithContextAndTokensAndEngine(UserMessage message, List<ConversationMessage> conversationContext, int maxTokens, string engine, CancellationToken cancellationToken)
    {
        var result = new LSResponse<string>();
        var response = await GenerateResponseWithContextAndEngineAsync(message, conversationContext, maxTokens, engine, cancellationToken);
        
        if (response.Status != Common.Packages.Response.Enums.ResponseStatus.Success || response.Result == null)
        {
            return result.From(response);
        }

        // Извлекаем текст из output массива
        var content = response.Result.output?.FirstOrDefault(o => o.type == "message")
                              ?.content?.FirstOrDefault(c => c.type == "output_text")
                              ?.text?.Trim();

        // Проверяем статус ответа
        if (response.Result.status == "incomplete")
        {
            _logger.LogWarning(new { 
                Method = nameof(GenerateUniversalFromResponsesApiWithContextAndTokensAndEngine),
                Status = "Incomplete response",
                Reason = response.Result.incomplete_details?.ToString(),
                ResponseId = response.Result.id,
                Engine = engine,
                ContextMessagesCount = conversationContext.Count,
                HasContent = !string.IsNullOrWhiteSpace(content)
            });
            
            // Если есть хоть какой-то текст, возвращаем его как успешный результат
            if (!string.IsNullOrWhiteSpace(content))
            {
                return result.Success(content);
            }
            
            // Если нет текста, возвращаем ошибку
            return result.Error(OpenAiErrorCodes.EmptyResponse, "Получен неполный ответ от OpenAI без текста.");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return result.Error(OpenAiErrorCodes.EmptyResponse);
        }

        return result.Success(content);
    }

    /// <summary>
    /// Универсальный метод для получения ответа от Chat Completions API с контекстом, настраиваемым количеством токенов и движком
    /// </summary>
    private async Task<LSResponse<string>> GenerateUniversalFromChatCompletionsApiWithContextAndTokensAndEngine(UserMessage message, List<ConversationMessage> conversationContext, int maxTokens, string engine, CancellationToken cancellationToken)
    {
        var result = new LSResponse<string>();
        var response = await GenerateTextCompletionWithContextAndEngineAsync(message, conversationContext, maxTokens, engine, cancellationToken);
        
        if (response.Status != Common.Packages.Response.Enums.ResponseStatus.Success || response.Result == null)
        {
            return result.From(response);
        }

        var content = response.Result.choices?.FirstOrDefault()?.message?.content?.Trim();
        if (string.IsNullOrWhiteSpace(content))
        {
            return result.Error(OpenAiErrorCodes.EmptyResponse);
        }

        return result.Success(content);
    }

    /// <summary>
    /// Генерация ответа с использованием нового Responses API с контекстом и пользовательским движком
    /// </summary>
    private async Task<LSResponse<ResponsesApiResponseDto?>> GenerateResponseWithContextAndEngineAsync(UserMessage message, List<ConversationMessage> conversationContext, int maxTokens, string engine, CancellationToken cancellationToken = default)
    {
        var result = new LSResponse<ResponsesApiResponseDto?>();
        try
        {
            _logger.LogInformation(new { 
                Method = nameof(GenerateResponseWithContextAndEngineAsync),
                ApiType = "Responses API",
                Message = message.message, 
                Role = message.role,
                MaxTokens = maxTokens,
                Temperature = message.temp,
                Model = engine,
                ContextMessagesCount = conversationContext.Count
            });

            var body = new ResponsesRequestDto()
            {
                model = engine,
                input = BuildInputForResponsesApiWithContext(message, conversationContext),
                max_output_tokens = maxTokens,
                store = false // По умолчанию не сохраняем для простоты
            };

            // Добавляем temperature только если модель его поддерживает
            if (!engine.StartsWith("o4") && !engine.StartsWith("o3") && !engine.StartsWith("o1"))
            {
                body.temperature = message.temp > 0 ? message.temp : _temp;
            }

            var url = Constants.OpenApiApis.responses;

            var request = _builder
                         .WithUrl(url)
                         .WithMethod(HttpMethod.Post)
                         .WithBody(body)
                         .Build();

            var response = await _httpClient.ExecuteRequestAsync<ResponsesApiResponseDto?>(request, Constants.OpenAiClientKey, cancellationToken).ConfigureAwait(false);

            if (response.Status == Common.Packages.Response.Enums.ResponseStatus.Success)
            {
                result.Result = response.Result;
                return result.Success();
            }

            return result.From(response);
        }
        catch (Exception ex)
        {
            _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                Method = nameof(GenerateResponseWithContextAndEngineAsync),
                Status = "Exception",
                Error = ex.GetType().Name,
                Message = ex.Message,
                Model = engine,
                StackTrace = ex.StackTrace
            });

            return result.Error(OpenAiErrorCodes.ApiCallError, ex.Message);
        }
    }

    /// <summary>
    /// Генерация ответа с использованием Chat Completions API с контекстом и пользовательским движком
    /// </summary>
    private async Task<LSResponse<TextCompletionDto?>> GenerateTextCompletionWithContextAndEngineAsync(UserMessage message, List<ConversationMessage> conversationContext, int maxTokens, string engine, CancellationToken cancellationToken = default)
    {
        var result = new LSResponse<TextCompletionDto?>();
        try
        {
            _logger.LogInformation(new { 
                Method = nameof(GenerateTextCompletionWithContextAndEngineAsync),
                ApiType = "Chat Completions API",
                Message = message.message, 
                Role = message.role,
                MaxTokens = maxTokens,
                Temperature = message.temp,
                Model = engine,
                ContextMessagesCount = conversationContext.Count
            });

            var body = new RequestModelDto()
            {
                model = engine,
                max_tokens = maxTokens,
                messages = BuildMessagesWithContext(message, conversationContext)
            };

            // Добавляем temperature только если модель его поддерживает
            if (!engine.StartsWith("o4") && !engine.StartsWith("o3") && !engine.StartsWith("o1"))
            {
                body.temperature = message.temp > 0 ? message.temp : _temp;
            }

            var url = Constants.OpenApiApis.completions;

            var request = _builder
                         .WithUrl(url)
                         .WithMethod(HttpMethod.Post)
                         .WithBody(body)
                         .Build();

            var response = await _httpClient.ExecuteRequestAsync<TextCompletionDto?>(request, Constants.OpenAiClientKey, cancellationToken).ConfigureAwait(false);

            if (response.Status == Common.Packages.Response.Enums.ResponseStatus.Success)
            {
                result.Result = response.Result;
                return result.Success();
            }

            return result.From(response);
        }
        catch (Exception ex)
        {
            _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                Method = nameof(GenerateTextCompletionWithContextAndEngineAsync),
                Status = "Exception",
                Error = ex.GetType().Name,
                Message = ex.Message,
                Model = engine,
                StackTrace = ex.StackTrace
            });

            return result.Error(OpenAiErrorCodes.ApiCallError, ex.Message);
        }
    }

    /// <summary>
    /// Универсальный метод для маппинга HTTP ошибок в соответствующие коды ошибок OpenAI
    /// </summary>
    /// <typeparam name="T">Тип ответа для LSResponse</typeparam>
    /// <param name="httpEx">HTTP исключение с информацией о статус коде</param>
    /// <returns>LSResponse с соответствующим кодом ошибки</returns>
    private LSResponse<T> MapHttpErrorToResponse<T>(HttpRequestException httpEx)
    {
        var result = new LSResponse<T>();
        var statusCode = httpEx.Data["StatusCode"]?.ToString();
        
        return statusCode switch
        {
            "401" => result.Error(OpenAiErrorCodes.InvalidApiKey),
            "429" => result.Error(OpenAiErrorCodes.RateLimitExceeded),
            "402" => result.Error(OpenAiErrorCodes.InsufficientFunds),
            "400" => result.Error(OpenAiErrorCodes.InvalidRequestFormat),
            "404" => result.Error(OpenAiErrorCodes.ModelNotFound),
            "413" => result.Error(OpenAiErrorCodes.TokenLimitExceeded),
            "503" => result.Error(OpenAiErrorCodes.ApiTimeout),
            _ => result.Error(OpenAiErrorCodes.ApiCallError)
        };
    }
}