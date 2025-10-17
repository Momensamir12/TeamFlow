using System.ComponentModel.DataAnnotations;

namespace App.Application.Authentication.DTOS;

public class LoginRequestDTO
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    
};