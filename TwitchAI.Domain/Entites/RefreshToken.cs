using TwitchAI.Domain.Entites.Abstract;

namespace TwitchAI.Domain.Entites;

public class RefreshToken : Entity
{
    public Guid AppUserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
}


