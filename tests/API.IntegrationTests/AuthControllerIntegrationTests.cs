using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Application.Common.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;
using WebApi;

namespace API.IntegrationTests
{
    public class AuthControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public AuthControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_ShouldReturnSuccess_WhenValidData()
        {
            var registerDto = new RegisterDto
            {
                UserName = $"testuser_{Guid.NewGuid():N}",
                Email = $"test{Guid.NewGuid():N}@example.com",
                Password = "Password123!"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            result.Should().NotBeNull();
            result!.User.Should().NotBeNull();
            result.User.UserName.Should().Be(registerDto.UserName);
            result.User.Email.Should().Be(registerDto.Email);
            result.AccessToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenEmailExists()
        {
            var registerDto = new RegisterDto
            {
                UserName = $"testuser_{Guid.NewGuid():N}",
                Email = "existing@example.com",
                Password = "Password123!"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenUsernameExists()
        {
            var registerDto = new RegisterDto
            {
                UserName = "existinguser",
                Email = $"test{Guid.NewGuid():N}@example.com",
                Password = "Password123!"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_ShouldReturnSuccess_WhenValidCredentials()
        {
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            result.Should().NotBeNull();
            result!.AccessToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenInvalidCredentials()
        {
            var loginDto = new LoginDto
            {
                Email = "nonexistent@example.com",
                Password = "WrongPassword123!"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ForgotPassword_ShouldReturnSuccess_WhenValidEmail()
        {
            var forgotPasswordDto = new ForgotPasswordDto
            {
                Email = "test@example.com"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", forgotPasswordDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ForgotPassword_ShouldReturnBadRequest_WhenEmailNotFound()
        {
            var forgotPasswordDto = new ForgotPasswordDto
            {
                Email = "nonexistent@example.com"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", forgotPasswordDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task RefreshToken_ShouldReturnSuccess_WhenValidToken()
        {
            var refreshTokenDto = new RefreshTokenDto
            {
                RefreshToken = "valid-refresh-token"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshTokenDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task RefreshToken_ShouldReturnUnauthorized_WhenInvalidToken()
        {
            var refreshTokenDto = new RefreshTokenDto
            {
                RefreshToken = "invalid-refresh-token"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshTokenDto);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}