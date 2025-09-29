namespace App.Infrastructure.Auth.Entities
{
    public class SecurityUser
    {
        public Guid Id { get; set; }
        public String Username { get; set; } = string.Empty;
        public String PasswordHash { get; set; } = string.Empty;
        
    };
}