using System.ComponentModel.DataAnnotations;
using Domain.Enums;
namespace Domain.Entities
{
    public class Subscription
    {
        public Guid Id { get; set; }
        public Guid FollowerId { get; set; }
        public Guid FollowingId { get; set; }
        public SubscriptionStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual User Follower { get; set; }
        public virtual User Following { get; set; }
    }
    
}
