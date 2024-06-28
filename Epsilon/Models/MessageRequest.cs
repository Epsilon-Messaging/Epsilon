using System.Diagnostics.CodeAnalysis;

namespace Epsilon.Models;

[ExcludeFromCodeCoverage]
public record MessageRequest(string Message, string Username)
{
    public MessageRequest() : this("", "")
    {
    }
}