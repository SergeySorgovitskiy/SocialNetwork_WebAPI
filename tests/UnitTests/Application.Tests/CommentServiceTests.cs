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
    public class CommentServiceTests
    {
        private readonly Mock<ICommentRepository> _commentRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPostRepository> _postRepositoryMock;
        private readonly CommentService _commentService;
        private readonly CancellationToken _ct = CancellationToken.None;

        public CommentServiceTests()
        {
            _commentRepositoryMock = new Mock<ICommentRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _postRepositoryMock = new Mock<IPostRepository>();
            _commentService = new CommentService(_commentRepositoryMock.Object, _userRepositoryMock.Object, _postRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateCommentAsync_ShouldCreate_WhenValid()
        {
            var authorId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            var user = new User { Id = authorId, UserName = "user" };
            var post = new Post { Id = postId, AuthorId = authorId, Content = "test post" };
            var createDto = new CreateCommentDto { Content = "test comment", PostId = postId };
            var comment = new Comment { Id = Guid.NewGuid(), Content = createDto.Content, PostId = postId, AuthorId = authorId, CreatedAt = DateTime.UtcNow };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(authorId, _ct)).ReturnsAsync(user);
            _postRepositoryMock.Setup(r => r.GetByIdAsync(postId, _ct)).ReturnsAsync(post);
            _commentRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Comment>(), _ct)).ReturnsAsync(comment);

            var result = await _commentService.CreateCommentAsync(authorId, createDto, _ct);

            result.Should().NotBeNull();
            result.Content.Should().Be(createDto.Content);
        }

        [Fact]
        public async Task CreateCommentAsync_ShouldThrow_WhenUserNotFound()
        {
            var authorId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            var createDto = new CreateCommentDto { Content = "test", PostId = postId };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(authorId, _ct)).ReturnsAsync((User)null);

            Func<Task> act = async () => await _commentService.CreateCommentAsync(authorId, createDto, _ct);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Пользователь не найден");
        }

        [Fact]
        public async Task CreateCommentAsync_ShouldThrow_WhenPostNotFound()
        {
            var authorId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            var user = new User { Id = authorId };
            var createDto = new CreateCommentDto { Content = "test", PostId = postId };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(authorId, _ct)).ReturnsAsync(user);
            _postRepositoryMock.Setup(r => r.GetByIdAsync(postId, _ct)).ReturnsAsync((Post)null);

            Func<Task> act = async () => await _commentService.CreateCommentAsync(authorId, createDto, _ct);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Пост не найден");
        }

        [Fact]
        public async Task CreateCommentAsync_ShouldThrow_WhenParentCommentNotFound()
        {
            var authorId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            var parentId = Guid.NewGuid();
            var user = new User { Id = authorId };
            var post = new Post { Id = postId };
            var createDto = new CreateCommentDto { Content = "test", PostId = postId, ParentCommentId = parentId };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(authorId, _ct)).ReturnsAsync(user);
            _postRepositoryMock.Setup(r => r.GetByIdAsync(postId, _ct)).ReturnsAsync(post);
            _commentRepositoryMock.Setup(r => r.GetByIdAsync(parentId, _ct)).ReturnsAsync((Comment)null);

            Func<Task> act = async () => await _commentService.CreateCommentAsync(authorId, createDto, _ct);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Родительский комментарий не найден");
        }

        [Fact]
        public async Task CreateCommentAsync_ShouldThrow_WhenNestingLevelExceeded()
        {
            var authorId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            var parentId = Guid.NewGuid();
            var user = new User { Id = authorId };
            var post = new Post { Id = postId };
            var parentComment = new Comment { Id = parentId, NestingLevel = 3 };
            var createDto = new CreateCommentDto { Content = "test", PostId = postId, ParentCommentId = parentId };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(authorId, _ct)).ReturnsAsync(user);
            _postRepositoryMock.Setup(r => r.GetByIdAsync(postId, _ct)).ReturnsAsync(post);
            _commentRepositoryMock.Setup(r => r.GetByIdAsync(parentId, _ct)).ReturnsAsync(parentComment);

            Func<Task> act = async () => await _commentService.CreateCommentAsync(authorId, createDto, _ct);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Максимальная вложенность комментариев составляет 3 уровней");
        }

        [Fact]
        public async Task UpdateCommentAsync_ShouldUpdate_WhenAuthor()
        {
            var commentId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            var comment = new Comment { Id = commentId, AuthorId = authorId, Content = "old" };
            var updateDto = new UpdateCommentDto { Content = "new" };
            var updatedComment = new Comment { Id = commentId, AuthorId = authorId, Content = "new" };
            _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId, _ct)).ReturnsAsync(comment);
            _commentRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Comment>(), _ct)).ReturnsAsync(updatedComment);

            var result = await _commentService.UpdateCommentAsync(commentId, authorId, updateDto, _ct);
            result.Content.Should().Be("new");
        }

        [Fact]
        public async Task UpdateCommentAsync_ShouldThrow_WhenNotAuthor()
        {
            var commentId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            var comment = new Comment { Id = commentId, AuthorId = Guid.NewGuid(), Content = "old" };
            var updateDto = new UpdateCommentDto { Content = "new" };
            _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId, _ct)).ReturnsAsync(comment);

            Func<Task> act = async () => await _commentService.UpdateCommentAsync(commentId, authorId, updateDto, _ct);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Вы не можете редактировать чужой комментарий");
        }

        [Fact]
        public async Task UpdateCommentAsync_ShouldThrow_WhenNotFound()
        {
            var commentId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            var updateDto = new UpdateCommentDto { Content = "new" };
            _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId, _ct)).ReturnsAsync((Comment)null);

            Func<Task> act = async () => await _commentService.UpdateCommentAsync(commentId, authorId, updateDto, _ct);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Комментарий не найден");
        }

        [Fact]
        public async Task DeleteCommentAsync_ShouldDelete_WhenAuthor()
        {
            var commentId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            var comment = new Comment { Id = commentId, AuthorId = authorId };
            _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId, _ct)).ReturnsAsync(comment);

            await _commentService.DeleteCommentAsync(commentId, authorId, _ct);
            _commentRepositoryMock.Verify(r => r.DeleteAsync(commentId, _ct), Times.Once);
        }

        [Fact]
        public async Task DeleteCommentAsync_ShouldThrow_WhenNotAuthor()
        {
            var commentId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            var comment = new Comment { Id = commentId, AuthorId = Guid.NewGuid() };
            _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId, _ct)).ReturnsAsync(comment);

            Func<Task> act = async () => await _commentService.DeleteCommentAsync(commentId, authorId, _ct);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Вы не можете удалить чужой комментарий");
        }

        [Fact]
        public async Task DeleteCommentAsync_ShouldThrow_WhenNotFound()
        {
            var commentId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId, _ct)).ReturnsAsync((Comment)null);

            Func<Task> act = async () => await _commentService.DeleteCommentAsync(commentId, authorId, _ct);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Комментарий не найден");
        }

        [Fact]
        public async Task GetCommentByIdAsync_ShouldReturn_WhenExists()
        {
            var commentId = Guid.NewGuid();
            var comment = new Comment { Id = commentId, Content = "test" };
            _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId, _ct)).ReturnsAsync(comment);

            var result = await _commentService.GetCommentByIdAsync(commentId, _ct);
            result.Should().NotBeNull();
            result.Id.Should().Be(commentId);
        }

        [Fact]
        public async Task GetCommentByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            var commentId = Guid.NewGuid();
            _commentRepositoryMock.Setup(r => r.GetByIdAsync(commentId, _ct)).ReturnsAsync((Comment)null);

            var result = await _commentService.GetCommentByIdAsync(commentId, _ct);
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetCommentsByPostIdAsync_ShouldReturnList()
        {
            var postId = Guid.NewGuid();
            var comments = new List<Comment> { new Comment { Id = Guid.NewGuid(), PostId = postId }, new Comment { Id = Guid.NewGuid(), PostId = postId } };
            _commentRepositoryMock.Setup(r => r.GetByPostIdAsync(postId, _ct)).ReturnsAsync(comments);

            var result = await _commentService.GetCommentsByPostIdAsync(postId, _ct);
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllCommentsAsync_ShouldReturnList()
        {
            var comments = new List<Comment> { new Comment { Id = Guid.NewGuid() }, new Comment { Id = Guid.NewGuid() } };
            _commentRepositoryMock.Setup(r => r.GetAllAsync(_ct)).ReturnsAsync(comments);

            var result = await _commentService.GetAllCommentsAsync(_ct);
            result.Should().HaveCount(2);
        }
    }
}