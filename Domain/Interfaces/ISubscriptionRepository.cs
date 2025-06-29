using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ISubscriptionRepository
    {
        Task<IEnumerable<Subscription>> GetAllAsync();
        Task<Subscription?> GetByIdAsync(Guid id);
        Task<IEnumerable<Subscription>> GetFollowersAsync(Guid userId);
        Task<IEnumerable<Subscription>> GetFollowingAsync(Guid userId);
        Task<Subscription?> GetByUsersAsync(Guid followerId, Guid followingId);
        Task<Subscription> CreateAsync(Subscription subscription);
        Task<Subscription> UpdateAsync(Subscription subscription);
        Task DeleteAsync(Guid id);
    }
}