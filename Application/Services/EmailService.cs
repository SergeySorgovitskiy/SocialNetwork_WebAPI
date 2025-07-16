using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;

namespace Application.Services;

public class EmailService : IEmailService
{
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration configuration)
    {
        _smtpServer = configuration["Email:SmtpServer"] ?? "smtp.mail.ru";
        _smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "587");
        _smtpUsername = configuration["Email:Username"] ?? "";
        _smtpPassword = configuration["Email:Password"] ?? "";
        _fromEmail = configuration["Email:FromEmail"] ?? "";
        _fromName = configuration["Email:FromName"] ?? "Social Network";
    }

    public async Task SendPasswordResetEmailAsync(string email, string resetToken, CancellationToken cancellationToken = default)
    {
        var subject = "Сброс пароля";
        var body = $@"
            <html>
            <body>
                <h2>Сброс пароля</h2>
                <p>Вы запросили сброс пароля для вашего аккаунта.</p>
                <p>Для сброса пароля перейдите по следующей ссылке:</p>
                <p><a href='https://yourdomain.com/reset-password?token={resetToken}'>Сбросить пароль</a></p>
                <p>Если вы не запрашивали сброс пароля, проигнорируйте это письмо.</p>
                <p>Ссылка действительна в течение 1 часа.</p>
            </body>
            </html>";

        await SendEmailAsync(email, subject, body, cancellationToken);
    }

    public async Task SendWelcomeEmailAsync(string email, string userName, CancellationToken cancellationToken = default)
    {
        var subject = "Добро пожаловать в Social Network!";
        var body = $@"
            <html>
            <body>
                <h2>Добро пожаловать, {userName}!</h2>
                <p>Спасибо за регистрацию в нашей социальной сети.</p>
                <p>Теперь вы можете:</p>
                <ul>
                    <li>Создавать посты</li>
                    <li>Комментировать</li>
                    <li>Ставить лайки</li>
                    <li>Подписываться на других пользователей</li>
                    <li>Использовать репосты и закладки</li>
                </ul>
                <p>Приятного использования!</p>
            </body>
            </html>";

        await SendEmailAsync(email, subject, body, cancellationToken);
    }

    public async Task<bool> IsEmailValidAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            if (!email.Contains("@") || !email.Contains("."))
                return false;

            if (email.Contains("..") || email.StartsWith(".") || email.EndsWith("."))
                return false;

            var mailAddress = new MailAddress(email);
            return mailAddress.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
        {
            Console.WriteLine($"Email would be sent to {toEmail}: {subject}");
            return;
        }

        using var client = new SmtpClient(_smtpServer, _smtpPort)
        {
            EnableSsl = _smtpPort == 465 || _smtpPort == 587,
            Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Timeout = 30000
        };

        using var message = new MailMessage
        {
            From = new MailAddress(_fromEmail, _fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        message.To.Add(toEmail);

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(30));

            await client.SendMailAsync(message, cts.Token);
            Console.WriteLine($"Email successfully sent to {toEmail}: {subject}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"Email sending to {toEmail} was cancelled due to timeout");
            throw new TimeoutException("Email sending timed out");
        }
        catch (SmtpException smtpEx)
        {
            Console.WriteLine($"SMTP error sending email to {toEmail}: {smtpEx.Message}");
            throw new InvalidOperationException($"SMTP error: {smtpEx.Message}", smtpEx);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send email to {toEmail}: {ex.Message}");
            throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
        }
    }
}