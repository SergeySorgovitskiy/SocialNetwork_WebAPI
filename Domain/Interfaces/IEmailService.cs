namespace Domain.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string email, string resetToken, CancellationToken cancellationToken = default);
    Task SendWelcomeEmailAsync(string email, string userName, CancellationToken cancellationToken = default);
    Task<bool> IsEmailValidAsync(string email, CancellationToken cancellationToken = default);
}