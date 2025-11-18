namespace App.Application.Authentication.DTOS;

public class TokenResponseDTO
{
    public required string AccessToken { get; set; } = string.Empty;
    public required string RefreshToken { get; set; } = string.Empty;
    public required Guid UserId { get; set; }
    public required string UserName { get; set; } = string.Empty;
    public required string Email { get; set; } = string.Empty;
    public required bool IsEmailVerified { get; set; }
};
