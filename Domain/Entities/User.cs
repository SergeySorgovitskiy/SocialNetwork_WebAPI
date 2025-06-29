using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string? AvatarUrl { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        public bool IsPrivate { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Subscription> Followers { get; set; } = new List<Subscription>();
        public virtual ICollection<Subscription> Following { get; set; } = new List<Subscription>();
       
    }
}
