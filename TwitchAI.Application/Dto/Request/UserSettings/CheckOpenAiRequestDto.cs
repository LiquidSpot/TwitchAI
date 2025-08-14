using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TwitchAI.Application.Dto.Request.UserSettings;

public sealed class CheckOpenAiRequestDto
{
    [JsonProperty("organizationId")]
    [JsonPropertyName("organizationId")]
    public string? OrganizationId { get; set; }

    [JsonProperty("projectId")]
    [JsonPropertyName("projectId")]
    public string? ProjectId { get; set; }

    [Required]
    [JsonProperty("apiKey")]
    [JsonPropertyName("apiKey")]
    public string ApiKey { get; set; } = string.Empty;
}


