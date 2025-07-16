using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class RepostRepository : IRepostRepository
{
    private readonly ApplicationDbContext _context;

    public RepostRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Repost?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Reposts
            .Include(r => r.User)
            .Include(r => r.OriginalPost)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Repost>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Reposts
            .Include(r => r.User)
            .Include(r => r.OriginalPost)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Repost>> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return await _context.Reposts
            .Include(r => r.User)
            .Include(r => r.OriginalPost)
            .Where(r => r.OriginalPostId == postId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Repost?> GetByUserAndPostAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default)
    {
        return await _context.Reposts
            .Include(r => r.User)
            .Include(r => r.OriginalPost)
            .FirstOrDefaultAsync(r => r.UserId == userId && r.OriginalPostId == postId, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default)
    {
        return await _context.Reposts
            .AnyAsync(r => r.UserId == userId && r.OriginalPostId == postId, cancellationToken);
    }

    public async Task<Repost> AddAsync(Repost repost, CancellationToken cancellationToken = default)
    {
        _context.Reposts.Add(repost);
        await _context.SaveChangesAsync(cancellationToken);
        return repost;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var repost = await _context.Reposts.FindAsync(new object[] { id }, cancellationToken);
        if (repost != null)
        {
            _context.Reposts.Remove(repost);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> GetRepostCountAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return await _context.Reposts
            .CountAsync(r => r.OriginalPostId == postId, cancellationToken);
    }
}