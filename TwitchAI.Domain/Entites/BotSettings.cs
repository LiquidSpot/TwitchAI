using TwitchAI.Domain.Entites.Abstract;

namespace TwitchAI.Domain.Entites;

public class BotSettings : Entity
{
    public Guid AppUserId { get; set; }

    public string DefaultRole { get; set; } = "bot";
    public int CooldownSeconds { get; set; } = 25;
    public int ReplyLimit { get; set; } = 3;

    public bool EnableAi { get; set; } = true;
    public bool EnableCompliment { get; set; } = true;
    public bool EnableFact { get; set; } = true;
    public bool EnableHoliday { get; set; } = true;
    public bool EnableTranslation { get; set; } = true;
    public bool EnableSoundAlerts { get; set; } = true;
    public bool EnableViewersStats { get; set; } = true;
}


