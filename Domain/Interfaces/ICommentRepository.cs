using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ICommentRepository
    {
        Task<IEnumerable<Comment>> GetAllAsync();
        Task<Comment?> GetByIdAsync(Guid id);
        Task<IEnumerable<Comment>> GetByPostIdAsync(Guid postId);
        Task<IEnumerable<Comment>> GetByAuthorIdAsync(Guid authorId);
        Task<Comment> CreateAsync(Comment comment);
        Task<Comment> UpdateAsync(Comment comment);
        Task DeleteAsync(Guid id);
    }
}