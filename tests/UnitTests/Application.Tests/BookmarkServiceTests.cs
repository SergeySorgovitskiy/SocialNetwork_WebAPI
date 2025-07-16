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
    public class BookmarkServiceTests
    {
        private readonly Mock<IBookmarkRepository> _bookmarkRepositoryMock;
        private readonly Mock<IPostRepository> _postRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly BookmarkService _bookmarkService;
        private readonly CancellationToken _ct = CancellationToken.None;

        public BookmarkServiceTests()
        {
            _bookmarkRepositoryMock = new Mock<IBookmarkRepository>();
            _postRepositoryMock = new Mock<IPostRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _bookmarkService = new BookmarkService(_bookmarkRepositoryMock.Object, _postRepositoryMock.Object, _userRepositoryMock.Object);
        }

        [Fact]
        public async Task AddBookmarkAsync_ShouldCreate_WhenValid()
        {
            var userId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            var user = new User { Id = userId };
            var post = new Post { Id = postId };
            var bookmark = new Bookmark { Id = Guid.NewGuid(), UserId = userId, PostId = postId };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, _ct)).ReturnsAsync(user);
            _postRepositoryMock.Setup(r => r.GetByIdAsync(postId, _ct)).ReturnsAsync(post);
            _bookmarkRepositoryMock.Setup(r => r.GetByUserAndPostAsync(userId, postId, _ct)).ReturnsAsync((Bookmark)null);
            _bookmarkRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Bookmark>(), _ct)).ReturnsAsync(bookmark);

            var result = await _bookmarkService.AddBookmarkAsync(userId, postId, _ct);

            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            result.PostId.Should().Be(postId);
        }

        [Fact]
        public async Task AddBookmarkAsync_ShouldThrow_WhenUserNotFound()
        {
            var userId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, _ct)).ReturnsAsync((User)null);

            Func<Task> act = async () => await _bookmarkService.AddBookmarkAsync(userId, postId, _ct);
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Пользователь не найден");
        }

        [Fact]
        public async Task AddBookmarkAsync_ShouldThrow_WhenPostNotFound()
        {
            var userId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            var user = new User { Id = userId };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, _ct)).ReturnsAsync(user);
            _postRepositoryMock.Setup(r => r.GetByIdAsync(postId, _ct)).ReturnsAsync((Post)null);

            Func<Task> act = async () => await _bookmarkService.AddBookmarkAsync(userId, postId, _ct);
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Пост не найден");
        }

        [Fact]
        public async Task AddBookmarkAsync_ShouldThrow_WhenAlreadyBookmarked()
        {
            var userId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            var user = new User { Id = userId };
            var post = new Post { Id = postId };
            var existingBookmark = new Bookmark { Id = Guid.NewGuid(), UserId = userId, PostId = postId };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, _ct)).ReturnsAsync(user);
            _postRepositoryMock.Setup(r => r.GetByIdAsync(postId, _ct)).ReturnsAsync(post);
            _bookmarkRepositoryMock.Setup(r => r.GetByUserAndPostAsync(userId, postId, _ct)).ReturnsAsync(existingBookmark);

            Func<Task> act = async () => await _bookmarkService.AddBookmarkAsync(userId, postId, _ct);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Пост уже добавлен в закладки");
        }

        [Fact]
        public async Task RemoveBookmarkAsync_ShouldDelete_WhenValid()
        {
            var userId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            var bookmark = new Bookmark { Id = Guid.NewGuid(), UserId = userId, PostId = postId };
            _bookmarkRepositoryMock.Setup(r => r.GetByUserAndPostAsync(userId, postId, _ct)).ReturnsAsync(bookmark);
            _bookmarkRepositoryMock.Setup(r => r.DeleteByUserAndPostAsync(userId, postId, _ct)).Returns(Task.CompletedTask);

            await _bookmarkService.RemoveBookmarkAsync(userId, postId, _ct);
            _bookmarkRepositoryMock.Verify(r => r.DeleteByUserAndPostAsync(userId, postId, _ct), Times.Once);
        }

        [Fact]
        public async Task RemoveBookmarkAsync_ShouldThrow_WhenNotFound()
        {
            var userId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            _bookmarkRepositoryMock.Setup(r => r.GetByUserAndPostAsync(userId, postId, _ct)).ReturnsAsync((Bookmark)null);

            Func<Task> act = async () => await _bookmarkService.RemoveBookmarkAsync(userId, postId, _ct);
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Закладка не найдена");
        }

        [Fact]
        public async Task GetUserBookmarksAsync_ShouldReturnList()
        {
            var userId = Guid.NewGuid();
            var bookmarks = new List<Bookmark> { new Bookmark { Id = Guid.NewGuid(), UserId = userId }, new Bookmark { Id = Guid.NewGuid(), UserId = userId } };
            _bookmarkRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, _ct)).ReturnsAsync(bookmarks);

            var result = await _bookmarkService.GetUserBookmarksAsync(userId, _ct);
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task IsBookmarkedAsync_ShouldReturnTrue_WhenBookmarked()
        {
            var userId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            _bookmarkRepositoryMock.Setup(r => r.ExistsAsync(userId, postId, _ct)).ReturnsAsync(true);

            var result = await _bookmarkService.IsBookmarkedAsync(userId, postId, _ct);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsBookmarkedAsync_ShouldReturnFalse_WhenNotBookmarked()
        {
            var userId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            _bookmarkRepositoryMock.Setup(r => r.ExistsAsync(userId, postId, _ct)).ReturnsAsync(false);

            var result = await _bookmarkService.IsBookmarkedAsync(userId, postId, _ct);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturn_WhenExists()
        {
            var id = Guid.NewGuid();
            var bookmark = new Bookmark { Id = id };
            _bookmarkRepositoryMock.Setup(r => r.GetByIdAsync(id, _ct)).ReturnsAsync(bookmark);

            var result = await _bookmarkService.GetByIdAsync(id, _ct);
            result.Should().NotBeNull();
            result.Id.Should().Be(id);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            var id = Guid.NewGuid();
            _bookmarkRepositoryMock.Setup(r => r.GetByIdAsync(id, _ct)).ReturnsAsync((Bookmark)null);

            var result = await _bookmarkService.GetByIdAsync(id, _ct);
            result.Should().BeNull();
        }
    }
}