using App.Application.Interfaces;
using App.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App.Application.Common;
using App.Application.Dto;

namespace App.Api.Controllers;

[Authorize(Policy = "EmailVerified")]
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
    public async Task<ActionResult<ApiResponse<string>>> CreateTask(CreateTaskDto userTaskDTO)
    {
        var userId = _currentUserService.UserId;
        await _taskService.CreateTaskAsync(userTaskDTO, userId);

        return Ok(ApiResponse<string>.SuccessResponse("", "Task created successfully"));
    }

    [HttpGet("assigned")]
    public async Task<ActionResult<ApiResponse<List<UserTaskDto>>>> GetAssignedTasks()
    {
        var userId = _currentUserService.UserId;
        var tasks = await _taskService.GetAssigneeTasksAsync(userId);
        return Ok(ApiResponse<List<UserTaskDto>>.SuccessResponse(tasks, "Tasks retrieved successfully"));
    }

    [HttpPut("status")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateTaskStatus(UpdateTaskStatusDto dto)
    {
        await _taskService.UpdateTaskStatusAsync(dto);
        return Ok(ApiResponse<string>.SuccessResponse("", "Task status updated successfully"));
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<string>>> UpdateTask(UpdateTaskDto dto)
    {
        await _taskService.UpdateTaskAsync(dto);
        return Ok(ApiResponse<string>.SuccessResponse("", "Task updated successfully"));
    }

    [HttpPut("title")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateTaskTitle(UpdateTaskTitleDto dto)
    {
        await _taskService.UpdateTaskTitleAsync(dto);
        return Ok(ApiResponse<string>.SuccessResponse("", "Task title updated successfully"));
    }

    [HttpPut("description")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateTaskDescription(UpdateTaskDescriptionDto dto)
    {
        await _taskService.UpdateTaskDescriptionAsync(dto);
        return Ok(ApiResponse<string>.SuccessResponse("", "Task description updated successfully"));
    }

    [HttpPut("deadline")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateTaskDeadline(UpdateTaskDeadlineDto dto)
    {
        await _taskService.UpdateTaskDeadlineAsync(dto);
        return Ok(ApiResponse<string>.SuccessResponse("", "Task deadline updated successfully"));
    }

    [HttpPut("priority")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateTaskPriority(UpdateTaskPriorityDto dto)
    {
        await _taskService.UpdateTaskPriorityAsync(dto);
        return Ok(ApiResponse<string>.SuccessResponse("", "Task priority updated successfully"));
    }

    [HttpPut("assign")]
    public async Task<ActionResult<ApiResponse<string>>> AssignTask(AssignTaskDto dto)
    {
        await _taskService.AssignTaskAsync(dto);
        return Ok(ApiResponse<string>.SuccessResponse("", "Task assigned successfully"));
    }

    [HttpDelete("{taskId}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteTask(Guid taskId)
    {
        await _taskService.DeleteTaskAsync(taskId);
        return Ok(ApiResponse<string>.SuccessResponse("", "Task deleted successfully"));
    }
}