using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
#pragma warning disable CS8618
using Newtonsoft.Json;

namespace TwitchAI.Application.Dto.Request.UserSettings;

public sealed class CheckTwitchRequestDto
{
    [Required]
    [JsonProperty("accessToken")]
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; } = string.Empty;

    [Required]
    [JsonProperty("clientId")]
    [JsonPropertyName("clientId")]
    public string ClientId { get; set; } = string.Empty;
}


