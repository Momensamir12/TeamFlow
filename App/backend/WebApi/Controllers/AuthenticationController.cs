using App.Infrastructure.Auth.Entities;
using Microsoft.AspNetCore.Mvc;
using App.Infrastructure.Authentication.Service;
using App.Application.Authentication.DTOS;
using App.Common;

namespace App.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AuthenticationService _authService;
    public AuthController(AuthenticationService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponseDTO>> Login(LoginRequestDTO request)
    {
        var token = await _authService.LoginAsync(request);
        if (token is null)
            return BadRequest("Invalid username or password");

        return Ok(token);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<TokenResponseDTO>> RefreshToken(RefreshTokenRequestDTO request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        if (result is null || result.RefreshToken is null)
            return Unauthorized("Invalid refresh token");

        return Ok(result);
    }
}
