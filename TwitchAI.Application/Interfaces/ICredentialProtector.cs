namespace TwitchAI.Application.Interfaces;

public interface ICredentialProtector
{
    string? Protect(string? plaintext);
    string? Unprotect(string? protectedText);
}


