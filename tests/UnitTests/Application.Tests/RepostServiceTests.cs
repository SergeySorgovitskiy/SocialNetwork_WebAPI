using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Application.Tests
{
    public class RepostServiceTests
    {
        private readonly Mock<IRepostRepository> _repostRepositoryMock;
        private readonly Mock<IPostRepository> _postRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly RepostService _repostService;
        private readonly CancellationToken _ct = CancellationToken.None;

        public RepostServiceTests()
        {
            _repostRepositoryMock = new Mock<IRepostRepository>();
            _postRepositoryMock = new Mock<IPostRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _repostService = new RepostService(_repostRepositoryMock.Object, _postRepositoryMock.Object, _userRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateRepostAsync_ShouldCreate_WhenValid()
        {
            var userId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            var user = new User { Id = userId };
            var post = new Post { Id = postId, RepostCount = 0 };
            var repost = new Repost { Id = Guid.NewGuid(), UserId = userId, OriginalPostId = postId, Comment = "test comment" };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, _ct)).ReturnsAsync(user);
            _postRepositoryMock.Setup(r => r.GetByIdAsync(postId, _ct)).ReturnsAsync(post);
            _repostRepositoryMock.Setup(r => r.GetByUserAndPostAsync(userId, postId, _ct)).ReturnsAsync((Repost)null);
            _repostRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Repost>(), _ct)).ReturnsAsync(repost);
            _repostRepositoryMock.Setup(r => r.GetRepostCountAsync(postId, _ct)).ReturnsAsync(1);
            _postRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Post>(), _ct)).ReturnsAsync(post);

            var result = await _repostService.CreateRepostAsync(userId, postId, "test comment", _ct);

            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            result.OriginalPostId.Should().Be(postId);
            result.Comment.Should().Be("test comment");
        }

        [Fact]
        public async Task CreateRepostAsync_ShouldThrow_WhenUserNotFound()
        {
            var userId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, _ct)).ReturnsAsync((User)null);

            Func<Task> act = async () => await _repostService.CreateRepostAsync(userId, postId, "comment", _ct);
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Пользователь не найден");
        }

        [Fact]
        public async Task CreateRepostAsync_ShouldThrow_WhenPostNotFound()
        {
            var userId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            var user = new User { Id = userId };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, _ct)).ReturnsAsync(user);
            _postRepositoryMock.Setup(r => r.GetByIdAsync(postId, _ct)).ReturnsAsync((Post)null);

            Func<Task> act = async () => await _repostService.CreateRepostAsync(userId, postId, "comment", _ct);
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Пост не найден");
        }

        [Fact]
        public async Task CreateRepostAsync_ShouldThrow_WhenAlreadyReposted()
        {
            var userId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            var user = new User { Id = userId };
            var post = new Post { Id = postId };
            var existingRepost = new Repost { Id = Guid.NewGuid(), UserId = userId, OriginalPostId = postId };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, _ct)).ReturnsAsync(user);
            _postRepositoryMock.Setup(r => r.GetByIdAsync(postId, _ct)).ReturnsAsync(post);
            _repostRepositoryMock.Setup(r => r.GetByUserAndPostAsync(userId, postId, _ct)).ReturnsAsync(existingRepost);

            Func<Task> act = async () => await _repostService.CreateRepostAsync(userId, postId, "comment", _ct);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Пользователь уже репостил этот пост");
        }

        [Fact]
        public async Task DeleteRepostAsync_ShouldDelete_WhenValid()
        {
            var userId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            var repost = new Repost { Id = Guid.NewGuid(), UserId = userId, OriginalPostId = postId };
            var post = new Post { Id = postId, RepostCount = 1 };
            _repostRepositoryMock.Setup(r => r.GetByUserAndPostAsync(userId, postId, _ct)).ReturnsAsync(repost);
            _repostRepositoryMock.Setup(r => r.DeleteAsync(repost.Id, _ct)).Returns(Task.CompletedTask);
            _postRepositoryMock.Setup(r => r.GetByIdAsync(postId, _ct)).ReturnsAsync(post);
            _repostRepositoryMock.Setup(r => r.GetRepostCountAsync(postId, _ct)).ReturnsAsync(0);
            _postRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Post>(), _ct)).ReturnsAsync(post);

            await _repostService.DeleteRepostAsync(userId, postId, _ct);
            _repostRepositoryMock.Verify(r => r.DeleteAsync(repost.Id, _ct), Times.Once);
        }

        [Fact]
        public async Task DeleteRepostAsync_ShouldThrow_WhenNotFound()
        {
            var userId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            _repostRepositoryMock.Setup(r => r.GetByUserAndPostAsync(userId, postId, _ct)).ReturnsAsync((Repost)null);

            Func<Task> act = async () => await _repostService.DeleteRepostAsync(userId, postId, _ct);
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Репост не найден");
        }

        [Fact]
        public async Task GetUserRepostsAsync_ShouldReturnList()
        {
            var userId = Guid.NewGuid();
            var reposts = new List<Repost> { new Repost { Id = Guid.NewGuid(), UserId = userId }, new Repost { Id = Guid.NewGuid(), UserId = userId } };
            _repostRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, _ct)).ReturnsAsync(reposts);

            var result = await _repostService.GetUserRepostsAsync(userId, _ct);
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetPostRepostsAsync_ShouldReturnList()
        {
            var postId = Guid.NewGuid();
            var reposts = new List<Repost> { new Repost { Id = Guid.NewGuid(), OriginalPostId = postId }, new Repost { Id = Guid.NewGuid(), OriginalPostId = postId } };
            _repostRepositoryMock.Setup(r => r.GetByPostIdAsync(postId, _ct)).ReturnsAsync(reposts);

            var result = await _repostService.GetPostRepostsAsync(postId, _ct);
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task HasUserRepostedAsync_ShouldReturnTrue_WhenReposted()
        {
            var userId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            _repostRepositoryMock.Setup(r => r.ExistsAsync(userId, postId, _ct)).ReturnsAsync(true);

            var result = await _repostService.HasUserRepostedAsync(userId, postId, _ct);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasUserRepostedAsync_ShouldReturnFalse_WhenNotReposted()
        {
            var userId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            _repostRepositoryMock.Setup(r => r.ExistsAsync(userId, postId, _ct)).ReturnsAsync(false);

            var result = await _repostService.HasUserRepostedAsync(userId, postId, _ct);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetRepostCountAsync_ShouldReturnCount()
        {
            var postId = Guid.NewGuid();
            var expectedCount = 5;
            _repostRepositoryMock.Setup(r => r.GetRepostCountAsync(postId, _ct)).ReturnsAsync(expectedCount);

            var result = await _repostService.GetRepostCountAsync(postId, _ct);
            result.Should().Be(expectedCount);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturn_WhenExists()
        {
            var id = Guid.NewGuid();
            var repost = new Repost { Id = id };
            _repostRepositoryMock.Setup(r => r.GetByIdAsync(id, _ct)).ReturnsAsync(repost);

            var result = await _repostService.GetByIdAsync(id, _ct);
            result.Should().NotBeNull();
            result.Id.Should().Be(id);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            var id = Guid.NewGuid();
            _repostRepositoryMock.Setup(r => r.GetByIdAsync(id, _ct)).ReturnsAsync((Repost)null);

            var result = await _repostService.GetByIdAsync(id, _ct);
            result.Should().BeNull();
        }
    }
}