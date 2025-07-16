using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.DTOs;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Application.Tests
{
    public class SubscriptionServiceTests
    {
        private readonly Mock<ISubscriptionRepository> _subscriptionRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly SubscriptionService _subscriptionService;
        private readonly CancellationToken _ct = CancellationToken.None;

        public SubscriptionServiceTests()
        {
            _subscriptionRepositoryMock = new Mock<ISubscriptionRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _subscriptionService = new SubscriptionService(_subscriptionRepositoryMock.Object, _userRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateSubscriptionAsync_ShouldCreate_WhenValid()
        {
            var followerId = Guid.NewGuid();
            var followingId = Guid.NewGuid();
            var follower = new User { Id = followerId };
            var following = new User { Id = followingId, IsPrivate = false };
            var createDto = new CreateSubscriptionDto { FollowerId = followerId, FollowingId = followingId };
            var subscription = new Subscription { Id = Guid.NewGuid(), FollowerId = followerId, FollowingId = followingId, Status = SubscriptionStatus.Approved };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(followerId, _ct)).ReturnsAsync(follower);
            _userRepositoryMock.Setup(r => r.GetByIdAsync(followingId, _ct)).ReturnsAsync(following);
            _subscriptionRepositoryMock.Setup(r => r.GetByUsersAsync(followerId, followingId, _ct)).ReturnsAsync((Subscription)null);
            _subscriptionRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Subscription>(), _ct)).ReturnsAsync(subscription);

            var result = await _subscriptionService.CreateSubscriptionAsync(createDto, _ct);
            result.Should().NotBeNull();
            result.FollowerId.Should().Be(followerId);
            result.FollowingId.Should().Be(followingId);
        }

        [Fact]
        public async Task CreateSubscriptionAsync_ShouldThrow_WhenFollowerNotFound()
        {
            var followerId = Guid.NewGuid();
            var followingId = Guid.NewGuid();
            var createDto = new CreateSubscriptionDto { FollowerId = followerId, FollowingId = followingId };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(followerId, _ct)).ReturnsAsync((User)null);

            Func<Task> act = async () => await _subscriptionService.CreateSubscriptionAsync(createDto, _ct);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Подписчик не найден");
        }

        [Fact]
        public async Task CreateSubscriptionAsync_ShouldThrow_WhenFollowingNotFound()
        {
            var followerId = Guid.NewGuid();
            var followingId = Guid.NewGuid();
            var follower = new User { Id = followerId };
            var createDto = new CreateSubscriptionDto { FollowerId = followerId, FollowingId = followingId };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(followerId, _ct)).ReturnsAsync(follower);
            _userRepositoryMock.Setup(r => r.GetByIdAsync(followingId, _ct)).ReturnsAsync((User)null);

            Func<Task> act = async () => await _subscriptionService.CreateSubscriptionAsync(createDto, _ct);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Пользователь для подписки не найден");
        }

        [Fact]
        public async Task CreateSubscriptionAsync_ShouldThrow_WhenAlreadyExists()
        {
            var followerId = Guid.NewGuid();
            var followingId = Guid.NewGuid();
            var follower = new User { Id = followerId };
            var following = new User { Id = followingId };
            var createDto = new CreateSubscriptionDto { FollowerId = followerId, FollowingId = followingId };
            var existing = new Subscription { Id = Guid.NewGuid(), FollowerId = followerId, FollowingId = followingId };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(followerId, _ct)).ReturnsAsync(follower);
            _userRepositoryMock.Setup(r => r.GetByIdAsync(followingId, _ct)).ReturnsAsync(following);
            _subscriptionRepositoryMock.Setup(r => r.GetByUsersAsync(followerId, followingId, _ct)).ReturnsAsync(existing);

            Func<Task> act = async () => await _subscriptionService.CreateSubscriptionAsync(createDto, _ct);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Подписка уже существует");
        }

        [Fact]
        public async Task UpdateSubscriptionAsync_ShouldUpdate_WhenValid()
        {
            var id = Guid.NewGuid();
            var subscription = new Subscription { Id = id, Status = SubscriptionStatus.Pending };
            var updateDto = new UpdateSubscriptionDto { Status = SubscriptionStatus.Approved.ToString() };
            var updated = new Subscription { Id = id, Status = SubscriptionStatus.Approved };
            _subscriptionRepositoryMock.Setup(r => r.GetByIdAsync(id, _ct)).ReturnsAsync(subscription);
            _subscriptionRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Subscription>(), _ct)).ReturnsAsync(updated);

            var result = await _subscriptionService.UpdateSubscriptionAsync(id, updateDto, _ct);
            result.Status.Should().Be(SubscriptionStatus.Approved.ToString());
        }

        [Fact]
        public async Task UpdateSubscriptionAsync_ShouldThrow_WhenNotFound()
        {
            var id = Guid.NewGuid();
            var updateDto = new UpdateSubscriptionDto { Status = SubscriptionStatus.Approved.ToString() };
            _subscriptionRepositoryMock.Setup(r => r.GetByIdAsync(id, _ct)).ReturnsAsync((Subscription)null);

            Func<Task> act = async () => await _subscriptionService.UpdateSubscriptionAsync(id, updateDto, _ct);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Подписка не найдена");
        }

        [Fact]
        public async Task DeleteSubscriptionAsync_ShouldCallRepo()
        {
            var id = Guid.NewGuid();
            await _subscriptionService.DeleteSubscriptionAsync(id, _ct);
            _subscriptionRepositoryMock.Verify(r => r.DeleteAsync(id, _ct), Times.Once);
        }

        [Fact]
        public async Task GetAllSubscriptionsAsync_ShouldReturnList()
        {
            var list = new List<Subscription> { new Subscription { Id = Guid.NewGuid() }, new Subscription { Id = Guid.NewGuid() } };
            _subscriptionRepositoryMock.Setup(r => r.GetAllAsync(_ct)).ReturnsAsync(list);

            var result = await _subscriptionService.GetAllSubscriptionsAsync(_ct);
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetSubscriptionByIdAsync_ShouldReturn_WhenExists()
        {
            var id = Guid.NewGuid();
            var subscription = new Subscription { Id = id };
            _subscriptionRepositoryMock.Setup(r => r.GetByIdAsync(id, _ct)).ReturnsAsync(subscription);

            var result = await _subscriptionService.GetSubscriptionByIdAsync(id, _ct);
            result.Should().NotBeNull();
            result.Id.Should().Be(id);
        }

        [Fact]
        public async Task GetSubscriptionByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            var id = Guid.NewGuid();
            _subscriptionRepositoryMock.Setup(r => r.GetByIdAsync(id, _ct)).ReturnsAsync((Subscription)null);

            var result = await _subscriptionService.GetSubscriptionByIdAsync(id, _ct);
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetFollowersAsync_ShouldReturnList()
        {
            var userId = Guid.NewGuid();
            var list = new List<Subscription> { new Subscription { Id = Guid.NewGuid(), FollowingId = userId }, new Subscription { Id = Guid.NewGuid(), FollowingId = userId } };
            _subscriptionRepositoryMock.Setup(r => r.GetFollowersAsync(userId, _ct)).ReturnsAsync(list);

            var result = await _subscriptionService.GetFollowersAsync(userId, _ct);
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetFollowingAsync_ShouldReturnList()
        {
            var userId = Guid.NewGuid();
            var list = new List<Subscription> { new Subscription { Id = Guid.NewGuid(), FollowerId = userId }, new Subscription { Id = Guid.NewGuid(), FollowerId = userId } };
            _subscriptionRepositoryMock.Setup(r => r.GetFollowingAsync(userId, _ct)).ReturnsAsync(list);

            var result = await _subscriptionService.GetFollowingAsync(userId, _ct);
            result.Should().HaveCount(2);
        }
    }
}