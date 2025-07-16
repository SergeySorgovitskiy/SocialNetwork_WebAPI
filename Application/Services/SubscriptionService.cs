using Application.Common.DTOs;
using Domain.Interfaces;
using Domain.Entities;
using Domain.Enums;

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

        public async Task<IEnumerable<SubscriptionDto>> GetAllSubscriptionsAsync(CancellationToken cancellationToken)
        {
            var subscriptions = await _subscriptionRepository.GetAllAsync(cancellationToken);
            return subscriptions.Select(MapToDto);
        }

        public async Task<SubscriptionDto?> GetSubscriptionByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(id, cancellationToken);
            return subscription != null ? MapToDto(subscription) : null;
        }

        public async Task<IEnumerable<SubscriptionDto>> GetFollowersAsync(Guid userId, CancellationToken cancellationToken)
        {
            var followers = await _subscriptionRepository.GetFollowersAsync(userId, cancellationToken);
            return followers.Select(MapToDto);
        }

        public async Task<IEnumerable<SubscriptionDto>> GetFollowingAsync(Guid userId, CancellationToken cancellationToken)
        {
            var following = await _subscriptionRepository.GetFollowingAsync(userId, cancellationToken);
            return following.Select(MapToDto);
        }

        public async Task<SubscriptionDto> CreateSubscriptionAsync(CreateSubscriptionDto createSubscriptionDto, CancellationToken cancellationToken)
        {
            var follower = await _userRepository.GetByIdAsync(createSubscriptionDto.FollowerId, cancellationToken);
            if (follower == null)
                throw new InvalidOperationException("Подписчик не найден");

            var following = await _userRepository.GetByIdAsync(createSubscriptionDto.FollowingId, cancellationToken);
            if (following == null)
                throw new InvalidOperationException("Пользователь для подписки не найден");

            var existingSubscription = await _subscriptionRepository.GetByUsersAsync(createSubscriptionDto.FollowerId, createSubscriptionDto.FollowingId, cancellationToken);
            if (existingSubscription != null)
                throw new InvalidOperationException("Подписка уже существует");

            var subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                FollowerId = createSubscriptionDto.FollowerId,
                FollowingId = createSubscriptionDto.FollowingId,
                Status = following.IsPrivate ? SubscriptionStatus.Pending : SubscriptionStatus.Approved,
                CreatedAt = DateTime.UtcNow
            };

            var createdSubscription = await _subscriptionRepository.CreateAsync(subscription, cancellationToken);
            return MapToDto(createdSubscription);
        }

        public async Task<SubscriptionDto> UpdateSubscriptionAsync(Guid id, UpdateSubscriptionDto updateSubscriptionDto, CancellationToken cancellationToken)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(id, cancellationToken);
            if (subscription == null)
                throw new InvalidOperationException("Подписка не найдена");

            subscription.Status = Enum.Parse<SubscriptionStatus>(updateSubscriptionDto.Status);

            var updatedSubscription = await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
            return MapToDto(updatedSubscription);
        }

        public async Task DeleteSubscriptionAsync(Guid id, CancellationToken cancellationToken)
        {
            await _subscriptionRepository.DeleteAsync(id, cancellationToken);
        }

        private static SubscriptionDto MapToDto(Subscription subscription)
        {
            return new SubscriptionDto
            {
                Id = subscription.Id,
                FollowerId = subscription.FollowerId,
                FollowerUserName = subscription.Follower?.UserName ?? "Неизвестный пользователь",
                FollowingId = subscription.FollowingId,
                FollowingUserName = subscription.Following?.UserName ?? "Неизвестный пользователь",
                Status = subscription.Status.ToString(),
                CreatedAt = subscription.CreatedAt
            };
        }
    }
}