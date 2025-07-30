using Application.Common.DTOs;
using Application.Common.Configuration;
using Domain.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtService _jwtService;
        private readonly JwtSettings _jwtSettings;
        private readonly IEmailService _emailService;
        private readonly Dictionary<string, (string Email, DateTime Expiry)> _resetTokens = new();

        public AuthService(IUserRepository userRepository, JwtService jwtService, IOptions<JwtSettings> jwtSettings, IEmailService emailService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _jwtSettings = jwtSettings.Value;
            _emailService = emailService;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(loginDto.Email, cancellationToken);
            if (user == null)
                return null;

            if (!VerifyPassword(loginDto.Password, user.PasswordHash))
                return null;

            return await GenerateAuthResponseAsync(user, cancellationToken);
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken)
        {
           
            var existingUserByEmail = await _userRepository.GetByEmailAsync(registerDto.Email, cancellationToken);
            if (existingUserByEmail != null)
                return null;

            var existingUserByUsername = await _userRepository.GetByUserNameAsync(registerDto.UserName, cancellationToken);
            if (existingUserByUsername != null)
                return null;

      
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                PasswordHash = HashPassword(registerDto.Password),
                Bio = registerDto.Bio,
                AvatarUrl = registerDto.AvatarUrl,
                IsPrivate = registerDto.IsPrivate,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

          
            try
            {
                await _emailService.SendWelcomeEmailAsync(user.Email, user.UserName, cancellationToken);
            }
            catch (Exception ex)
            {
            
                Console.WriteLine($"Ошибка отправки приветственного письма: {ex.Message}");
            }

            return await GenerateAuthResponseAsync(user, cancellationToken);
        }

        public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByRefreshTokenAsync(refreshToken, cancellationToken);
            if (user == null || user.RefreshTokenExpiry <= DateTime.UtcNow)
                return null;

            return await GenerateAuthResponseAsync(user, cancellationToken);
        }

        public async Task<bool> LogoutAsync(Guid userId, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return false;

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _userRepository.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<bool> ForgotPasswordAsync(string email, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Начинаем процесс сброса пароля для email: {email}");

            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                Console.WriteLine($"Пользователь с email {email} не найден");
                return false;
            }

            Console.WriteLine($"Пользователь найден: {user.UserName}");

            
            var resetToken = GenerateResetToken();
            var expiry = DateTime.UtcNow.AddHours(1); 

            Console.WriteLine($"Сгенерирован токен сброса: {resetToken.Substring(0, 10)}...");

            
            _resetTokens[resetToken] = (email, expiry);

            
            try
            {
                Console.WriteLine($"Начинаем отправку email на {email}");
                await _emailService.SendPasswordResetEmailAsync(email, resetToken, cancellationToken);
                Console.WriteLine($"Email успешно отправлен на {email}");
                return true;
            }
            catch (TimeoutException ex)
            {
                Console.WriteLine($"Таймаут при отправке email: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка отправки email для сброса пароля: {ex.Message}");
                Console.WriteLine($"Тип исключения: {ex.GetType().Name}");
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto, CancellationToken cancellationToken)
        {
            
            if (!_resetTokens.TryGetValue(resetPasswordDto.Token, out var tokenInfo))
                return false;

            if (tokenInfo.Expiry < DateTime.UtcNow)
            {
                _resetTokens.Remove(resetPasswordDto.Token);
                return false;
            }

            if (tokenInfo.Email != resetPasswordDto.Email)
                return false;

            var user = await _userRepository.GetByEmailAsync(resetPasswordDto.Email, cancellationToken);
            if (user == null)
                return false;

            
            user.PasswordHash = HashPassword(resetPasswordDto.NewPassword);
            await _userRepository.SaveChangesAsync(cancellationToken);

            
            _resetTokens.Remove(resetPasswordDto.Token);

            return true;
        }

        private async Task<AuthResponseDto> GenerateAuthResponseAsync(User user, CancellationToken cancellationToken)
        {
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

           
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
            await _userRepository.SaveChangesAsync(cancellationToken);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                RefreshTokenExpiresAt = user.RefreshTokenExpiry.Value,
                User = new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    AvatarUrl = user.AvatarUrl,
                    Bio = user.Bio,
                    IsPrivate = user.IsPrivate,
                    CreatedAt = user.CreatedAt
                }
            };
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword == passwordHash;
        }

        private string GenerateResetToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }
    }
}