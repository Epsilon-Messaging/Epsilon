using System.Diagnostics.CodeAnalysis;

namespace Common.Models;

[ExcludeFromCodeCoverage]
public record MessageRequest(string Message, string PublicKey)
{
    public MessageRequest() : this("", "")
    {
    }
}