namespace Application.Common.DTOs
{
    public class PostDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public Guid AuthorId { get; set; }
        public string AuthorUsername { get; set; } = string.Empty;
        public string? MediaUrls { get; set; }
        public string? Hashtags { get; set; }
        public int LikeCount { get; set; }
        public int RepostCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreatePostDto
    {
        public string Content { get; set; } = string.Empty;
        public string? MediaUrls { get; set; }
    }

    public class UpdatePostDto
    {
        public string Content { get; set; } = string.Empty;
        public string? MediaUrls { get; set; }
    }
}