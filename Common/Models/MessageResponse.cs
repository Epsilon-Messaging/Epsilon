using System.Diagnostics.CodeAnalysis;

namespace Common.Models;

[ExcludeFromCodeCoverage]
public record MessageResponse(string Message, string PublicKey, string Username)
{
    public MessageResponse() : this("", "", "")
    {
    }
}