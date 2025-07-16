using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Bookmark
{
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }
    public User User { get; set; }

    [Required]
    public Guid PostId { get; set; }
    public Post Post { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}