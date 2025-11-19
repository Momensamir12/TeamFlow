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
    private readonly UserProfileService _userProfileService;

    public UserController(
        RegisterationService registerationService, 
        TaskService taskService, 
        ICurrentUserService currentUserService,
        UserProfileService userProfileService)
    {
        _registerationService = registerationService;
        _taskService = taskService;
        _currentUserService = currentUserService;
        _userProfileService = userProfileService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<string>>> Register(RegisterRequestDTO request)
    {
        var result = await _registerationService.RegisterUserAsync(request);

        if (!result.Succeeded)
            return BadRequest(ApiResponse<string>.FailureResponse(result.Error ?? "Registration failed"));

        return Ok(ApiResponse<string>.SuccessResponse("", "User registered successfully"));
    }

    [Authorize(Policy = "EmailVerified")]
    [HttpGet("tasks/my")]
    public async Task<ActionResult<ApiResponse<List<UserTaskDto>>>> GetMyTasks()
    {
        var userId = _currentUserService.UserId;
        var tasks = await _taskService.GetAssigneeTasksAsync(userId);

        return Ok(ApiResponse<List<UserTaskDto>>.SuccessResponse(tasks, "Tasks retrieved successfully"));
    }

    [Authorize(Policy = "EmailVerified")]
    [HttpGet("profile")]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetProfile(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var result = await _userProfileService.GetUserProfileAsync(userId, cancellationToken);

        if (!result.Succeeded)
            return NotFound(ApiResponse<UserProfileDto>.FailureResponse(result.Error ?? "Profile not found"));

        return Ok(ApiResponse<UserProfileDto>.SuccessResponse(result.Value!, "Profile retrieved successfully"));
    }

    [Authorize(Policy = "EmailVerified")]
    [HttpPut("profile")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateProfile([FromBody] UpdateUserProfileDto updateDto, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var result = await _userProfileService.UpdateUserProfileAsync(userId, updateDto, cancellationToken);

        if (!result.Succeeded)
            return BadRequest(ApiResponse<string>.FailureResponse(result.Error ?? "Failed to update profile"));

        return Ok(ApiResponse<string>.SuccessResponse("", "Profile updated successfully"));
    }

    [Authorize(Policy = "EmailVerified")]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<string>>> ChangePassword([FromBody] ChangePasswordDto changePasswordDto, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var result = await _userProfileService.ChangePasswordAsync(userId, changePasswordDto, cancellationToken);

        if (!result.Succeeded)
            return BadRequest(ApiResponse<string>.FailureResponse(result.Error ?? "Failed to change password"));

        return Ok(ApiResponse<string>.SuccessResponse("", "Password changed successfully"));
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<string>>> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto, CancellationToken cancellationToken)
    {
        var result = await _userProfileService.ForgotPasswordAsync(forgotPasswordDto, cancellationToken);

        if (!result.Succeeded)
            return BadRequest(ApiResponse<string>.FailureResponse(result.Error ?? "Failed to process request"));

        return Ok(ApiResponse<string>.SuccessResponse("", "If the email exists, a password reset link has been sent"));
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<string>>> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto, CancellationToken cancellationToken)
    {
        var result = await _userProfileService.ResetPasswordAsync(resetPasswordDto, cancellationToken);

        if (!result.Succeeded)
            return BadRequest(ApiResponse<string>.FailureResponse(result.Error ?? "Failed to reset password"));

        return Ok(ApiResponse<string>.SuccessResponse("", "Password reset successfully"));
    }
}