using App.Application.Common;
using App.Application.DTOs;
using App.Application.Interfaces;
using App.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[Authorize(Policy = "EmailVerified")]
[Route("api/tasks/{taskId}/comments")]
[ApiController]
public class TaskCommentController : ControllerBase
{
    private readonly TaskCommentService _commentService;
    private readonly ICurrentUserService _currentUserService;

    public TaskCommentController(TaskCommentService commentService, ICurrentUserService currentUserService)
    {
        _commentService = commentService;
        _currentUserService = currentUserService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TaskCommentDto>>> CreateComment(Guid taskId, [FromBody] CreateTaskCommentDto dto)
    {
        var userId = _currentUserService.UserId;
        dto.TaskId = taskId;
        var comment = await _commentService.CreateCommentAsync(dto, userId);

        return Created(nameof(CreateComment), ApiResponse<TaskCommentDto>.SuccessResponse(comment, "Comment created successfully"));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<TaskCommentDto>>>> GetTaskComments(Guid taskId)
    {
        var userId = _currentUserService.UserId;
        var comments = await _commentService.GetTaskCommentsAsync(taskId, userId);

        return Ok(ApiResponse<IEnumerable<TaskCommentDto>>.SuccessResponse(comments, "Comments retrieved successfully"));
    }

    [HttpPut("{commentId}")]
    public async Task<ActionResult<ApiResponse<TaskCommentDto>>> UpdateComment(Guid taskId, Guid commentId, [FromBody] UpdateTaskCommentDto dto)
    {
        var userId = _currentUserService.UserId;
        var comment = await _commentService.UpdateCommentAsync(commentId, dto, userId);

        return Ok(ApiResponse<TaskCommentDto>.SuccessResponse(comment, "Comment updated successfully"));
    }

    [HttpDelete("{commentId}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteComment(Guid taskId, Guid commentId)
    {
        var userId = _currentUserService.UserId;
        await _commentService.DeleteCommentAsync(commentId, userId);

        return Ok(ApiResponse<string>.SuccessResponse("", "Comment deleted successfully"));
    }
}
