namespace Epsilon.Models;

public record MessageResponse(string Message, string Username)
{
    public MessageResponse() : this("", "")
    {
    }
}