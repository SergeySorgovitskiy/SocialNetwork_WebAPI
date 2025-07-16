using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ApplicationDbContext _context;

        public CommentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Comment>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.Comments
                .Include(c => c.Author)
                .Include(c => c.Post)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Comment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Comments
                .Include(c => c.Author)
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Comment>> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken)
        {
            return await _context.Comments
                .Include(c => c.Author)
                .Where(c => c.PostId == postId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Comment>> GetByAuthorIdAsync(Guid authorId, CancellationToken cancellationToken)
        {
            return await _context.Comments
                .Include(c => c.Post)
                .Where(c => c.AuthorId == authorId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Comment> CreateAsync(Comment comment, CancellationToken cancellationToken)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync(cancellationToken);
            return comment;
        }

        public async Task<Comment> UpdateAsync(Comment comment, CancellationToken cancellationToken)
        {
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync(cancellationToken);
            return comment;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var comment = await _context.Comments.FindAsync(new object[] { id }, cancellationToken);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
