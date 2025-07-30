using Application.Common.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _authService.RegisterAsync(registerDto, cancellationToken);
                if (result == null)
                {
                    return BadRequest(new { message = "Пользователь с таким email или username уже существует" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при регистрации", error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _authService.LoginAsync(loginDto, cancellationToken);
                if (result == null)
                {
                    return Unauthorized(new { message = "Неверный email или пароль" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при входе", error = ex.Message });
            }
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken(RefreshTokenDto refreshTokenDto, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken, cancellationToken);
                if (result == null)
                {
                    return Unauthorized(new { message = "Недействительный refresh token" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при обновлении токена", error = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<ActionResult> Logout(CancellationToken cancellationToken)
        {
            try
            {

                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized(new { message = "Недействительный токен" });
                }

                var result = await _authService.LogoutAsync(userId, cancellationToken);
                if (!result)
                {
                    return BadRequest(new { message = "Ошибка при выходе" });
                }

                return Ok(new { message = "Успешный выход" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при выходе", error = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _authService.ForgotPasswordAsync(forgotPasswordDto.Email, cancellationToken);
                if (!result)
                {
                    return BadRequest(new { message = "Пользователь с таким email не найден" });
                }

                return Ok(new { message = "Инструкции по сбросу пароля отправлены на email" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при сбросе пароля", error = ex.Message });
            }
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword(ResetPasswordDto resetPasswordDto, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _authService.ResetPasswordAsync(resetPasswordDto, cancellationToken);
                if (!result)
                {
                    return BadRequest(new { message = "Ошибка при сбросе пароля" });
                }

                return Ok(new { message = "Пароль успешно изменен" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при сбросе пароля", error = ex.Message });
            }
        }
    }
}