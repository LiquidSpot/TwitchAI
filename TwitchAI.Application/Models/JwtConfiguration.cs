namespace TwitchAI.Application.Models;

public class JwtConfiguration
{
    public string Issuer { get; set; } = "twitchai";
    public string Audience { get; set; } = "twitchai-clients";
    public string Secret { get; set; } = "CHANGE_ME_SUPER_SECRET_KEY";
    public int AccessTokenLifetimeMinutes { get; set; } = 60;
    public int RefreshTokenLifetimeDays { get; set; } = 30;
}


