using System.ComponentModel.DataAnnotations;

namespace Epsilon.Models
{
    public class User
    { 
       // Hash of user's public key
       [Key]
       public byte[]? Id { get; set; }
       
       [Required]
       public byte[]? PublicKey { get; set; }
       
       // Can we possibly encrypt this such that only the sender and receiver can view this?
       [Required]
       [StringLength(16)]
       public string? Username { get; set; }
    }
}