using System.Diagnostics.CodeAnalysis;

namespace Epsilon.Models;

[ExcludeFromCodeCoverage]
public record LoginRequest(string Username)
{
    public LoginRequest() : this("")
    {
    }
}