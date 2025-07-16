using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infrastructure.Repositories
{
    public class LikeRepository : ILikeRepository
    {
        private readonly ApplicationDbContext _context;
        public LikeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Like like, CancellationToken cancellationToken)
        {
            await _context.AddAsync(like, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveAsync(Guid postId, Guid userId, CancellationToken cancellationToken)
        {
            var like = await _context.Likes
            .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId, cancellationToken);

            if (like != null)
            {
                _context.Likes.Remove(like);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<bool> ExistsAsync(Guid postId, Guid userId, CancellationToken cancellationToken)
        {
            return await _context.Likes
                .AnyAsync(l => l.PostId == postId && l.UserId == userId, cancellationToken);
        }

        public async Task<int> GetLikesCountAsync(Guid postId, CancellationToken cancellationToken)
        {
            return await _context.Likes
                .CountAsync(l => l.PostId == postId, cancellationToken);
        }

        public async Task<List<Like>> GetLikesForPostAsync(Guid postId, CancellationToken cancellationToken)
        {
            return await _context.Likes
                .Where(l => l.PostId == postId)
                .ToListAsync(cancellationToken);
        }
    }
}


