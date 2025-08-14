using Common.Packages.HttpClient.Models;
using Common.Packages.HttpClient.Services.Interfaces;
using Common.Packages.Response.Models;
using Microsoft.Extensions.Options;
using TwitchAI.Application.Constants;
using TwitchAI.Application.Interfaces;
using TwitchAI.Application.Models;

namespace TwitchAI.Infrastructure.Services;

internal class IntegrationCheckService : IIntegrationCheckService
{
    private readonly ILSClientService _httpClient;
    private readonly AppConfiguration _cfg;

    public IntegrationCheckService(ILSClientService httpClient, IOptions<AppConfiguration> cfg)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _cfg = cfg.Value;
    }

    public async Task<LSResponse<string>> CheckTwitchAsync(string clientId, string accessToken, CancellationToken ct)
    {
        var resp = new LSResponse<string>();
        var request = new RequestBuilder<object>()
            .WithUrl("users")
            .WithMethod(HttpMethod.Get)
            .WithHeaders(new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {accessToken}",
                ["Client-Id"] = clientId
            })
            .Build();

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(8));

            var result = await _httpClient
                .ExecuteRequestAsync<object>(request, Constants.TwitchApiClientKey, cts.Token)
                .ConfigureAwait(false);

            if (result.Status != Common.Packages.Response.Enums.ResponseStatus.Success)
                return resp.Success("error");
            return resp.Success("ok");
        }
        catch
        {
            // Любая сетевая ошибка/таймаут трактуем как ошибку проверки
            return resp.Success("error");
        }
    }

    public async Task<LSResponse<string>> CheckOpenAiAsync(string apiKey, string? organizationId, string? projectId, CancellationToken ct)
    {
        var resp = new LSResponse<string>();

        var headers = new Dictionary<string, string>
        {
            ["Authorization"] = $"Bearer {apiKey}"
        };
        if (!string.IsNullOrEmpty(organizationId)) headers["OpenAI-Organization"] = organizationId;
        if (!string.IsNullOrEmpty(projectId)) headers["OpenAI-Project"] = projectId;

        var request = new RequestBuilder<object>()
            .WithUrl("models")
            .WithMethod(HttpMethod.Get)
            .WithHeaders(headers)
            .Build();

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(8));

            var result = await _httpClient
                .ExecuteRequestAsync<object>(request, Constants.OpenAiClientKey, cts.Token)
                .ConfigureAwait(false);

            if (result.Status != Common.Packages.Response.Enums.ResponseStatus.Success)
                return resp.Success("error");
            return resp.Success("ok");
        }
        catch
        {
            return resp.Success("error");
        }
    }
}


