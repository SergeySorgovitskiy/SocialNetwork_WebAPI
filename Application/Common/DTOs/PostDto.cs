namespace Application.Common.DTOs
{
    public class PostDto
    {
        public Guid Id { get; set; }
        public required string Content { get; set; }
        public Guid AuthorId { get; set; }
        public required string AuthorUserName { get; set; }
        public ICollection<string> MediaUrls { get; set; } = new List<string>();
        public ICollection<string> Hashtags { get; set; } = new List<string>();
        public int LikeCount { get; set; }
        public int RepostCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreatePostDto
    {
        public required string Content { get; set; }
        public ICollection<string> MediaUrls { get; set; } = new List<string>();
    }

    public class UpdatePostDto
    {
        public required string Content { get; set; }
        public ICollection<string> MediaUrls { get; set; } = new List<string>();
    }
}