using TwitchAI.Domain.Entites.Abstract;

namespace TwitchAI.Domain.Entites;

public class UserIntegrationSettings : Entity
{
    public Guid AppUserId { get; set; }
    public string? TwitchChannelName { get; set; }
    public string? TwitchBotUsername { get; set; }
    public string? TwitchAccessTokenEncrypted { get; set; }
    public string? TwitchRefreshTokenEncrypted { get; set; }
    public string? TwitchClientId { get; set; }

    public string? OpenAiOrganizationId { get; set; }
    public string? OpenAiProjectId { get; set; }
    public string? OpenAiApiKeyEncrypted { get; set; }
}


