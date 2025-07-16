using Common.Packages.Logger.Services.Interfaces;
using Common.Packages.Response.Models;

using NAudio.Wave;

using TwitchAI.Application.Interfaces;

namespace TwitchAI.Infrastructure.Services
{
    internal sealed class SoundAlertsService : ISoundAlertsService
    {
        private readonly Dictionary<string, string> _map = new();

        private DateTime _lastPlayed = DateTime.MinValue;
        private TimeSpan _cooldown = TimeSpan.FromSeconds(10);
        private const float _volume = .20f;

        public bool IsReady { get; private set; }

        public async Task<IReadOnlyCollection<string>> SetUpAsync(string soundsFolder,
                                                                  TimeSpan? cooldown = null,
                                                                  CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(soundsFolder) || !Directory.Exists(soundsFolder))
                throw new DirectoryNotFoundException(soundsFolder);

            _map.Clear();
            foreach (var file in Directory.EnumerateFiles(soundsFolder)
                                          .Where(f => f.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) ||
                                                      f.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)))
            {
                _map['!' + Path.GetFileNameWithoutExtension(file).ToLowerInvariant()] = file;
            }

            _cooldown = cooldown ?? _cooldown;
            IsReady = _map.Count > 0;

            return _map.Keys.ToArray();
        }

        public async Task<LSResponse<string?>> HandleAsync(string raw, CancellationToken ct = default)
        {
            var resp = new LSResponse<string?>();

            if (!IsReady || string.IsNullOrWhiteSpace(raw)) return resp;

            var alias = raw.Split(' ', 2)[0].ToLowerInvariant();
            if (!_map.TryGetValue(alias, out var file)) return resp;

            var now = DateTime.UtcNow;
            if (now - _lastPlayed < _cooldown)
            {
                var rest = _cooldown - (now - _lastPlayed);
                return resp.Success($"Подождите ещё {rest.Seconds}.{rest.Milliseconds:D1} сек.");
            }

            _lastPlayed = now;
            await PlayAsync(file, ct);
            return resp.Success(null);
        }

        public async Task PlayAsync(string file, CancellationToken ct = default)
        {
            await using var reader = new AudioFileReader(file) { Volume = _volume };
            using var output = new WaveOutEvent();
            output.Init(reader);
            output.Play();
            while (output.PlaybackState == PlaybackState.Playing && !ct.IsCancellationRequested)
                await Task.Delay(100, ct);
        }
    }
}
