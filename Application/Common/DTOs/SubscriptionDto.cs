namespace Application.Common.DTOs
{
    public class SubscriptionDto
    {
        public Guid Id { get; set; }
        public Guid FollowerId { get; set; }
        public string FollowerUsername { get; set; } = string.Empty;
        public Guid FollowingId { get; set; }
        public string FollowingUsername { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateSubscriptionDto
    {
        public Guid FollowerId { get; set; }
        public Guid FollowingId { get; set; }
    }

    public class UpdateSubscriptionDto
    {
        public string Status { get; set; } = string.Empty;
    }
}