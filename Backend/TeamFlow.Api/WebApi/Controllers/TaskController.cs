using App.Application.Interfaces;
using App.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App.Application.Common;
using App.Application.Dto;

namespace App.Api.Controllers;

/// <summary>
/// Task management endpoints for creating, retrieving, and updating tasks.
/// </summary>
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

    /// <summary>
    /// Create a new task (personal or project-based).
    /// </summary>
    /// <param name="userTaskDTO">Task details including title, description, priority, deadline, and optional assignee</param>
    /// <returns>The newly created task</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserTaskDto>>> CreateTask(CreateTaskDto userTaskDTO)
    {
        var userId = _currentUserService.UserId;
        var createdTask = await _taskService.CreateTaskAsync(userTaskDTO, userId);

        return Ok(ApiResponse<UserTaskDto>.SuccessResponse(createdTask, "Task created successfully"));
    }

    /// <summary>
    /// Get all tasks assigned to the current user.
    /// Includes personal tasks owned by the user and project tasks assigned to them.
    /// </summary>
    /// <returns>List of tasks assigned to or owned by the user</returns>
    [HttpGet("assigned")]
    public async Task<ActionResult<ApiResponse<List<UserTaskDto>>>> GetAssignedTasks()
    {
        var userId = _currentUserService.UserId;
        var tasks = await _taskService.GetAssigneeTasksAsync(userId);
        return Ok(ApiResponse<List<UserTaskDto>>.SuccessResponse(tasks, "Tasks retrieved successfully"));
    }

    /// <summary>
    /// Update the status of a task (To Do, In Progress, Done, Blocked).
    /// </summary>
    /// <param name="dto">Task ID and new status</param>
    /// <returns>Success message</returns>
    [HttpPut("status")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateTaskStatus(UpdateTaskStatusDto dto)
    {
        await _taskService.UpdateTaskStatusAsync(dto);
        return Ok(ApiResponse<string>.SuccessResponse("", "Task status updated successfully"));
    }

    /// <summary>
    /// Update multiple properties of a task at once.
    /// </summary>
    /// <param name="dto">Task ID and properties to update</param>
    /// <returns>Success message</returns>
    [HttpPut]
    public async Task<ActionResult<ApiResponse<string>>> UpdateTask(UpdateTaskDto dto)
    {
        await _taskService.UpdateTaskAsync(dto);
        return Ok(ApiResponse<string>.SuccessResponse("", "Task updated successfully"));
    }

    /// <summary>
    /// Update the title of a task.
    /// </summary>
    /// <param name="dto">Task ID and new title</param>
    /// <returns>Success message</returns>
    [HttpPut("title")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateTaskTitle(UpdateTaskTitleDto dto)
    {
        await _taskService.UpdateTaskTitleAsync(dto);
        return Ok(ApiResponse<string>.SuccessResponse("", "Task title updated successfully"));
    }

    /// <summary>
    /// Update the description of a task.
    /// </summary>
    /// <param name="dto">Task ID and new description</param>
    /// <returns>Success message</returns>
    [HttpPut("description")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateTaskDescription(UpdateTaskDescriptionDto dto)
    {
        await _taskService.UpdateTaskDescriptionAsync(dto);
        return Ok(ApiResponse<string>.SuccessResponse("", "Task description updated successfully"));
    }

    /// <summary>
    /// Update the deadline of a task.
    /// </summary>
    /// <param name="dto">Task ID and new deadline date</param>
    /// <returns>Success message</returns>
    [HttpPut("deadline")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateTaskDeadline(UpdateTaskDeadlineDto dto)
    {
        await _taskService.UpdateTaskDeadlineAsync(dto);
        return Ok(ApiResponse<string>.SuccessResponse("", "Task deadline updated successfully"));
    }

    /// <summary>
    /// Update the priority level of a task (Low, Medium, High, Urgent).
    /// </summary>
    /// <param name="dto">Task ID and new priority level</param>
    /// <returns>Success message</returns>
    [HttpPut("priority")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateTaskPriority(UpdateTaskPriorityDto dto)
    {
        await _taskService.UpdateTaskPriorityAsync(dto);
        return Ok(ApiResponse<string>.SuccessResponse("", "Task priority updated successfully"));
    }

    [HttpPut("assign")]
    public async Task<ActionResult<ApiResponse<string>>> AssignTask(AssignTaskDto dto)
    {
        var userId = _currentUserService.UserId;
        await _taskService.AssignTaskAsync(dto, userId);
        return Ok(ApiResponse<string>.SuccessResponse("", "Task assigned successfully"));
    }

    [HttpDelete("{taskId}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteTask(Guid taskId)
    {
        await _taskService.DeleteTaskAsync(taskId);
        return Ok(ApiResponse<string>.SuccessResponse("", "Task deleted successfully"));
    }
}