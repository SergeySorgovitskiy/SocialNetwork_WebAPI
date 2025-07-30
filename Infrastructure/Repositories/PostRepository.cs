using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly ApplicationDbContext _context;

        public PostRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Post>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.Posts
                .Include(p => p.Author)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Post>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
        {
            return await _context.Posts
                .Include(p => p.Author)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Post>> GetByAuthorIdAsync(Guid authorId, CancellationToken cancellationToken)
        {
            return await _context.Posts
                .Include(p => p.Author)
                .Where(p => p.AuthorId == authorId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Post>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken)
        {
            return await _context.Posts
                .Include(p => p.Author)
                .Where(p => p.AuthorId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Post>> GetPostsByUserIdsAsync(IEnumerable<Guid> userIds, int page, int pageSize, CancellationToken cancellationToken)
        {
            return await _context.Posts
                .Include(p => p.Author)
                .Where(p => userIds.Contains(p.AuthorId))
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<Post> CreateAsync(Post post, CancellationToken cancellationToken)
        {
            _context.Posts.Add(post);
            await _context.SaveChangesAsync(cancellationToken);
            return post;
        }

        public async Task<Post> UpdateAsync(Post post, CancellationToken cancellationToken)
        {
            _context.Posts.Update(post);
            await _context.SaveChangesAsync(cancellationToken);
            return post;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var post = await _context.Posts.FindAsync(new object[] { id }, cancellationToken);
            if (post != null)
            {
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken)
        {
            return await _context.Posts.CountAsync(cancellationToken);
        }

        public async Task<int> GetCountByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.Posts.CountAsync(p => p.AuthorId == userId, cancellationToken);
        }

        public async Task<int> GetPostsCountByUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken)
        {
            return await _context.Posts.CountAsync(p => userIds.Contains(p.AuthorId), cancellationToken);
        }
    }
}
