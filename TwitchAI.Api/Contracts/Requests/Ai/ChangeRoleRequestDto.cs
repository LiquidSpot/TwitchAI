using System.ComponentModel.DataAnnotations;

namespace TwitchAI.Api.Contracts.Requests.Ai;

public sealed class ChangeRoleRequestDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(32)]
    public string RoleName { get; set; } = string.Empty;
}


