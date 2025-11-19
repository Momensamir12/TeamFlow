using Microsoft.AspNetCore.Mvc;
using App.Infrastructure.Authentication.Service;
using App.Application.Authentication.DTOS;
using App.Application.Common;

namespace App.API.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AuthenticationService _authService;
    
    public AuthController(AuthenticationService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<TokenResponseDTO>>> Login(LoginRequestDTO request)
    {
        var token = await _authService.LoginAsync(request);
        
        if (token is null)
            return BadRequest(ApiResponse<TokenResponseDTO>.FailureResponse("Invalid username or password"));

        return Ok(ApiResponse<TokenResponseDTO>.SuccessResponse(token, "Login successful"));
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<TokenResponseDTO>>> RefreshToken(RefreshTokenRequestDTO request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        
        if (result is null || result.RefreshToken is null)
            return Unauthorized(ApiResponse<TokenResponseDTO>.FailureResponse("Invalid refresh token"));

        return Ok(ApiResponse<TokenResponseDTO>.SuccessResponse(result, "Token refreshed successfully"));
    }

    [HttpPost("verify-email")]
    public async Task<ActionResult<ApiResponse<string>>> VerifyEmail([FromBody] VerifyEmailRequestDTO request)
    {
        var result = await _authService.VerifyEmailAsync(request.Token);
        
        if (!result.Succeeded)
            return BadRequest(ApiResponse<string>.FailureResponse(result.Error ?? "Email verification failed"));

        return Ok(ApiResponse<string>.SuccessResponse("", "Email verified successfully"));
    }

    [HttpPost("resend-verification")]
    public async Task<ActionResult<ApiResponse<string>>> ResendVerification([FromBody] ResendVerificationRequestDTO request)
    {
        var result = await _authService.ResendVerificationEmailAsync(request.Email);
        
        if (!result.Succeeded)
            return BadRequest(ApiResponse<string>.FailureResponse(result.Error ?? "Failed to resend verification email"));

        return Ok(ApiResponse<string>.SuccessResponse("", "Verification email sent successfully"));
    }
}