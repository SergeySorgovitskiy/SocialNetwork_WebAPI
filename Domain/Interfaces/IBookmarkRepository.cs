using Domain.Entities;

namespace Domain.Interfaces;

public interface IBookmarkRepository
{
    Task<Bookmark?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Bookmark>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Bookmark?> GetByUserAndPostAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default);
    Task<Bookmark> AddAsync(Bookmark bookmark, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteByUserAndPostAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default);
}