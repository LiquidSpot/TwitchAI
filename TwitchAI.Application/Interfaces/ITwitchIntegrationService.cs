using Common.Packages.Response.Models;
using TwitchAI.Application.Dto.Response;
using TwitchAI.Domain.Entites;
using TwitchLib.Client.Models;

namespace TwitchAI.Application.Interfaces;

public interface ITwitchIntegrationService
{
    Task<LSResponse> InitializeClientAsync(ConnectionCredentials credentials);

    Task<LSResponse<string>> SendMessage(ChatMessageDto response);
}