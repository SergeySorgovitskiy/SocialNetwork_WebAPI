using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IPostRepository
    {
        Task<IEnumerable<Post>> GetAllAsync(CancellationToken cancellationToken);
        Task<IEnumerable<Post>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken);
        Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<Post>> GetByAuthorIdAsync(Guid authorId, CancellationToken cancellationToken);
        Task<IEnumerable<Post>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken);
        Task<IEnumerable<Post>> GetPostsByUserIdsAsync(IEnumerable<Guid> userIds, int page, int pageSize, CancellationToken cancellationToken);
        Task<Post> CreateAsync(Post post, CancellationToken cancellationToken);
        Task<Post> UpdateAsync(Post post, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task<int> GetTotalCountAsync(CancellationToken cancellationToken);
        Task<int> GetCountByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<int> GetPostsCountByUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken);
    }
}