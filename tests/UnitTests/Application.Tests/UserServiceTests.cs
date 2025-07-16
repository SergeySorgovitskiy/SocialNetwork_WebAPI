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
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly UserService _userService;
        private readonly CancellationToken _ct = CancellationToken.None;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _userService = new UserService(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldCreate_WhenValid()
        {
            var createDto = new CreateUserDto
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "password123",
                Bio = "Test bio",
                IsPrivate = false
            };
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = createDto.UserName,
                Email = createDto.Email,
                Bio = createDto.Bio,
                IsPrivate = createDto.IsPrivate,
                CreatedAt = DateTime.UtcNow
            };

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(createDto.Email, _ct)).ReturnsAsync((User)null);
            _userRepositoryMock.Setup(r => r.GetByUsernameAsync(createDto.UserName, _ct)).ReturnsAsync((User)null);
            _userRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<User>(), _ct)).ReturnsAsync(user);

            var result = await _userService.CreateUserAsync(createDto, _ct);

            result.Should().NotBeNull();
            result.UserName.Should().Be(createDto.UserName);
            result.Email.Should().Be(createDto.Email);
            result.Bio.Should().Be(createDto.Bio);
            result.IsPrivate.Should().Be(createDto.IsPrivate);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrow_WhenEmailExists()
        {
            var createDto = new CreateUserDto
            {
                UserName = "testuser",
                Email = "existing@example.com",
                Password = "password123"
            };
            var existingUser = new User { Email = createDto.Email };
            _userRepositoryMock.Setup(r => r.GetByEmailAsync(createDto.Email, _ct)).ReturnsAsync(existingUser);

            Func<Task> act = async () => await _userService.CreateUserAsync(createDto, _ct);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Пользователь с таким email уже существует");
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrow_WhenUsernameExists()
        {
            var createDto = new CreateUserDto
            {
                UserName = "existinguser",
                Email = "test@example.com",
                Password = "password123"
            };
            _userRepositoryMock.Setup(r => r.GetByEmailAsync(createDto.Email, _ct)).ReturnsAsync((User)null);
            _userRepositoryMock.Setup(r => r.GetByUsernameAsync(createDto.UserName, _ct)).ReturnsAsync(new User { UserName = createDto.UserName });

            Func<Task> act = async () => await _userService.CreateUserAsync(createDto, _ct);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Пользователь с таким username уже существует");
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldUpdate_WhenValid()
        {
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                UserName = "olduser",
                Email = "test@example.com",
                Bio = "Old bio"
            };
            var updateDto = new UpdateUserDto
            {
                UserName = "newuser",
                Bio = "New bio",
                AvatarUrl = "new-avatar.jpg",
                IsPrivate = true
            };
            var updatedUser = new User
            {
                Id = userId,
                UserName = updateDto.UserName,
                Email = user.Email,
                Bio = updateDto.Bio,
                AvatarUrl = updateDto.AvatarUrl,
                IsPrivate = updateDto.IsPrivate.Value
            };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, _ct)).ReturnsAsync(user);
            _userRepositoryMock.Setup(r => r.GetByUsernameAsync(updateDto.UserName, _ct)).ReturnsAsync((User)null);
            _userRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<User>(), _ct)).ReturnsAsync(updatedUser);

            var result = await _userService.UpdateUserAsync(userId, updateDto, _ct);

            result.Should().NotBeNull();
            result.UserName.Should().Be(updateDto.UserName);
            result.Bio.Should().Be(updateDto.Bio);
            result.AvatarUrl.Should().Be(updateDto.AvatarUrl);
            result.IsPrivate.Should().Be(updateDto.IsPrivate.Value);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldThrow_WhenUserNotFound()
        {
            var userId = Guid.NewGuid();
            var updateDto = new UpdateUserDto { UserName = "newuser" };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, _ct)).ReturnsAsync((User)null);

            Func<Task> act = async () => await _userService.UpdateUserAsync(userId, updateDto, _ct);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Пользователь не найден");
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldThrow_WhenUsernameTaken()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, UserName = "olduser" };
            var updateDto = new UpdateUserDto { UserName = "existinguser" };
            var existingUser = new User { Id = Guid.NewGuid(), UserName = updateDto.UserName };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, _ct)).ReturnsAsync(user);
            _userRepositoryMock.Setup(r => r.GetByUsernameAsync(updateDto.UserName, _ct)).ReturnsAsync(existingUser);

            Func<Task> act = async () => await _userService.UpdateUserAsync(userId, updateDto, _ct);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Пользователь с таким username уже существует");
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldReturnTrue_WhenUserExists()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id = userId };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, _ct)).ReturnsAsync(user);
            _userRepositoryMock.Setup(r => r.DeleteAsync(userId, _ct)).Returns(Task.CompletedTask);

            var result = await _userService.DeleteUserAsync(userId, _ct);

            result.Should().BeTrue();
            _userRepositoryMock.Verify(r => r.DeleteAsync(userId, _ct), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldReturnFalse_WhenUserNotFound()
        {
            var userId = Guid.NewGuid();
            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, _ct)).ReturnsAsync((User)null);

            var result = await _userService.DeleteUserAsync(userId, _ct);

            result.Should().BeFalse();
            _userRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturn_WhenExists()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, UserName = "testuser", Email = "test@example.com" };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, _ct)).ReturnsAsync(user);

            var result = await _userService.GetUserByIdAsync(userId, _ct);

            result.Should().NotBeNull();
            result.Id.Should().Be(userId);
            result.UserName.Should().Be(user.UserName);
            result.Email.Should().Be(user.Email);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            var userId = Guid.NewGuid();
            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, _ct)).ReturnsAsync((User)null);

            var result = await _userService.GetUserByIdAsync(userId, _ct);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnList()
        {
            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), UserName = "user1" },
                new User { Id = Guid.NewGuid(), UserName = "user2" }
            };
            _userRepositoryMock.Setup(r => r.GetAllAsync(_ct)).ReturnsAsync(users);

            var result = await _userService.GetAllUsersAsync(_ct);

            result.Should().HaveCount(2);
            result.Should().AllSatisfy(u => u.Should().NotBeNull());
        }
    }
}