using System.ComponentModel.DataAnnotations;

namespace TwitchAI.Application.Dto.Request.UserSettings;

public sealed class CheckOpenAiRequestDto
{
    public string? OrganizationId { get; set; }
    public string? ProjectId { get; set; }
    [Required] public string ApiKey { get; set; } = string.Empty;
}


