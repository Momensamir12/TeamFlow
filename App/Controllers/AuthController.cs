using System.Threading.Tasks;
using App.Application.Auth.DTOS;
using App.Infrastructure.Auth.Entities;
using App.Infrastructure.Auth.Service;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Authorization;

namespace App.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("register")]
        public async Task<ActionResult<SecurityUser>> Register(SecurityUserDTO request)
        {
            var user = await _authService.RegisterUserAsync(request);
            if (user is null)
                return BadRequest("Username already exists.");
            return Ok(user);
        }
        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDTO>> Login(SecurityUserDTO request)
        {
            var token = await _authService.LoginAsync(request);
            if (token is null)
                return BadRequest("Invalid username or password");

            return Ok(token);
        }
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public ActionResult<string> AdminEndpoint()
        {
            return Ok("You are an admin !");
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
}