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
            MessagesAllowedInPeriod = 750,
            ThrottlingPeriod = TimeSpan.FromSeconds(30)
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
        _logger.LogInformation(new { 
            Method = nameof(SendMessage),
            HasMessage = !string.IsNullOrEmpty(response.Message),
            Message = response.Message,
            UserName = response.TwitchUser?.UserName,
            IsClientConnected = _client?.IsConnected,
            ChannelName = _configuration.ChannelName
        });

        if (!string.IsNullOrEmpty(response.Message))
        {
            if (_client?.IsConnected == true)
            {
                _client.SendMessage(_configuration.ChannelName, $"@{response.TwitchUser.UserName}: {response.Message}");
                _logger.LogInformation(new { 
                    Method = nameof(SendMessage),
                    Status = "Message sent successfully",
                    Channel = _configuration.ChannelName,
                    User = response.TwitchUser.UserName
                });
            }
            else
            {
                _logger.LogError((int)OpenAiErrorCodes.EmptyResponse, new { 
                    Method = nameof(SendMessage),
                    Status = "Client not connected",
                    IsConnected = _client?.IsConnected,
                    Channel = _configuration.ChannelName
                });
            }
            
            return new LSResponse<string>().Success(response.Message);
        }
        
        _logger.LogInformation(new { 
            Method = nameof(SendMessage),
            Status = "No message to send",
            Message = response.Message
        });
        
        return new LSResponse<string>().Success(string.Empty);
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
