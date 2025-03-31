using System.Diagnostics.CodeAnalysis;

namespace Epsilon.Models;

[ExcludeFromCodeCoverage]
public record LoginResponse(string ChallangeToken, string SystemPublic)
{
    public LoginResponse() : this("", "")
    {
    }
}