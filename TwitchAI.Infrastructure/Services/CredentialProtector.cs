using Microsoft.AspNetCore.DataProtection;
using TwitchAI.Application.Interfaces;

namespace TwitchAI.Infrastructure.Services;

internal class CredentialProtector : ICredentialProtector
{
    private readonly IDataProtector _protector;

    public CredentialProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("TwitchAI.Credentials");
    }

    public string? Protect(string? plaintext)
    {
        if (string.IsNullOrEmpty(plaintext)) return null;
        return _protector.Protect(plaintext);
    }

    public string? Unprotect(string? protectedText)
    {
        if (string.IsNullOrEmpty(protectedText)) return null;
        return _protector.Unprotect(protectedText);
    }
}


