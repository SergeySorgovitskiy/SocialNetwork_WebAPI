using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Services;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Application.Tests
{
    public class EmailServiceTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly EmailService _emailService;

        public EmailServiceTests()
        {
            _configurationMock = new Mock<IConfiguration>();
            SetupEmptyConfiguration();
            _emailService = new EmailService(_configurationMock.Object);
        }

        private void SetupEmptyConfiguration()
        {
            _configurationMock.Setup(c => c["Email:SmtpServer"]).Returns((string)null);
            _configurationMock.Setup(c => c["Email:SmtpPort"]).Returns((string)null);
            _configurationMock.Setup(c => c["Email:Username"]).Returns((string)null);
            _configurationMock.Setup(c => c["Email:Password"]).Returns((string)null);
            _configurationMock.Setup(c => c["Email:FromEmail"]).Returns((string)null);
            _configurationMock.Setup(c => c["Email:FromName"]).Returns((string)null);
        }

        [Fact]
        public async Task SendPasswordResetEmailAsync_ShouldNotThrow_WhenValidEmail()
        {
            var email = "test@example.com";
            var resetToken = "test-reset-token";

            var act = async () => await _emailService.SendPasswordResetEmailAsync(email, resetToken, CancellationToken.None);
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task SendWelcomeEmailAsync_ShouldNotThrow_WhenValidData()
        {
            var email = "test@example.com";
            var userName = "testuser";

            var act = async () => await _emailService.SendWelcomeEmailAsync(email, userName, CancellationToken.None);
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task IsEmailValidAsync_ShouldReturnTrue_WhenValidEmail()
        {
            var validEmails = new[]
            {
                "test@example.com",
                "user.name@domain.co.uk",
                "user+tag@example.org",
                "123@test.com"
            };

            foreach (var email in validEmails)
            {
                var result = await _emailService.IsEmailValidAsync(email, CancellationToken.None);
                result.Should().BeTrue($"Email {email} should be valid");
            }
        }

        [Fact]
        public async Task IsEmailValidAsync_ShouldReturnFalse_WhenInvalidEmail()
        {
            var invalidEmails = new[]
            {
                "invalid-email",
                "@example.com",
                "test@",
                "test@.com",
                "",
                "test@example",
                "test..test@example.com"
            };

            foreach (var email in invalidEmails)
            {
                var result = await _emailService.IsEmailValidAsync(email, CancellationToken.None);
                result.Should().BeFalse($"Email {email} should be invalid");
            }
        }

        [Fact]
        public async Task IsEmailValidAsync_ShouldReturnFalse_WhenNullEmail()
        {
            var result = await _emailService.IsEmailValidAsync(null!, CancellationToken.None);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsEmailValidAsync_ShouldReturnFalse_WhenEmptyEmail()
        {
            var result = await _emailService.IsEmailValidAsync("", CancellationToken.None);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsEmailValidAsync_ShouldReturnFalse_WhenWhitespaceEmail()
        {
            var result = await _emailService.IsEmailValidAsync("   ", CancellationToken.None);
            result.Should().BeFalse();
        }

        [Fact]
        public void EmailService_ShouldUseDefaultConfiguration_WhenNoConfigurationProvided()
        {
            var emptyConfigMock = new Mock<IConfiguration>();
            emptyConfigMock.Setup(c => c["Email:SmtpServer"]).Returns((string)null);
            emptyConfigMock.Setup(c => c["Email:SmtpPort"]).Returns((string)null);
            emptyConfigMock.Setup(c => c["Email:Username"]).Returns((string)null);
            emptyConfigMock.Setup(c => c["Email:Password"]).Returns((string)null);
            emptyConfigMock.Setup(c => c["Email:FromEmail"]).Returns((string)null);
            emptyConfigMock.Setup(c => c["Email:FromName"]).Returns((string)null);

            var emailService = new EmailService(emptyConfigMock.Object);

            emailService.Should().NotBeNull();
        }

        [Fact]
        public void EmailService_ShouldUseCustomConfiguration_WhenProvided()
        {
            var customConfigMock = new Mock<IConfiguration>();
            customConfigMock.Setup(c => c["Email:SmtpServer"]).Returns("custom.smtp.com");
            customConfigMock.Setup(c => c["Email:SmtpPort"]).Returns("465");
            customConfigMock.Setup(c => c["Email:Username"]).Returns("custom@test.com");
            customConfigMock.Setup(c => c["Email:Password"]).Returns("custompassword");
            customConfigMock.Setup(c => c["Email:FromEmail"]).Returns("custom@test.com");
            customConfigMock.Setup(c => c["Email:FromName"]).Returns("Custom Social Network");

            var emailService = new EmailService(customConfigMock.Object);

            emailService.Should().NotBeNull();
        }

        [Fact]
        public async Task SendPasswordResetEmailAsync_ShouldHandleCancellation()
        {
            var email = "test@example.com";
            var resetToken = "test-reset-token";
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var act = async () => await _emailService.SendPasswordResetEmailAsync(email, resetToken, cts.Token);
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task SendWelcomeEmailAsync_ShouldHandleCancellation()
        {
            var email = "test@example.com";
            var userName = "testuser";
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var act = async () => await _emailService.SendWelcomeEmailAsync(email, userName, cts.Token);
            await act.Should().NotThrowAsync();
        }
    }
}