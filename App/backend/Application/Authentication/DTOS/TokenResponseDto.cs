namespace App.Application.Authentication.DTOS;

public class TokenResponseDTO
{
    public required string AccessToken { get; set; } = string.Empty;
    public required string RefreshToken { get; set; } = string.Empty;
};
