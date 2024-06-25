namespace Epsilon.Models;

public record LoginRequest(string Username)
{
    public LoginRequest() : this("")
    {
    }
}