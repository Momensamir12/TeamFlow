using System.Threading.Tasks;
using App.Application.Auth.DTOS;
using App.Infrastructure.Auth.Entities;
using App.Infrastructure.Auth.Service;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<string>> Login(SecurityUserDTO request)
        {
            var token = await _authService.LoginAsync(request);
            if (token is null)
                return BadRequest("Invalid username or password");

            return Ok(token);
        }

    }
}