using System.Diagnostics.CodeAnalysis;

namespace Common.Models;

[ExcludeFromCodeCoverage]
public record LoginResponse(string ChallangeToken, string SystemPublic)
{
    public LoginResponse() : this("", "")
    {
    }
}