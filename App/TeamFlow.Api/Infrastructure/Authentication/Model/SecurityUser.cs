namespace App.Infrastructure.Auth.Entities
{
    public class SecurityUser
    {
        public Guid Id { get; set; }
        public String Username { get; set; } = string.Empty;
        public String PasswordHash { get; set; } = string.Empty;
        public String Email { get; set; } = string.Empty;
        public List<string>? Roles { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    };
}