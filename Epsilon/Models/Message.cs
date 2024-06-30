using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Epsilon.Models;
public class Message
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int? MessageId { get; set; }

    // Hash of senders's public key
    [Required]
    [ForeignKey("User")]
    public byte[]? SenderId { get; set; }

    // Hash of recipient's public key
    [Required]
    [ForeignKey("User")]
    public byte[]? RecipientId { get; set; }

    [Required]
    public byte[]? EncryptedMessage { get; set; }

    [Required]
    public DateTime? CreatedAt { get; set; }
}