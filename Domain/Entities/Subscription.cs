using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Subscription
    {
        public Guid Id { get; set; }
        public Guid FollowerId { get; set; }
        public Guid FollowingId { get; set; }
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "approved"; 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual User Follower { get; set; } = null!;
        public virtual User Following { get; set; } = null!;
    }
}
