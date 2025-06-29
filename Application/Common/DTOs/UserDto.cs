namespace Application.Common.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public bool IsPrivate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Bio { get; set; }
    }

    public class UpdateUserDto
    {
        public string? Username { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public bool? IsPrivate { get; set; }
    }
}