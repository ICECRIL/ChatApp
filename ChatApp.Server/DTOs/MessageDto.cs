namespace ChatApp.Server.DTOs
{
    public record MessageDto
    {
        public Guid Id { get; set; }
        public required string? UserName { get; set; }
        public Guid SenderId { get; set; }
        public required string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
