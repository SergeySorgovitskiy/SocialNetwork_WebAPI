using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Comment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PostId { get; set; }
        public Guid AuthorId { get; set; }
        public Guid? ParentCommentId { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int NestingLevel { get; set; } = 0;
        public virtual Post Post { get; set; }
        public virtual User Author { get; set; }
        public virtual Comment? ParentComment { get; set; }
        public virtual ICollection<Comment> Replies { get; set; } = new HashSet<Comment>();
    }
}
