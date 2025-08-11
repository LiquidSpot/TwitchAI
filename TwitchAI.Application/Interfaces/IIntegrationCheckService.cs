using Common.Packages.Response.Models;

namespace TwitchAI.Application.Interfaces;

public interface IIntegrationCheckService
{
    Task<LSResponse<string>> CheckTwitchAsync(string clientId, string accessToken, CancellationToken ct);
    Task<LSResponse<string>> CheckOpenAiAsync(string apiKey, string? organizationId, string? projectId, CancellationToken ct);
}


