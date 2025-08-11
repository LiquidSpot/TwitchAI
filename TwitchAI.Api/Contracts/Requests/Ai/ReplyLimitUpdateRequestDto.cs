using System.ComponentModel.DataAnnotations;

namespace TwitchAI.Api.Contracts.Requests.Ai;

public sealed class ReplyLimitUpdateRequestDto
{
    [Required]
    public Guid UserId { get; set; }

    [Range(1, 10)]
    public int Limit { get; set; }
}


