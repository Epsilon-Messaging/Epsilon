namespace Epsilon.Models
{
    public class Message
    {
        // Hash of senders's public key
       public byte[]? SenderId { get; set; }
        // Hash of recipient's public key
       public byte[]? RecipientId { get; set; }
       public byte[]? EncryptedMessage { get; set; }
       public DateTime? CreatedAt { get; set; }
    }
}
