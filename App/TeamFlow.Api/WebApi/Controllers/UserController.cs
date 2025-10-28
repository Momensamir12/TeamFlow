using Microsoft.AspNetCore.Mvc;
using App.Application.Authentication.DTOS;
using App.Application.Service;
using App.Application.Services;
using Microsoft.AspNetCore.Authorization;
using App.Application.Interfaces;
using App.Application.Common;
using App.Application.Dto;

namespace App.API.Controllers;

[Route("api/users")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly RegisterationService _registerationService;
    private readonly TaskService _taskService;
    private readonly ICurrentUserService _currentUserService;

    public UserController(RegisterationService registerationService, TaskService taskService, ICurrentUserService currentUserService)
    {
        _registerationService = registerationService;
        _taskService = taskService;
        _currentUserService = currentUserService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<string>>> Register(RegisterRequestDTO request)
    {
        var result = await _registerationService.RegisterUserAsync(request);

        if (!result.Succeeded)
            return BadRequest(ApiResponse<string>.FailureResponse(result.Error ?? "Registration failed"));

        return Ok(ApiResponse<string>.SuccessResponse("", "User registered successfully"));
    }

    [Authorize]
    [HttpGet("tasks/my")]
    public async Task<ActionResult<ApiResponse<List<UserTaskDto>>>> GetMyTasks()
    {
        var userId = _currentUserService.UserId;
        var tasks = await _taskService.GetAssigneeTasksAsync(userId);

        return Ok(ApiResponse<List<UserTaskDto>>.SuccessResponse(tasks, "Tasks retrieved successfully"));
    }
}