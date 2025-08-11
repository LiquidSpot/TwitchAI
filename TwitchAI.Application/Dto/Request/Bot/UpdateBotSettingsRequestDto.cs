using System.ComponentModel.DataAnnotations;

namespace TwitchAI.Application.Dto.Request.Bot;

public sealed class UpdateBotSettingsRequestDto
{
    [Required] public Guid UserId { get; set; }
    public string DefaultRole { get; set; } = "bot";
    [Range(1, 600)] public int CooldownSeconds { get; set; } = 25;
    [Range(1, 10)] public int ReplyLimit { get; set; } = 3;
    public bool EnableAi { get; set; } = true;
    public bool EnableCompliment { get; set; } = true;
    public bool EnableFact { get; set; } = true;
    public bool EnableHoliday { get; set; } = true;
    public bool EnableTranslation { get; set; } = true;
    public bool EnableSoundAlerts { get; set; } = true;
    public bool EnableViewersStats { get; set; } = true;
}


