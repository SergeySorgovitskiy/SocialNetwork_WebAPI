using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Http.Headers;

namespace Domain.Entities
{
    public class Post
    {
        public Guid Id { get; set; }

        [Required]
        public Guid AuthorId { get; set; }

        [Required]
        [StringLength(280, MinimumLength = 10)]
        public string Content { get; set; }
        public ICollection<string> MediaUrls { get; set; } = new HashSet<string>();
        public ICollection<string> Hashtags { get; set; } = new HashSet<string>();
        public int RepostCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public User Author { get; set; }
        public ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();
        public ICollection<Like> Likes { get; set; } = new HashSet<Like>();
        public ICollection<Repost> Reposts { get; set; } = new HashSet<Repost>();
        public ICollection<Bookmark> Bookmarks { get; set; } = new HashSet<Bookmark>();
    }
}
