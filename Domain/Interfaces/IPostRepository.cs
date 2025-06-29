using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IPostRepository
    {
        Task<IEnumerable<Post>> GetAllAsync();
        Task<Post?> GetByIdAsync(Guid id);
        Task<IEnumerable<Post>> GetByAuthorIdAsync(Guid authorId);
        Task<Post> CreateAsync(Post post);
        Task<Post> UpdateAsync(Post post);
        Task DeleteAsync(Guid id);
    }
}