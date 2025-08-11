using System.ComponentModel.DataAnnotations;

namespace TwitchAI.Api.Contracts.Requests.Ai;

public sealed class EngineUpdateRequestDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(64)]
    public string EngineName { get; set; } = string.Empty;
}


