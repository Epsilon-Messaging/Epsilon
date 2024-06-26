using System.Diagnostics.CodeAnalysis;

namespace Epsilon.Models;

[ExcludeFromCodeCoverage]
public record LoginResponse(bool Success, string Reason)
{
    public LoginResponse() : this(true, "")
    {
    }
}