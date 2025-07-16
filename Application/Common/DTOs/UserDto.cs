namespace Application.Common.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public bool IsPrivate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateUserDto
    {
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsPrivate { get; set; }
    }

    public class UpdateUserDto
    {
        public string? UserName { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public bool? IsPrivate { get; set; }
    }
}