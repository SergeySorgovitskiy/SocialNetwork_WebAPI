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
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly ApplicationDbContext _context;
        public SubscriptionRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Subscription>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.Subscriptions
                .Include(s => s.Follower)
                .Include(s => s.Following)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(cancellationToken);
        }
        public async Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Subscriptions
                .Include(s => s.Follower)
                .Include(s => s.Following)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }
        public async Task<IEnumerable<Subscription>> GetFollowersAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.Subscriptions
                .Include(s => s.Follower)
                .Where(s => s.FollowingId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(cancellationToken);
        }
        public async Task<IEnumerable<Subscription>> GetFollowingAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.Subscriptions
                .Include(s => s.Following)
                .Where(s => s.FollowerId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(cancellationToken);
        }
        public async Task<IEnumerable<Subscription>> GetUserSubscriptionsAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.Subscriptions
                .Include(s => s.Following)
                .Where(s => s.FollowerId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(cancellationToken);
        }
        public async Task<Subscription?> GetByUsersAsync(Guid followerId, Guid followingId, CancellationToken cancellationToken)
        {
            return await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.FollowerId == followerId && s.FollowingId == followingId, cancellationToken);
        }
        public async Task<bool> IsSubscribedAsync(Guid followerId, Guid followingId, CancellationToken cancellationToken)
        {
            return await _context.Subscriptions
                .AnyAsync(s => s.FollowerId == followerId && s.FollowingId == followingId, cancellationToken);
        }
        public async Task<Subscription> CreateAsync(Subscription subscription, CancellationToken cancellationToken)
        {
            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync(cancellationToken);
            return subscription;
        }
        public async Task<Subscription> UpdateAsync(Subscription subscription, CancellationToken cancellationToken)
        {
            _context.Subscriptions.Update(subscription);
            await _context.SaveChangesAsync(cancellationToken);
            return subscription;
        }
        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var subscription = await _context.Subscriptions.FindAsync(new object[] { id }, cancellationToken);
            if (subscription != null)
            {
                _context.Subscriptions.Remove(subscription);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
