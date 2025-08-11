using System.ComponentModel.DataAnnotations;

namespace TwitchAI.Api.Contracts.Requests.Utility;

public sealed class ComplimentRequestDto
{
    [Required]
    public Guid UserId { get; set; }

    [MaxLength(64)]
    public string? TargetUsername { get; set; }
}


