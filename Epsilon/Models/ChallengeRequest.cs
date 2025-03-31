using System.Diagnostics.CodeAnalysis;

namespace Epsilon.Models;

[ExcludeFromCodeCoverage]
public record ChallengeRequest(string SignedChallenge)
{
    public ChallengeRequest() : this("")
    {
    }
}