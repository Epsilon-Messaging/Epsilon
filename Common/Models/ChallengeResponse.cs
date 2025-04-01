using System.Diagnostics.CodeAnalysis;

namespace Common.Models;

[ExcludeFromCodeCoverage]
public record ChallengeResponse(bool Success)
{
    public ChallengeResponse() : this(true)
    {
    }
}