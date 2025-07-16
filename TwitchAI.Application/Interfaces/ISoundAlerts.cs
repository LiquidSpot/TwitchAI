using Common.Packages.Response.Models;

namespace TwitchAI.Application.Interfaces
{
    /// <summary>
    ///  Звуковые алёрты, хранят карту  "!alias" → "C:\...\file.mp3"  
    /// </summary>
    public interface ISoundAlertsService
    {
        /// <summary> Настроен ли сервис (загружена ли карта команд). </summary>
        bool IsReady { get; }

        /// <summary> Инициализирует карту команд, возвращает список допустимых «!alias». </summary>
        Task<IReadOnlyCollection<string>> SetUpAsync(string soundsFolder,
            TimeSpan? cooldown = null,
            CancellationToken ct = default);

        /// <summary> Проверяет входящее сообщение; если найден алиас — запускает звук. </summary>
        Task<LSResponse<string?>> HandleAsync(string rawMessage, CancellationToken ct = default);

        /// <summary> Проигрывает конкретный файл (используется команд-хэндлером). </summary>
        Task PlayAsync(string file, CancellationToken ct = default);
    }
}
