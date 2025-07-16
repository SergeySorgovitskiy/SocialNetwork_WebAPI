namespace Application.Common.DTOs
{
    public class SubscriptionDto
    {
        public Guid Id { get; set; }
        public Guid FollowerId { get; set; }
        public required string FollowerUserName { get; set; }
        public Guid FollowingId { get; set; }
        public required string FollowingUserName { get; set; }
        public required string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateSubscriptionDto
    {
        public Guid FollowerId { get; set; }
        public Guid FollowingId { get; set; }
    }

    public class UpdateSubscriptionDto
    {
        public required string Status { get; set; }
    }
}