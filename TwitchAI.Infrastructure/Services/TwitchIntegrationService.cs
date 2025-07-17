using Common.Packages.Logger.Services.Interfaces;
using Common.Packages.Response.Models;

using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Serilog.Context;

using TwitchAI.Application.Dto.Response;
using TwitchAI.Application.Interfaces;
using TwitchAI.Application.Models;
using TwitchAI.Application.UseCases.Twitch.Message;
using TwitchAI.Domain.Enums.ErrorCodes;

using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Interfaces;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

// ReSharper disable All

namespace TwitchAI.Infrastructure.Services;

/// <summary>
/// Живёт всё время работы приложения и держит постоянное WebSocket-соединение с Twitch.
/// </summary>
internal sealed class TwitchIntegrationService : ITwitchIntegrationService, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    public readonly IHttpContextAccessor _httpAccessor;
    private readonly TwitchConfiguration _configuration;
    private readonly IExternalLogger<TwitchIntegrationService> _logger;

    private ITwitchClient? _client;
    private bool _disposed;

    public TwitchIntegrationService(
        IServiceScopeFactory scopeFactory,
        IHttpContextAccessor httpAccessor,
        IOptions<TwitchConfiguration> configuration,
        IExternalLogger<TwitchIntegrationService> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _configuration = configuration?.Value ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpAccessor = httpAccessor ?? throw new ArgumentNullException(nameof(httpAccessor));
    }

    public async Task<LSResponse> InitializeClientAsync(ConnectionCredentials credentials)
    {
        var response = new LSResponse();

        var clientOptions = new ClientOptions
        {
            MessagesAllowedInPeriod = 300,
            ThrottlingPeriod = TimeSpan.FromSeconds(36)
        };

        var wsClient = new WebSocketClient(clientOptions);
        _client = new TwitchClient(wsClient);
        _client.OnLog += (_, e) => _logger.LogDebug(new { TwitchLib = e.Data });
        _client.OnJoinedChannel += (_, e) => _logger.LogDebug(new { Connected = e.Channel });
        _client.OnConnected += Client_OnConnected;
        _client.OnMessageReceived += Client_OnMessageReceived;
        _client.Initialize(credentials, _configuration.ChannelName);
        _client.Connect();
        
        _logger.LogInformation(new { Channel = _configuration.ChannelName, IsInitialized = _client.IsInitialized, IsConnected = _client.IsConnected });
        return response.Success();
    }
    private void Client_OnConnected(object? sender, OnConnectedArgs e)
    {
        _logger.LogInformation(new { Status = $"Connected as {e.BotUsername}."});
        _client?.JoinChannel(_configuration.ChannelName);
    }

    public async Task<LSResponse<string>> SendMessage(ChatMessageDto response)
    {
        _logger.LogInformation(new
        {
            Method = nameof(SendMessage),
            HasMessage = !string.IsNullOrEmpty(response.Message),
            UserName = response.TwitchUser?.UserName,
            IsClientConnected = _client?.IsConnected,
            ChannelName = _configuration.ChannelName,
            MessageLength = response.Message?.Length ?? 0
        });

        if (!string.IsNullOrEmpty(response.Message))
        {
            if (_client?.IsConnected == true)
            {
                try
                {
                    // Twitch лимит: 500 символов для всего сообщения
                    var userPrefix = $"@{response.TwitchUser.UserName}: ";
                    var maxContentLength = 500 - userPrefix.Length;

                    // Разбиваем сообщение на части, если оно слишком длинное
                    var messageParts = SplitMessage(response.Message, maxContentLength);
                    
                    // Отправляем каждую часть с задержкой
                    for (int i = 0; i < messageParts.Count; i++)
                    {
                        var part = messageParts[i];
                        var partNumber = i + 1;
                        var totalParts = messageParts.Count;

                        // Добавляем номер части только если сообщение разделено
                        var messageToSend = totalParts > 1 ? $"{userPrefix}({partNumber}/{totalParts}) {part}" : $"{userPrefix}{part}";

                        try
                        {
                            _client.SendMessage(_configuration.ChannelName, messageToSend
                                                                           .Replace("nya~", "nya ")
                                                                           .Replace("UwU~", "UwU ")
                                                                           .Replace("nyaYay~", "nyaYay ")
                                                                           .Replace("Nyaa", "Nyaa ")
                                                                           .Replace("Nyaa~", "Nyaa "));

                            _logger.LogInformation(new
                            {
                                Status = "Message part sent successfully",
                                User = response.TwitchUser.UserName,
                                PartNumber = partNumber,
                                TotalParts = totalParts
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError((int)BaseErrorCodes.InternalServerError, new
                            {
                                Method = nameof(SendMessage),
                                Status = "Failed to send message part",
                                Channel = _configuration.ChannelName,
                                User = response.TwitchUser.UserName,
                                PartNumber = partNumber,
                                TotalParts = totalParts,
                                Error = ex.GetType().Name,
                                Message = ex.Message
                            });
                        }

                        // Увеличиваем задержку между сообщениями до 3 секунд
                        if (i < messageParts.Count - 1) { await Task.Delay(2000); }
                    }
                   
                }
                catch (Exception ex)
                {
                    _logger.LogError((int)BaseErrorCodes.InternalServerError, new
                    {
                        Method = nameof(SendMessage),
                        Status = "Exception during message sending",
                        Error = ex.GetType().Name,
                        Message = ex.Message,
                        Channel = _configuration.ChannelName
                    });
                }
            }
            else
            {
                _logger.LogError((int)BaseErrorCodes.ConnectionTimeout, new
                {
                    Method = nameof(SendMessage),
                    Status = "Client not connected",
                    IsConnected = _client?.IsConnected,
                    Channel = _configuration.ChannelName
                });
            }

            return new LSResponse<string>().Success(response.Message);
        }

        _logger.LogInformation(new
        {
            Method = nameof(SendMessage),
            Status = "No message to send",
            Message = response.Message
        });

        return new LSResponse<string>().Success(string.Empty);
    }

    /// <summary>
    /// Разбивает сообщение на части с учетом лимита символов
    /// </summary>
    private List<string> SplitMessage(string message, int maxLength)
    {
        var parts = new List<string>();
        
        if (message.Length <= maxLength)
        {
            parts.Add(message);
            return parts;
        }

        // Попытаемся разбить по предложениям
        var sentences = message.Split(new[] { ". ", "! ", "? ", ".\n", "!\n", "?\n" }, StringSplitOptions.RemoveEmptyEntries);
        
        var currentPart = "";
        
        foreach (var sentence in sentences)
        {
            var sentenceWithPunctuation = sentence.TrimEnd() + (sentence.EndsWith(".") || sentence.EndsWith("!") || sentence.EndsWith("?") ? "" : ".");
            
            // Проверяем, поместится ли предложение в текущую часть
            if (currentPart.Length + sentenceWithPunctuation.Length + 1 <= maxLength)
            {
                currentPart += (currentPart.Length > 0 ? " " : "") + sentenceWithPunctuation;
            }
            else
            {
                // Если текущая часть не пуста, добавляем ее в результат
                if (!string.IsNullOrEmpty(currentPart))
                {
                    parts.Add(currentPart.Trim());
                    currentPart = "";
                }
                
                // Если предложение само по себе слишком длинное, разбиваем по словам
                if (sentenceWithPunctuation.Length > maxLength)
                {
                    var wordParts = SplitByWords(sentenceWithPunctuation, maxLength);
                    parts.AddRange(wordParts);
                }
                else
                {
                    currentPart = sentenceWithPunctuation;
                }
            }
        }
        
        // Добавляем последнюю часть, если она не пуста
        if (!string.IsNullOrEmpty(currentPart))
        {
            parts.Add(currentPart.Trim());
        }
        
        return parts;
    }

    /// <summary>
    /// Разбивает текст по словам с учетом лимита символов
    /// </summary>
    private List<string> SplitByWords(string text, int maxLength)
    {
        var parts = new List<string>();
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        var currentPart = "";
        
        foreach (var word in words)
        {
            // Проверяем, поместится ли слово в текущую часть
            if (currentPart.Length + word.Length + 1 <= maxLength)
            {
                currentPart += (currentPart.Length > 0 ? " " : "") + word;
            }
            else
            {
                // Если текущая часть не пуста, добавляем ее в результат
                if (!string.IsNullOrEmpty(currentPart))
                {
                    parts.Add(currentPart.Trim());
                    currentPart = "";
                }
                
                // Если слово само по себе слишком длинное, обрезаем его
                if (word.Length > maxLength)
                {
                    var truncatedWord = word.Substring(0, maxLength - 3) + "...";
                    parts.Add(truncatedWord);
                }
                else
                {
                    currentPart = word;
                }
            }
        }
        
        // Добавляем последнюю часть, если она не пуста
        if (!string.IsNullOrEmpty(currentPart))
        {
            parts.Add(currentPart.Trim());
        }
        
        return parts;
    }

    private void Client_OnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        _ = Task.Run(() => HandleMessageAsync(e));
    }

    private async Task HandleMessageAsync(OnMessageReceivedArgs e)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var requestId = string.Format($"ID:{Guid.NewGuid().ToString("N").Substring(0, 10)}");
        LSRequestContext.Id = requestId;

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        using (LogContext.PushProperty("RequestID", requestId))
        {
            try
            {
                var response =
                    await mediator.Send(new HandleMessageCommand(e.ChatMessage));
                
                if(response.Result.Message is not null)
                    await SendMessage(response.Result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BaseErrorCodes.IncorrectRequest, new { Message = $"Error handle from @{e.ChatMessage.Username}. Msg: {e.ChatMessage.Message}" });
            }
            finally
            {
                LSRequestContext.Id = null;
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _client?.Disconnect();
        _disposed = true;
    }
}
