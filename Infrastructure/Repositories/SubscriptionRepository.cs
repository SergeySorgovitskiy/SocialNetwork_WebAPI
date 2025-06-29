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

        public async Task<IEnumerable<Subscription>> GetAllAsync()
        {
            return await _context.Subscriptions
                .Include(s => s.Follower)
                .Include(s => s.Following)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<Subscription?> GetByIdAsync(Guid id)
        {
            return await _context.Subscriptions
                .Include(s => s.Follower)
                .Include(s => s.Following)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Subscription>> GetFollowersAsync(Guid userId)
        {
            return await _context.Subscriptions
                .Include(s => s.Follower)
                .Where(s => s.FollowingId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Subscription>> GetFollowingAsync(Guid userId)
        {
            return await _context.Subscriptions
                .Include(s => s.Following)
                .Where(s => s.FollowerId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<Subscription?> GetByUsersAsync(Guid followerId, Guid followingId)
        {
            return await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.FollowerId == followerId && s.FollowingId == followingId);
        }

        public async Task<Subscription> CreateAsync(Subscription subscription)
        {
            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();
            return subscription;
        }

        public async Task<Subscription> UpdateAsync(Subscription subscription)
        {
            _context.Subscriptions.Update(subscription);
            await _context.SaveChangesAsync();
            return subscription;
        }

        public async Task DeleteAsync(Guid id)
        {
            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription != null)
            {
                _context.Subscriptions.Remove(subscription);
                await _context.SaveChangesAsync();
            }
        }
    }
}
