using System.ComponentModel.DataAnnotations;

namespace TwitchAI.Application.Dto.Request.UserSettings;

public sealed class CheckTwitchRequestDto
{
    [Required] public string AccessToken { get; set; } = string.Empty;
    [Required] public string ClientId { get; set; } = string.Empty;
}


