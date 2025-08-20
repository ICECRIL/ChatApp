using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApp.Server.Models
{
    [Table("Messages")]
    public class Message 
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, StringLength(4000)] 
        public string Content { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("SenderId")]
        public required User Sender { get; set; }
        public Guid SenderId { get; set; }
    }
}
