using System.Diagnostics.CodeAnalysis;

namespace Epsilon.Models;

[ExcludeFromCodeCoverage]
public record ChallengeResponse(bool Success)
{
    public ChallengeResponse() : this(true)
    {
    }
}