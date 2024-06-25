namespace Epsilon.Models;

public record LoginResponse(bool Success, string Reason)
{
    public LoginResponse() : this(true, "")
    {
    }
}