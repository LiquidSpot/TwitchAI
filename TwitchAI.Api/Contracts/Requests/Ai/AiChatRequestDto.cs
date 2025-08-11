using System.ComponentModel.DataAnnotations;
using TwitchAI.Domain.Enums;

namespace TwitchAI.Api.Contracts.Requests.Ai;

public sealed class AiChatRequestDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;

    public Role Role { get; set; } = Role.bot;

    [Range(0, 2)]
    public double? Temperature { get; set; } = 0.3;

    [Range(1, 4000)]
    public int? MaxTokens { get; set; } = 512;

    public Guid? ChatMessageId { get; set; }
}


