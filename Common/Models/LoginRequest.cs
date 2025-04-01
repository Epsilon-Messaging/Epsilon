using System.Diagnostics.CodeAnalysis;

namespace Common.Models;

[ExcludeFromCodeCoverage]
public record LoginRequest(string PublicKey, string Username)
{
    public LoginRequest() : this("", "")
    {
    }
}