namespace Epsilon.Models
{
    public class User
    { 
        // Hash of user's public key
       public byte[]? Id { get; set; }
       public byte[]? PublicKey { get; set; }
       // Can we possibly encrypt this such that only the sender and receiver can view this?
       public string? Username { get; set; }
    }
}