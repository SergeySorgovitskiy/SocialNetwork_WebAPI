using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Content { get; set; } = string.Empty;

        public Guid PostId { get; set; }

        public Guid AuthorId { get; set; }

        public Guid? ParentCommentId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Post Post { get; set; } = null!;
        public virtual User Author { get; set; } = null!;
        public virtual Comment? ParentComment { get; set; }
        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
    }
}
