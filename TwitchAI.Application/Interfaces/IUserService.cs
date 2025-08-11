using Common.Packages.Response.Models;
using TwitchAI.Domain.Entites;

namespace TwitchAI.Application.Interfaces;

public interface IUserService
{
    Task<LSResponse<AppUser>> RegisterAsync(string email, string password, CancellationToken ct);
    Task<LSResponse<string>> LoginAsync(string email, string password, CancellationToken ct); // returns JWT
    Task<LSResponse<bool>> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken ct);
    Task<LSResponse<AppUser>> FindByEmailAsync(string email, CancellationToken ct);
}


