using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.DTOs;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Application.Tests
{
    public class PostServiceTests
    {
        private readonly Mock<IPostRepository> _postRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly PostService _postService;
        private readonly CancellationToken _ct = CancellationToken.None;

        public PostServiceTests()
        {
            _postRepositoryMock = new Mock<IPostRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _postService = new PostService(_postRepositoryMock.Object, _userRepositoryMock.Object);
        }

        [Fact]
        public async Task CreatePostAsync_ShouldCreatePost_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, UserName = "testuser" };
            var createDto = new CreatePostDto { Content = "Hello #world!", MediaUrls = new List<string> { "url1" } };
            var post = new Post { Id = Guid.NewGuid(), Content = createDto.Content, AuthorId = userId, MediaUrls = new HashSet<string>(createDto.MediaUrls), Hashtags = new HashSet<string> { "world" }, CreatedAt = DateTime.UtcNow };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, _ct)).ReturnsAsync(user);
            _postRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Post>(), _ct)).ReturnsAsync(post);

            // Act
            var result = await _postService.CreatePostAsync(userId, createDto, _ct);

            // Assert
            result.Should().NotBeNull();
            result.Content.Should().Be(createDto.Content);
            result.Hashtags.Should().Contain("world");
            _userRepositoryMock.Verify(r => r.GetByIdAsync(userId, _ct), Times.Once);
            _postRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Post>(), _ct), Times.Once);
        }

        [Fact]
        public async Task CreatePostAsync_ShouldThrow_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var createDto = new CreatePostDto { Content = "Test post" };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, _ct)).ReturnsAsync((User)null);

            // Act
            Func<Task> act = async () => await _postService.CreatePostAsync(userId, createDto, _ct);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Пользователь не найден");
        }

        [Fact]
        public async Task GetPostByIdAsync_ShouldReturnPost_WhenExists()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var post = new Post { Id = postId, Content = "Test", AuthorId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
            _postRepositoryMock.Setup(r => r.GetByIdAsync(postId, _ct)).ReturnsAsync(post);

            // Act
            var result = await _postService.GetPostByIdAsync(postId, _ct);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(postId);
        }

        [Fact]
        public async Task GetPostByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            var postId = Guid.NewGuid();
            _postRepositoryMock.Setup(r => r.GetByIdAsync(postId, _ct)).ReturnsAsync((Post)null);

            // Act
            var result = await _postService.GetPostByIdAsync(postId, _ct);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdatePostAsync_ShouldUpdatePost_WhenAuthorMatches()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            var post = new Post { Id = postId, AuthorId = authorId, Content = "Old content" };
            var updateDto = new UpdatePostDto { Content = "New content #updated", MediaUrls = new List<string>() };
            var updatedPost = new Post { Id = postId, AuthorId = authorId, Content = updateDto.Content, Hashtags = new HashSet<string> { "updated" } };

            _postRepositoryMock.Setup(r => r.GetByIdAsync(postId, _ct)).ReturnsAsync(post);
            _postRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Post>(), _ct)).ReturnsAsync(updatedPost);

            // Act
            var result = await _postService.UpdatePostAsync(postId, authorId, updateDto, _ct);

            // Assert
            result.Content.Should().Be(updateDto.Content);
            result.Hashtags.Should().Contain("updated");
            _postRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Post>(), _ct), Times.Once);
        }

        [Fact]
        public async Task UpdatePostAsync_ShouldThrow_WhenNotAuthor()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            var post = new Post { Id = postId, AuthorId = Guid.NewGuid() };
            var updateDto = new UpdatePostDto { Content = "New content", MediaUrls = new List<string>() };
            _postRepositoryMock.Setup(r => r.GetByIdAsync(postId, _ct)).ReturnsAsync(post);

            // Act
            Func<Task> act = async () => await _postService.UpdatePostAsync(postId, authorId, updateDto, _ct);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Вы не можете редактировать чужой пост");
        }

        [Fact]
        public async Task UpdatePostAsync_ShouldThrow_WhenPostNotFound()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            var updateDto = new UpdatePostDto { Content = "New content", MediaUrls = new List<string>() };
            _postRepositoryMock.Setup(r => r.GetByIdAsync(postId, _ct)).ReturnsAsync((Post)null);

            // Act
            Func<Task> act = async () => await _postService.UpdatePostAsync(postId, authorId, updateDto, _ct);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Пост не найден");
        }

        [Fact]
        public async Task DeletePostAsync_ShouldDelete_WhenAuthorMatches()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            var post = new Post { Id = postId, AuthorId = authorId };
            _postRepositoryMock.Setup(r => r.GetByIdAsync(postId, _ct)).ReturnsAsync(post);

            // Act
            await _postService.DeletePostAsync(postId, authorId, _ct);

            // Assert
            _postRepositoryMock.Verify(r => r.DeleteAsync(postId, _ct), Times.Once);
        }

        [Fact]
        public async Task DeletePostAsync_ShouldThrow_WhenNotAuthor()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            var post = new Post { Id = postId, AuthorId = Guid.NewGuid() };
            _postRepositoryMock.Setup(r => r.GetByIdAsync(postId, _ct)).ReturnsAsync(post);

            // Act
            Func<Task> act = async () => await _postService.DeletePostAsync(postId, authorId, _ct);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Вы не можете удалить чужой пост");
        }

        [Fact]
        public async Task DeletePostAsync_ShouldThrow_WhenPostNotFound()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            _postRepositoryMock.Setup(r => r.GetByIdAsync(postId, _ct)).ReturnsAsync((Post)null);

            // Act
            Func<Task> act = async () => await _postService.DeletePostAsync(postId, authorId, _ct);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Пост не найден");
        }

        [Fact]
        public async Task GetPostsByAuthorIdAsync_ShouldReturnPosts()
        {
            // Arrange
            var authorId = Guid.NewGuid();
            var posts = new List<Post>
            {
                new Post { Id = Guid.NewGuid(), AuthorId = authorId, Content = "Post 1", CreatedAt = DateTime.UtcNow },
                new Post { Id = Guid.NewGuid(), AuthorId = authorId, Content = "Post 2", CreatedAt = DateTime.UtcNow }
            };
            _postRepositoryMock.Setup(r => r.GetByAuthorIdAsync(authorId, _ct)).ReturnsAsync(posts);

            // Act
            var result = await _postService.GetPostsByAuthorIdAsync(authorId, _ct);

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(p => p.AuthorId.Should().Be(authorId));
        }
    }
}