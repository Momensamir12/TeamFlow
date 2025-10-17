using App.Application.Interfaces;
using App.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App.Application.Common;

namespace App.API.Controllers;

[Authorize]
[Route("api/tasks")]
[ApiController]
public class TaskController : ControllerBase
{
    private readonly TaskService _taskService;
    private readonly ICurrentUserService _currentUserService;
    
    public TaskController(TaskService taskService, ICurrentUserService currentUserService)
    {
        _taskService = taskService;
        _currentUserService = currentUserService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<string>>> CreateTask(UserTaskDTO userTaskDTO)
    {
        var userId = _currentUserService.UserId;
        await _taskService.CreateTaskAsync(userTaskDTO, userId);
        
        return Ok(ApiResponse<string>.SuccessResponse("", "Task created successfully"));
    }
}