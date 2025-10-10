using App.Infrastructure.Auth.Entities;
using Microsoft.AspNetCore.Mvc;
using App.Infrastructure.Authentication.Service;
using App.Application.Authentication.DTOS;
using App.Common;
using App.Application.Users.Service;

namespace App.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly RegisterationService _registerationService;
    public UserController(RegisterationService registerationService)
    {
        _registerationService = registerationService;
    }
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDTO request)
    {
        var result = await _registerationService.RegisterUserAsync(request);

        if (!result.Succeeded)
            return BadRequest(new { message = result.Error });

        return Ok(new { message = "User registered successfully" });
    }
}
