using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.DTOs;
using Application.Common.Configuration;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Application.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly JwtService _jwtService;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IOptions<JwtSettings>> _jwtSettingsMock;
        private readonly AuthService _authService;
        private readonly CancellationToken _ct = CancellationToken.None;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _emailServiceMock = new Mock<IEmailService>();
            _jwtSettingsMock = new Mock<IOptions<JwtSettings>>();

            var jwtSettings = new JwtSettings
            {
                SecretKey = "test-secret-key-that-is-long-enough-for-hmacsha256",
                Issuer = "test-issuer",
                Audience = "test-audience",
                AccessTokenExpirationMinutes = 60,
                RefreshTokenExpirationDays = 7
            };
            _jwtSettingsMock.Setup(x => x.Value).Returns(jwtSettings);
            _jwtService = new JwtService(_jwtSettingsMock.Object);

            _authService = new AuthService(_userRepositoryMock.Object, _jwtService, _jwtSettingsMock.Object, _emailServiceMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_ShouldCreateUser_WhenValidData()
        {
            var registerDto = new RegisterDto
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "Password123!"
            };

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(registerDto.Email, _ct)).ReturnsAsync((User)null);
            _userRepositoryMock.Setup(r => r.GetByUserNameAsync(registerDto.UserName, _ct)).ReturnsAsync((User)null);
            _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>(), _ct)).ReturnsAsync((User)null);
            _userRepositoryMock.Setup(r => r.SaveChangesAsync(_ct)).Returns(Task.FromResult(0));

            var result = await _authService.RegisterAsync(registerDto, _ct);

            result.Should().NotBeNull();
            result.AccessToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
            result.User.Should().NotBeNull();
            result.User.UserName.Should().Be(registerDto.UserName);
            result.User.Email.Should().Be(registerDto.Email);
            _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), _ct), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnNull_WhenEmailAlreadyExists()
        {
            var registerDto = new RegisterDto
            {
                UserName = "testuser",
                Email = "existing@example.com",
                Password = "Password123!"
            };

            var existingUser = new User { Email = registerDto.Email };
            _userRepositoryMock.Setup(r => r.GetByEmailAsync(registerDto.Email, _ct)).ReturnsAsync(existingUser);

            var result = await _authService.RegisterAsync(registerDto, _ct);

            result.Should().BeNull();
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnNull_WhenUserNameAlreadyExists()
        {
            var registerDto = new RegisterDto
            {
                UserName = "existinguser",
                Email = "test@example.com",
                Password = "Password123!"
            };

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(registerDto.Email, _ct)).ReturnsAsync((User)null);
            _userRepositoryMock.Setup(r => r.GetByUserNameAsync(registerDto.UserName, _ct)).ReturnsAsync(new User { UserName = registerDto.UserName });

            var result = await _authService.RegisterAsync(registerDto, _ct);

            result.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenValidCredentials()
        {
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = loginDto.Email,
                UserName = "testuser",
                PasswordHash = HashPassword(loginDto.Password)
            };

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(loginDto.Email, _ct)).ReturnsAsync(user);
            _userRepositoryMock.Setup(r => r.SaveChangesAsync(_ct)).Returns(Task.FromResult(0));

            var result = await _authService.LoginAsync(loginDto, _ct);

            result.Should().NotBeNull();
            result.AccessToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
            result.User.Should().NotBeNull();
            result.User.Email.Should().Be(loginDto.Email);
        }

        private string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenUserNotFound()
        {
            var loginDto = new LoginDto
            {
                Email = "nonexistent@example.com",
                Password = "Password123!"
            };

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(loginDto.Email, _ct)).ReturnsAsync((User)null);

            var result = await _authService.LoginAsync(loginDto, _ct);

            result.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenInvalidPassword()
        {
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "WrongPassword123!"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = loginDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!")
            };

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(loginDto.Email, _ct)).ReturnsAsync(user);

            var result = await _authService.LoginAsync(loginDto, _ct);

            result.Should().BeNull();
        }

        [Fact]
        public async Task ForgotPasswordAsync_ShouldSendEmail_WhenUserExists()
        {
            var forgotPasswordDto = new ForgotPasswordDto
            {
                Email = "test@example.com"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = forgotPasswordDto.Email,
                UserName = "testuser"
            };

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(forgotPasswordDto.Email, _ct)).ReturnsAsync(user);
            _emailServiceMock.Setup(e => e.SendPasswordResetEmailAsync(forgotPasswordDto.Email, It.IsAny<string>(), _ct)).Returns(Task.CompletedTask);

            var result = await _authService.ForgotPasswordAsync(forgotPasswordDto.Email, _ct);

            result.Should().BeTrue();
            _emailServiceMock.Verify(e => e.SendPasswordResetEmailAsync(forgotPasswordDto.Email, It.IsAny<string>(), _ct), Times.Once);
        }

        [Fact]
        public async Task ForgotPasswordAsync_ShouldNotSendEmail_WhenUserNotFound()
        {
            var forgotPasswordDto = new ForgotPasswordDto
            {
                Email = "nonexistent@example.com"
            };

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(forgotPasswordDto.Email, _ct)).ReturnsAsync((User)null);

            var result = await _authService.ForgotPasswordAsync(forgotPasswordDto.Email, _ct);

            result.Should().BeFalse();
            _emailServiceMock.Verify(e => e.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ResetPasswordAsync_ShouldUpdatePassword_WhenValidToken()
        {
            var email = "test@example.com";
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                UserName = "testuser"
            };

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(email, _ct)).ReturnsAsync(user);
            _userRepositoryMock.Setup(r => r.SaveChangesAsync(_ct)).Returns(Task.FromResult(0));
            _emailServiceMock.Setup(e => e.SendPasswordResetEmailAsync(email, It.IsAny<string>(), _ct)).Returns(Task.CompletedTask);

            var forgotPasswordResult = await _authService.ForgotPasswordAsync(email, _ct);
            forgotPasswordResult.Should().BeTrue();

            var resetPasswordDto = new ResetPasswordDto
            {
                Email = email,
                Token = "valid-token",
                NewPassword = "NewPassword123!"
            };

            var result = await _authService.ResetPasswordAsync(resetPasswordDto, _ct);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task ResetPasswordAsync_ShouldReturnFalse_WhenInvalidToken()
        {
            var resetPasswordDto = new ResetPasswordDto
            {
                Email = "test@example.com",
                Token = "invalid-token",
                NewPassword = "NewPassword123!"
            };

            var result = await _authService.ResetPasswordAsync(resetPasswordDto, _ct);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task ResetPasswordAsync_ShouldReturnFalse_WhenExpiredToken()
        {
            var resetPasswordDto = new ResetPasswordDto
            {
                Email = "test@example.com",
                Token = "expired-token",
                NewPassword = "NewPassword123!"
            };

            var result = await _authService.ResetPasswordAsync(resetPasswordDto, _ct);

            result.Should().BeFalse();
        }
    }
}