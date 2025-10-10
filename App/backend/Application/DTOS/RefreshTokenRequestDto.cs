namespace App.Application.Authentication.DTOS;

public class RefreshTokenRequestDTO
{
    public Guid UserId { get; set; }
    public required string RefreshToken { get; set; } = string.Empty;
};
