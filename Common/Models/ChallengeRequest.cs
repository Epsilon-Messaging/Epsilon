using System.Diagnostics.CodeAnalysis;

namespace Common.Models;

[ExcludeFromCodeCoverage]
public record ChallengeRequest(string SignedChallenge)
{
    public ChallengeRequest() : this("")
    {
    }
}