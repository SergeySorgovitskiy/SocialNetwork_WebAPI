using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Services;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Application.Tests
{
    public class NewsFeedServiceTests
    {
        private readonly Mock<IPostRepository> _postRepositoryMock;
        private readonly Mock<ISubscriptionRepository> _subscriptionRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly NewsFeedService _newsFeedService;
        private readonly CancellationToken _ct = CancellationToken.None;

        public NewsFeedServiceTests()
        {
            _postRepositoryMock = new Mock<IPostRepository>();
            _subscriptionRepositoryMock = new Mock<ISubscriptionRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _newsFeedService = new NewsFeedService(_postRepositoryMock.Object, _subscriptionRepositoryMock.Object, _userRepositoryMock.Object);
        }

        [Fact]
        public async Task GetPersonalFeedAsync_ShouldReturnPosts_WhenValid()
        {
            var userId = Guid.NewGuid();
            var filter = new NewsFeedFilter { Page = 1, PageSize = 10 };
            var subscriptions = new List<Subscription>
            {
                new Subscription { FollowingId = Guid.NewGuid() },
                new Subscription { FollowingId = Guid.NewGuid() }
            };
            var posts = new List<Post>
            {
                new Post { Id = Guid.NewGuid(), Content = "Post 1", CreatedAt = DateTime.UtcNow },
                new Post { Id = Guid.NewGuid(), Content = "Post 2", CreatedAt = DateTime.UtcNow }
            };

            _subscriptionRepositoryMock.Setup(r => r.GetUserSubscriptionsAsync(userId, _ct)).ReturnsAsync(subscriptions);
            _postRepositoryMock.Setup(r => r.GetPostsByUserIdsAsync(It.IsAny<List<Guid>>(), filter.Page, filter.PageSize, _ct)).ReturnsAsync(posts);
            _postRepositoryMock.Setup(r => r.GetPostsCountByUserIdsAsync(It.IsAny<List<Guid>>(), _ct)).ReturnsAsync(2);

            var result = await _newsFeedService.GetPersonalFeedAsync(userId, filter, _ct);

            result.Should().NotBeNull();
            result.Posts.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.Page.Should().Be(filter.Page);
            result.PageSize.Should().Be(filter.PageSize);
        }

        [Fact]
        public async Task GetGlobalFeedAsync_ShouldReturnPosts_WhenValid()
        {
            var filter = new NewsFeedFilter { Page = 1, PageSize = 10 };
            var posts = new List<Post>
            {
                new Post { Id = Guid.NewGuid(), Content = "Global Post 1", CreatedAt = DateTime.UtcNow },
                new Post { Id = Guid.NewGuid(), Content = "Global Post 2", CreatedAt = DateTime.UtcNow }
            };

            _postRepositoryMock.Setup(r => r.GetAllAsync(filter.Page, filter.PageSize, _ct)).ReturnsAsync(posts);
            _postRepositoryMock.Setup(r => r.GetTotalCountAsync(_ct)).ReturnsAsync(2);

            var result = await _newsFeedService.GetGlobalFeedAsync(filter, _ct);

            result.Should().NotBeNull();
            result.Posts.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetUserFeedAsync_ShouldReturnPosts_WhenPublicUser()
        {
            var userId = Guid.NewGuid();
            var targetUserId = Guid.NewGuid();
            var filter = new NewsFeedFilter { Page = 1, PageSize = 10 };
            var targetUser = new User { Id = targetUserId, IsPrivate = false };
            var posts = new List<Post>
            {
                new Post { Id = Guid.NewGuid(), Content = "User Post 1", CreatedAt = DateTime.UtcNow },
                new Post { Id = Guid.NewGuid(), Content = "User Post 2", CreatedAt = DateTime.UtcNow }
            };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(targetUserId, _ct)).ReturnsAsync(targetUser);
            _postRepositoryMock.Setup(r => r.GetByUserIdAsync(targetUserId, filter.Page, filter.PageSize, _ct)).ReturnsAsync(posts);
            _postRepositoryMock.Setup(r => r.GetCountByUserIdAsync(targetUserId, _ct)).ReturnsAsync(2);

            var result = await _newsFeedService.GetUserFeedAsync(userId, targetUserId, filter, _ct);

            result.Should().NotBeNull();
            result.Posts.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetUserFeedAsync_ShouldThrow_WhenUserNotFound()
        {
            var userId = Guid.NewGuid();
            var targetUserId = Guid.NewGuid();
            var filter = new NewsFeedFilter { Page = 1, PageSize = 10 };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(targetUserId, _ct)).ReturnsAsync((User)null);

            Func<Task> act = async () => await _newsFeedService.GetUserFeedAsync(userId, targetUserId, filter, _ct);
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Пользователь не найден");
        }

        [Fact]
        public async Task GetUserFeedAsync_ShouldThrow_WhenPrivateUserNotFollowing()
        {
            var userId = Guid.NewGuid();
            var targetUserId = Guid.NewGuid();
            var filter = new NewsFeedFilter { Page = 1, PageSize = 10 };
            var targetUser = new User { Id = targetUserId, IsPrivate = true };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(targetUserId, _ct)).ReturnsAsync(targetUser);
            _subscriptionRepositoryMock.Setup(r => r.IsSubscribedAsync(userId, targetUserId, _ct)).ReturnsAsync(false);

            Func<Task> act = async () => await _newsFeedService.GetUserFeedAsync(userId, targetUserId, filter, _ct);
            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Профиль пользователя является приватным");
        }

        [Fact]
        public async Task GetUserFeedAsync_ShouldReturnPosts_WhenPrivateUserFollowing()
        {
            var userId = Guid.NewGuid();
            var targetUserId = Guid.NewGuid();
            var filter = new NewsFeedFilter { Page = 1, PageSize = 10 };
            var targetUser = new User { Id = targetUserId, IsPrivate = true };
            var posts = new List<Post>
            {
                new Post { Id = Guid.NewGuid(), Content = "Private User Post", CreatedAt = DateTime.UtcNow }
            };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(targetUserId, _ct)).ReturnsAsync(targetUser);
            _subscriptionRepositoryMock.Setup(r => r.IsSubscribedAsync(userId, targetUserId, _ct)).ReturnsAsync(true);
            _postRepositoryMock.Setup(r => r.GetByUserIdAsync(targetUserId, filter.Page, filter.PageSize, _ct)).ReturnsAsync(posts);
            _postRepositoryMock.Setup(r => r.GetCountByUserIdAsync(targetUserId, _ct)).ReturnsAsync(1);

            var result = await _newsFeedService.GetUserFeedAsync(userId, targetUserId, filter, _ct);

            result.Should().NotBeNull();
            result.Posts.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetUserFeedAsync_ShouldReturnPosts_WhenPrivateUserOwnProfile()
        {
            var userId = Guid.NewGuid();
            var filter = new NewsFeedFilter { Page = 1, PageSize = 10 };
            var targetUser = new User { Id = userId, IsPrivate = true };
            var posts = new List<Post>
            {
                new Post { Id = Guid.NewGuid(), Content = "Own Private Post", CreatedAt = DateTime.UtcNow }
            };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, _ct)).ReturnsAsync(targetUser);
            _postRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, filter.Page, filter.PageSize, _ct)).ReturnsAsync(posts);
            _postRepositoryMock.Setup(r => r.GetCountByUserIdAsync(userId, _ct)).ReturnsAsync(1);

            var result = await _newsFeedService.GetUserFeedAsync(userId, userId, filter, _ct);

            result.Should().NotBeNull();
            result.Posts.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetPersonalFeedAsync_ShouldApplySearchFilter()
        {
            var userId = Guid.NewGuid();
            var filter = new NewsFeedFilter { Page = 1, PageSize = 10, SearchQuery = "test" };
            var subscriptions = new List<Subscription>();
            var posts = new List<Post>
            {
                new Post { Id = Guid.NewGuid(), Content = "Test post", CreatedAt = DateTime.UtcNow },
                new Post { Id = Guid.NewGuid(), Content = "Another post", CreatedAt = DateTime.UtcNow }
            };

            _subscriptionRepositoryMock.Setup(r => r.GetUserSubscriptionsAsync(userId, _ct)).ReturnsAsync(subscriptions);
            _postRepositoryMock.Setup(r => r.GetPostsByUserIdsAsync(It.IsAny<List<Guid>>(), filter.Page, filter.PageSize, _ct)).ReturnsAsync(posts);
            _postRepositoryMock.Setup(r => r.GetPostsCountByUserIdsAsync(It.IsAny<List<Guid>>(), _ct)).ReturnsAsync(1);

            var result = await _newsFeedService.GetPersonalFeedAsync(userId, filter, _ct);

            result.Should().NotBeNull();
            result.Posts.Should().HaveCount(1);
            foreach (var p in result.Posts)
            {
                p.Content.ToLower().Should().Contain("test");
            }
        }

        [Fact]
        public async Task GetPersonalFeedAsync_ShouldApplyHashtagFilter()
        {
            var userId = Guid.NewGuid();
            var filter = new NewsFeedFilter { Page = 1, PageSize = 10, Hashtag = "test" };
            var subscriptions = new List<Subscription>();
            var posts = new List<Post>
            {
                new Post { Id = Guid.NewGuid(), Content = "Post with #test", Hashtags = new HashSet<string> { "test" }, CreatedAt = DateTime.UtcNow },
                new Post { Id = Guid.NewGuid(), Content = "Post with #other", Hashtags = new HashSet<string> { "other" }, CreatedAt = DateTime.UtcNow }
            };

            _subscriptionRepositoryMock.Setup(r => r.GetUserSubscriptionsAsync(userId, _ct)).ReturnsAsync(subscriptions);
            _postRepositoryMock.Setup(r => r.GetPostsByUserIdsAsync(It.IsAny<List<Guid>>(), filter.Page, filter.PageSize, _ct)).ReturnsAsync(posts);
            _postRepositoryMock.Setup(r => r.GetPostsCountByUserIdsAsync(It.IsAny<List<Guid>>(), _ct)).ReturnsAsync(1);

            var result = await _newsFeedService.GetPersonalFeedAsync(userId, filter, _ct);

            result.Should().NotBeNull();
            result.Posts.Should().HaveCount(1);
            result.Posts.Should().AllSatisfy(p => p.Hashtags.Should().Contain("test"));
        }
    }
}