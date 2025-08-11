using System.Security.Cryptography;
using System.Text;
using Common.Packages.Response.Models;
using Microsoft.Extensions.Options;
using TwitchAI.Application.Interfaces;
using TwitchAI.Application.Models;
using TwitchAI.Domain.Entites;
using TwitchAI.Application.Interfaces.Infrastructure;
using Common.Packages.Logger.Services.Interfaces;

namespace TwitchAI.Infrastructure.Services;

internal class UserService : IUserService
{
    private readonly IUnitOfWork _uow;
    private readonly IRepository<AppUser, Guid> _userRepository;
    private readonly AppConfiguration _cfg;
    private readonly IExternalLogger<UserService> _logger;

    public UserService(IUnitOfWork uow, IOptions<AppConfiguration> cfg, IExternalLogger<UserService> logger)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _userRepository = uow.Factory<AppUser, Guid>() ?? throw new ArgumentNullException(nameof(uow));
        _cfg = cfg.Value ?? throw new ArgumentNullException(nameof(cfg));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LSResponse<AppUser>> RegisterAsync(string email, string password, CancellationToken ct)
    {
        var resp = new LSResponse<AppUser>();
        email = email.Trim().ToLowerInvariant();

        if (await _userRepository.AnyAsync(x => x!.Email == email, ct))
            return resp.Success(default!); // already exists (idempotent)

        var (hash, salt) = HashPassword(password);
        var user = new AppUser { Email = email, PasswordHash = hash, PasswordSalt = salt };
        await _userRepository.AddAsync(user, ct, saveChanges: true);
        return resp.Success(user);
    }

    public async Task<LSResponse<string>> LoginAsync(string email, string password, CancellationToken ct)
    {
        var resp = new LSResponse<string>();
        email = email.Trim().ToLowerInvariant();
        var user = await _userRepository.SingleOrDefaultAsync(x => x!.Email == email, ct);
        if (user == null) return resp.Success(string.Empty);

        if (!VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
            return resp.Success(string.Empty);

        // For now return a simple opaque token (placeholder). Replace with JWT later.
        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        user.LastLoginAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync(ct, saveChanges: true);
        return resp.Success(token);
    }

    public async Task<LSResponse<AppUser>> FindByEmailAsync(string email, CancellationToken ct)
    {
        var resp = new LSResponse<AppUser>();
        email = email.Trim().ToLowerInvariant();
        var user = await _userRepository.SingleOrDefaultAsync(x => x!.Email == email, ct);
        return resp.Success(user!);
    }

    public async Task<LSResponse<bool>> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken ct)
    {
        var resp = new LSResponse<bool>();
        var user = await _userRepository.GetAsync(userId, ct);
        if (user == null) return resp.Success(false);
        if (!VerifyPassword(currentPassword, user.PasswordHash, user.PasswordSalt))
            return resp.Success(false);

        var (hash, salt) = HashPassword(newPassword);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        await _uow.SaveChangesAsync(ct, saveChanges: true);
        return resp.Success(true);
    }

    private static (string hash, string salt) HashPassword(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        var saltBytes = new byte[16];
        rng.GetBytes(saltBytes);
        var salt = Convert.ToBase64String(saltBytes);
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + salt));
        var hash = Convert.ToBase64String(hashBytes);
        return (hash, salt);
    }

    private static bool VerifyPassword(string password, string hash, string salt)
    {
        using var sha256 = SHA256.Create();
        var computed = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password + salt)));
        return computed == hash;
    }
}


