using System.Diagnostics.CodeAnalysis;

namespace Epsilon.Models;

[ExcludeFromCodeCoverage]
public record MessageResponse(string Message, string Username)
{
    public MessageResponse() : this("", "")
    {
    }
}