using System.ComponentModel.DataAnnotations;

namespace TwitchAI.Application.Dto.Request.UserSettings;

public sealed class UpsertUserSettingsRequestDto
{
    [Required] public Guid UserId { get; set; }
    public string? TwitchChannelName { get; set; }
    public string? TwitchBotUsername { get; set; }
    public string? TwitchAccessToken { get; set; }
    public string? TwitchRefreshToken { get; set; }
    public string? TwitchClientId { get; set; }
    public string? OpenAiOrganizationId { get; set; }
    public string? OpenAiProjectId { get; set; }
    public string? OpenAiApiKey { get; set; }
}


