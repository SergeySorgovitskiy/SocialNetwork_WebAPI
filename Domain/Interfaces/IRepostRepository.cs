using Domain.Entities;

namespace Domain.Interfaces;

public interface IRepostRepository
{
    Task<Repost?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Repost>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Repost>> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<Repost?> GetByUserAndPostAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default);
    Task<Repost> AddAsync(Repost repost, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> GetRepostCountAsync(Guid postId, CancellationToken cancellationToken = default);
}