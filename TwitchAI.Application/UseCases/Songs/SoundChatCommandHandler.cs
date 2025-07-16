using Common.Packages.Logger.Services.Interfaces;
using Common.Packages.Response.Behaviors;
using Common.Packages.Response.Enums;
using Common.Packages.Response.Models;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TwitchAI.Application.Interfaces;
using TwitchAI.Application.Models;
using TwitchLib.PubSub.Models.Responses.Messages.AutomodCaughtMessage;

namespace TwitchAI.Application.UseCases.Songs
{
    internal sealed class SoundChatCommandHandler: ICommandHandler<SoundChatCommand, LSResponse<string>>
    {
        private static readonly SemaphoreSlim _initLock = new(1, 1);
        private readonly ISoundAlertsService _alerts;
        private readonly IOptions<AppConfiguration> _config;
        private readonly IWebHostEnvironment _env;
        private readonly IExternalLogger<SoundChatCommandHandler> _logger;

        public SoundChatCommandHandler(ISoundAlertsService alerts,
            IExternalLogger<SoundChatCommandHandler> log,
            IOptions<AppConfiguration> config, IWebHostEnvironment env)
        {
            _alerts = alerts ?? throw new ArgumentNullException(nameof(alerts));
            _logger = log ?? throw new ArgumentNullException(nameof(log));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        public async Task<LSResponse<string?>> Handle(SoundChatCommand command, CancellationToken ct)
        {
            var response = new LSResponse<string>();
            _logger.LogInformation(command);

            if (!_alerts.IsReady) await EnsureSetupAsync(ct);

            var result = await _alerts.HandleAsync(command.RawMessage, ct);
            if (result.Status == ResponseStatus.Error) 
                return new LSResponse<string>().From(result);

            return response.Success(result.Result);
        }

        private async Task EnsureSetupAsync(CancellationToken ct)
        {
            if (_alerts.IsReady) return;

            await _initLock.WaitAsync(ct);
            try
            {
                if (_alerts.IsReady) return;

                var folder = _config.Value.SoundAlerts
                    ?? Path.Combine(_env.ContentRootPath, "sounds");

                var cooldown = TimeSpan.FromSeconds(_config.Value.CooldownSecounds);

                await _alerts.SetUpAsync(folder, cooldown, ct);
            }
            finally { _initLock.Release(); }
        }
    }
}
