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

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToDto);
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user != null ? MapToDto(user) : null;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            var existingUser = await _userRepository.GetByEmailAsync(createUserDto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Пользователь с таким email уже существует");
            }

            existingUser = await _userRepository.GetByUsernameAsync(createUserDto.Username);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Пользователь с таким username уже существует");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = createUserDto.Username,
                Email = createUserDto.Email,
                PasswordHash = HashPassword(createUserDto.Password),
                Bio = createUserDto.Bio,
                CreatedAt = DateTime.UtcNow
            };

            var createdUser = await _userRepository.CreateAsync(user);
            return MapToDto(createdUser);
        }

        public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new InvalidOperationException("Пользователь не найден");
            }

            if (!string.IsNullOrEmpty(updateUserDto.Username))
            {
                var existingUser = await _userRepository.GetByUsernameAsync(updateUserDto.Username);
                if (existingUser != null && existingUser.Id != id)
                {
                    throw new InvalidOperationException("Пользователь с таким username уже существует");
                }
                user.Username = updateUserDto.Username;
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

            user.UpdatedAt = DateTime.UtcNow;

            var updatedUser = await _userRepository.UpdateAsync(user);
            return MapToDto(updatedUser);
        }

        public async Task DeleteUserAsync(Guid id)
        {
            await _userRepository.DeleteAsync(id);
        }

        private static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
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