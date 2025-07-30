using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Repost
{
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }
    public User User { get; set; }

    [Required]
    public Guid OriginalPostId { get; set; }
    public Post OriginalPost { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Опциональный комментарий к репосту
    public string? Comment { get; set; }
}