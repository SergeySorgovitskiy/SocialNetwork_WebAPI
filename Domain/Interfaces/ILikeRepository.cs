using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ILikeRepository
    {
        Task AddAsync(Like like, CancellationToken cancellationToken);
        Task RemoveAsync(Guid postId, Guid userId, CancellationToken cancellationToken);
        Task<bool> ExistsAsync(Guid postId, Guid userId, CancellationToken cancellationToken);
        Task<int> GetLikesCountAsync(Guid postId, CancellationToken cancellationToken);
        Task<List<Like>> GetLikesForPostAsync(Guid postId, CancellationToken cancellationToken);

    }
}
