namespace App.Application.Auth.DTOS
{
    public class RefreshTokenRequestDTO
    {
        public Guid UserId { get; set; }
        public required string RefreshToken { get; set; } = string.Empty;
    };
}