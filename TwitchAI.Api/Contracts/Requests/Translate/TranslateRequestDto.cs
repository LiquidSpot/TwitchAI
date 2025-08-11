using System.ComponentModel.DataAnnotations;

namespace TwitchAI.Api.Contracts.Requests.Translate;

public sealed class TranslateRequestDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(4)]
    public string Language { get; set; } = "en";

    [Required]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;
}


