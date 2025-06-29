using Application.Common.DTOs;
using Domain.Interfaces;
using Domain.Entities;

namespace Application.Services
{
    public class SubscriptionService
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IUserRepository _userRepository;

        public SubscriptionService(ISubscriptionRepository subscriptionRepository, IUserRepository userRepository)
        {
            _subscriptionRepository = subscriptionRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<SubscriptionDto>> GetAllSubscriptionsAsync()
        {
            var subscriptions = await _subscriptionRepository.GetAllAsync();
            return subscriptions.Select(MapToDto);
        }

        public async Task<SubscriptionDto?> GetSubscriptionByIdAsync(Guid id)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(id);
            return subscription != null ? MapToDto(subscription) : null;
        }

        public async Task<IEnumerable<SubscriptionDto>> GetFollowersAsync(Guid userId)
        {
            var followers = await _subscriptionRepository.GetFollowersAsync(userId);
            return followers.Select(MapToDto);
        }

        public async Task<IEnumerable<SubscriptionDto>> GetFollowingAsync(Guid userId)
        {
            var following = await _subscriptionRepository.GetFollowingAsync(userId);
            return following.Select(MapToDto);
        }

        public async Task<SubscriptionDto> CreateSubscriptionAsync(CreateSubscriptionDto createSubscriptionDto)
        {
            var follower = await _userRepository.GetByIdAsync(createSubscriptionDto.FollowerId);
            if (follower == null)
                throw new InvalidOperationException("Подписчик не найден");

            var following = await _userRepository.GetByIdAsync(createSubscriptionDto.FollowingId);
            if (following == null)
                throw new InvalidOperationException("Пользователь для подписки не найден");

            var existingSubscription = await _subscriptionRepository.GetByUsersAsync(createSubscriptionDto.FollowerId, createSubscriptionDto.FollowingId);
            if (existingSubscription != null)
                throw new InvalidOperationException("Подписка уже существует");

            var subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                FollowerId = createSubscriptionDto.FollowerId,
                FollowingId = createSubscriptionDto.FollowingId,
                Status = following.IsPrivate ? "pending" : "approved",
                CreatedAt = DateTime.UtcNow
            };

            var createdSubscription = await _subscriptionRepository.CreateAsync(subscription);
            return MapToDto(createdSubscription);
        }

        public async Task<SubscriptionDto> UpdateSubscriptionAsync(Guid id, UpdateSubscriptionDto updateSubscriptionDto)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(id);
            if (subscription == null)
                throw new InvalidOperationException("Подписка не найдена");

            subscription.Status = updateSubscriptionDto.Status;

            var updatedSubscription = await _subscriptionRepository.UpdateAsync(subscription);
            return MapToDto(updatedSubscription);
        }

        public async Task DeleteSubscriptionAsync(Guid id)
        {
            await _subscriptionRepository.DeleteAsync(id);
        }

        private static SubscriptionDto MapToDto(Subscription subscription)
        {
            return new SubscriptionDto
            {
                Id = subscription.Id,
                FollowerId = subscription.FollowerId,
                FollowerUsername = subscription.Follower?.Username ?? "Неизвестный пользователь",
                FollowingId = subscription.FollowingId,
                FollowingUsername = subscription.Following?.Username ?? "Неизвестный пользователь",
                Status = subscription.Status,
                CreatedAt = subscription.CreatedAt
            };
        }
    }
}