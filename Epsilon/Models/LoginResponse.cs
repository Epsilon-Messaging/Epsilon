namespace Epsilon.Models;

public record LoginResponse(bool success, string reason)
{
    public LoginResponse() : this(true, "")
    {
    }
}