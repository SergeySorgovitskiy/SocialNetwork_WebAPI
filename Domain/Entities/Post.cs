using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Post
    {
        public Guid Id { get; set; }

        public Guid AuthorId { get; set; }

        [Required]
        [StringLength(280)]
        public string Content { get; set; } = string.Empty;

        public string? MediaUrls { get; set; }

        public string? Hashtags { get; set; }

        public int LikeCount { get; set; } = 0;

        public int RepostCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public virtual User Author { get; set; } = null!;
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
