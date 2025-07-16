using Application.Common.DTOs;
using Domain.Interfaces;
using Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken)
        {
            var users = await _userRepository.GetAllAsync(cancellationToken);
            return users.Select(MapToDto);
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            return user != null ? MapToDto(user) : null;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto, CancellationToken cancellationToken)
        {
            var existingUser = await _userRepository.GetByEmailAsync(createUserDto.Email, cancellationToken);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Пользователь с таким email уже существует");
            }

            existingUser = await _userRepository.GetByUsernameAsync(createUserDto.UserName, cancellationToken);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Пользователь с таким username уже существует");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = createUserDto.UserName,
                Email = createUserDto.Email,
                PasswordHash = HashPassword(createUserDto.Password),
                Bio = createUserDto.Bio,
                IsPrivate = createUserDto.IsPrivate,
                CreatedAt = DateTime.UtcNow
            };

            var createdUser = await _userRepository.CreateAsync(user, cancellationToken);
            return MapToDto(createdUser);
        }

        public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                throw new InvalidOperationException("Пользователь не найден");
            }

            if (!string.IsNullOrEmpty(updateUserDto.UserName))
            {
                var existingUser = await _userRepository.GetByUsernameAsync(updateUserDto.UserName, cancellationToken);
                if (existingUser != null && existingUser.Id != id)
                {
                    throw new InvalidOperationException("Пользователь с таким username уже существует");
                }
                user.UserName = updateUserDto.UserName;
            }

            if (updateUserDto.Bio != null)
            {
                user.Bio = updateUserDto.Bio;
            }

            if (updateUserDto.AvatarUrl != null)
            {
                user.AvatarUrl = updateUserDto.AvatarUrl;
            }

            if (updateUserDto.IsPrivate.HasValue)
            {
                user.IsPrivate = updateUserDto.IsPrivate.Value;
            }

            var updatedUser = await _userRepository.UpdateAsync(user, cancellationToken);
            return MapToDto(updatedUser);
        }

        public async Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            if (user == null)
                return false;

            await _userRepository.DeleteAsync(id, cancellationToken);
            return true;
        }

        private static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl,
                Bio = user.Bio,
                IsPrivate = user.IsPrivate,
                CreatedAt = user.CreatedAt
            };
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}