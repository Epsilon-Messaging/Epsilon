namespace Epsilon.Models;

public record LoginRequest(int Id)
{
    public LoginRequest() : this(0)
    {
    }
}