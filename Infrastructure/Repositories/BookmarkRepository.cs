using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class BookmarkRepository : IBookmarkRepository
{
    private readonly ApplicationDbContext _context;

    public BookmarkRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Bookmark?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Bookmarks
            .Include(b => b.User)
            .Include(b => b.Post)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Bookmark>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Bookmarks
            .Include(b => b.User)
            .Include(b => b.Post)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Bookmark?> GetByUserAndPostAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default)
    {
        return await _context.Bookmarks
            .Include(b => b.User)
            .Include(b => b.Post)
            .FirstOrDefaultAsync(b => b.UserId == userId && b.PostId == postId, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default)
    {
        return await _context.Bookmarks
            .AnyAsync(b => b.UserId == userId && b.PostId == postId, cancellationToken);
    }

    public async Task<Bookmark> AddAsync(Bookmark bookmark, CancellationToken cancellationToken = default)
    {
        _context.Bookmarks.Add(bookmark);
        await _context.SaveChangesAsync(cancellationToken);
        return bookmark;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var bookmark = await _context.Bookmarks.FindAsync(new object[] { id }, cancellationToken);
        if (bookmark != null)
        {
            _context.Bookmarks.Remove(bookmark);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeleteByUserAndPostAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default)
    {
        var bookmark = await _context.Bookmarks
            .FirstOrDefaultAsync(b => b.UserId == userId && b.PostId == postId, cancellationToken);

        if (bookmark != null)
        {
            _context.Bookmarks.Remove(bookmark);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}