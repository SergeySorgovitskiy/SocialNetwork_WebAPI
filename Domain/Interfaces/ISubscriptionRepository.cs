using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ISubscriptionRepository
    {
        Task<IEnumerable<Subscription>> GetAllAsync(CancellationToken cancellationToken);
        Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<Subscription>> GetFollowersAsync(Guid userId, CancellationToken cancellationToken);
        Task<IEnumerable<Subscription>> GetFollowingAsync(Guid userId, CancellationToken cancellationToken);
        Task<IEnumerable<Subscription>> GetUserSubscriptionsAsync(Guid userId, CancellationToken cancellationToken);
        Task<Subscription?> GetByUsersAsync(Guid followerId, Guid followingId, CancellationToken cancellationToken);
        Task<bool> IsSubscribedAsync(Guid followerId, Guid followingId, CancellationToken cancellationToken);
        Task<Subscription> CreateAsync(Subscription subscription, CancellationToken cancellationToken);
        Task<Subscription> UpdateAsync(Subscription subscription, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    }
}