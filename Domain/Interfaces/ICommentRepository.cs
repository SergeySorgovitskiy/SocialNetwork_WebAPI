using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ICommentRepository
    {
        Task<IEnumerable<Comment>> GetAllAsync(CancellationToken cancellationToken);
        Task<Comment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<Comment>> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken);
        Task<IEnumerable<Comment>> GetByAuthorIdAsync(Guid authorId, CancellationToken cancellationToken);
        Task<Comment> CreateAsync(Comment comment, CancellationToken cancellationToken);
        Task<Comment> UpdateAsync(Comment comment, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    }
}