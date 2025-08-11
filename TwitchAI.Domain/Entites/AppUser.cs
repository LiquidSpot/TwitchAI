using TwitchAI.Domain.Entites.Abstract;

namespace TwitchAI.Domain.Entites;

public class AppUser : Entity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
}


