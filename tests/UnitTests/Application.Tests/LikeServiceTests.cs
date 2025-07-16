using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;
using System.Collections.Generic;

namespace Application.Tests
{
    public class LikeServiceTests
    {
        private readonly Mock<ILikeRepository> _likeRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPostRepository> _postRepositoryMock;
        private readonly LikeService _likeService;
        private readonly CancellationToken _ct = CancellationToken.None;

        public LikeServiceTests()
        {
            _likeRepositoryMock = new Mock<ILikeRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _postRepositoryMock = new Mock<IPostRepository>();
            _likeService = new LikeService(_likeRepositoryMock.Object, _userRepositoryMock.Object, _postRepositoryMock.Object);
        }

        [Fact]
        public async Task AddLikeAsync_ShouldReturnTrue_WhenValidData()
        {
           
            var userId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            var user = new User { Id = userId, UserName = "testuser" };
            var post = new Post { Id = postId, AuthorId = Guid.NewGuid(), Content = "Test post" };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, _ct)).ReturnsAsync(user);
            _postRepositoryMock.Setup(r => r.GetByIdAsync(postId, _ct)).ReturnsAsync(post);
            _likeRepositoryMock.Setup(r => r.ExistsAsync(postId, userId, _ct)).ReturnsAsync(false);
            _likeRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Like>(), _ct)).Returns(Task.CompletedTask);

           
            var result = await _likeService.AddLikeAsync(postId, userId, _ct);

          
            result.Should().BeTrue();
            _likeRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Like>(), _ct), Times.Once);
        }

        [Fact]
        public async Task AddLikeAsync_ShouldReturnFalse_WhenAlreadyLiked()
        {
           
            var userId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            var user = new User { Id = userId, UserName = "testuser" };
            var post = new Post { Id = postId, AuthorId = Guid.NewGuid(), Content = "Test post" };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, _ct)).ReturnsAsync(user);
            _postRepositoryMock.Setup(r => r.GetByIdAsync(postId, _ct)).ReturnsAsync(post);
            _likeRepositoryMock.Setup(r => r.ExistsAsync(postId, userId, _ct)).ReturnsAsync(true);

          
            var result = await _likeService.AddLikeAsync(postId, userId, _ct);

          
            result.Should().BeFalse();
            _likeRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Like>(), _ct), Times.Never);
        }

        [Fact]
        public async Task AddLikeAsync_ShouldThrow_WhenUserNotFound()
        {
            var userId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            var post = new Post { Id = postId, AuthorId = Guid.NewGuid(), Content = "Test post" };

            _postRepositoryMock.Setup(r => r.GetByIdAsync(postId, _ct)).ReturnsAsync(post);
            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, _ct)).ReturnsAsync((User)null);

            Func<Task> act = async () => await _likeService.AddLikeAsync(postId, userId, _ct);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Пользователь не найден");
        }

        [Fact]
        public async Task AddLikeAsync_ShouldThrow_WhenPostNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            var user = new User { Id = userId, UserName = "testuser" };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, _ct)).ReturnsAsync(user);
            _postRepositoryMock.Setup(r => r.GetByIdAsync(postId, _ct)).ReturnsAsync((Post)null);

            // Act
            Func<Task> act = async () => await _likeService.AddLikeAsync(postId, userId, _ct);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Пост не найден");
        }

        [Fact]
        public async Task RemoveLikeAsync_ShouldReturnTrue_WhenLikeExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            _likeRepositoryMock.Setup(r => r.ExistsAsync(postId, userId, _ct)).ReturnsAsync(true);
            _likeRepositoryMock.Setup(r => r.RemoveAsync(postId, userId, _ct)).Returns(Task.CompletedTask);

            // Act
            var result = await _likeService.RemoveLikeAsync(postId, userId, _ct);

            // Assert
            result.Should().BeTrue();
            _likeRepositoryMock.Verify(r => r.RemoveAsync(postId, userId, _ct), Times.Once);
        }

        [Fact]
        public async Task RemoveLikeAsync_ShouldReturnFalse_WhenLikeNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            _likeRepositoryMock.Setup(r => r.ExistsAsync(postId, userId, _ct)).ReturnsAsync(false);

            // Act
            var result = await _likeService.RemoveLikeAsync(postId, userId, _ct);

            // Assert
            result.Should().BeFalse();
            _likeRepositoryMock.Verify(r => r.RemoveAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetLikesCountAsync_ShouldReturnCount()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var expectedCount = 5;
            _likeRepositoryMock.Setup(r => r.GetLikesCountAsync(postId, _ct)).ReturnsAsync(expectedCount);

            // Act
            var result = await _likeService.GetLikesCountAsync(postId, _ct);

            // Assert
            result.Should().Be(expectedCount);
        }

        [Fact]
        public async Task IsPostLikedByUserAsync_ShouldReturnTrue_WhenLiked()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            _likeRepositoryMock.Setup(r => r.ExistsAsync(postId, userId, _ct)).ReturnsAsync(true);

            // Act
            var result = await _likeService.IsPostLikedByUserAsync(postId, userId, _ct);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsPostLikedByUserAsync_ShouldReturnFalse_WhenNotLiked()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            _likeRepositoryMock.Setup(r => r.ExistsAsync(postId, userId, _ct)).ReturnsAsync(false);

            // Act
            var result = await _likeService.IsPostLikedByUserAsync(postId, userId, _ct);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetLikesForPostAsync_ShouldReturnLikes()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var likes = new List<Like>
            {
                new Like { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), PostId = postId, CreatedAt = DateTime.UtcNow },
                new Like { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), PostId = postId, CreatedAt = DateTime.UtcNow }
            };

            _likeRepositoryMock.Setup(r => r.GetLikesForPostAsync(postId, _ct)).ReturnsAsync(likes);

            // Act
            var result = await _likeService.GetLikesForPostAsync(postId, _ct);

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(l => l.PostId.Should().Be(postId));
        }
    }
}