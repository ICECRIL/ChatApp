using System.ComponentModel.DataAnnotations;

namespace ChatApp.Server.DTOs
{
    public record MessageUpdateDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required, StringLength(4000)]
        public required string Content { get; set; }
    }
}
