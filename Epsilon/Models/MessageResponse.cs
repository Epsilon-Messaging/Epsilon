namespace Epsilon.Models;

public record MessageResponse(string Message)
{
    public MessageResponse() : this("")
    {
    }
}