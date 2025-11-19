using App.Application.Common;
using App.Application.Dto;
using App.Application.Interfaces;
using App.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

/// <summary>
/// Project management endpoints for creating and managing projects within workspaces.
/// </summary>
[Authorize(Policy = "EmailVerified")]
[Route("api/projects")]
[ApiController]
public class ProjectController : ControllerBase
{
    private readonly ProjectService _projectService;
    private readonly ICurrentUserService _currentUserService;

    public ProjectController(ProjectService projectService, ICurrentUserService currentUserService)
    {
        _projectService = projectService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Create a new project within a workspace.
    /// </summary>
    /// <param name="dto">Project details including name, description, and workspace ID</param>
    /// <returns>The newly created project</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProjectListDto>>> CreateProject(CreateProjectDto dto)
    {
        var userId = _currentUserService.UserId;
        var project = await _projectService.CreateProjectAsync(dto, userId);
        return Ok(ApiResponse<ProjectListDto>.SuccessResponse(project, "Project created successfully"));
    }

    /// <summary>
    /// Update project details such as name and description.
    /// </summary>
    /// <param name="dto">Project ID and updated details</param>
    /// <returns>Success message</returns>
    [HttpPut]
    public async Task<ActionResult<ApiResponse<string>>> UpdateProject(UpdateProjectDto dto)
    {
        var userId = _currentUserService.UserId;
        await _projectService.UpdateProjectAsync(dto, userId);
        return Ok(ApiResponse<string>.SuccessResponse("", "Project updated successfully"));
    }

    /// <summary>
    /// Get detailed information about a specific project including members and task count.
    /// </summary>
    /// <param name="projectId">ID of the project to retrieve</param>
    /// <returns>Detailed project information</returns>
    [HttpGet("{projectId}")]
    public async Task<ActionResult<ApiResponse<ProjectDetailsDto>>> GetProjectDetails(Guid projectId)
    {
        var userId = _currentUserService.UserId;
        var project = await _projectService.GetProjectDetailsAsync(projectId, userId);
        return Ok(ApiResponse<ProjectDetailsDto>.SuccessResponse(project, "Project retrieved successfully"));
    }

    /// <summary>
    /// Get all projects in a specific workspace.
    /// </summary>
    /// <param name="workspaceId">ID of the workspace</param>
    /// <returns>List of projects in the workspace</returns>
    [HttpGet("workspace/{workspaceId}")]
    public async Task<ActionResult<ApiResponse<List<ProjectListDto>>>> GetWorkspaceProjects(Guid workspaceId)
    {
        var userId = _currentUserService.UserId;
        var projects = await _projectService.GetWorkspaceProjectsAsync(workspaceId, userId);
        return Ok(ApiResponse<List<ProjectListDto>>.SuccessResponse(projects, "Projects retrieved successfully"));
    }

    /// <summary>
    /// Add a team member to a project.
    /// </summary>
    /// <param name="dto">Project ID and member user ID</param>
    /// <returns>Success message</returns>
    [HttpPost("members")]
    public async Task<ActionResult<ApiResponse<string>>> AddMemberToProject(AddProjectMemberDto dto)
    {
        var userId = _currentUserService.UserId;
        await _projectService.AddMemberToProjectAsync(dto, userId);
        return Ok(ApiResponse<string>.SuccessResponse("", "Member added to project successfully"));
    }

    /// <summary>
    /// Remove a team member from a project.
    /// </summary>
    /// <param name="projectId">ID of the project</param>
    /// <param name="memberUserId">ID of the member to remove</param>
    /// <returns>Success message</returns>
    [HttpDelete("{projectId}/members/{memberUserId}")]
    public async Task<ActionResult<ApiResponse<string>>> RemoveMemberFromProject(Guid projectId, Guid memberUserId)
    {
        var userId = _currentUserService.UserId;
        await _projectService.RemoveMemberFromProjectAsync(projectId, memberUserId, userId);
        return Ok(ApiResponse<string>.SuccessResponse("", "Member removed from project successfully"));
    }

    /// <summary>
    /// Get the current user's role in a project (Viewer, Member, Admin).
    /// </summary>
    /// <param name="projectId">ID of the project</param>
    /// <returns>User's role level as integer (0=Viewer, 1=Member, 2=Admin)</returns>
    [HttpGet("{projectId}/my-role")]
    public async Task<ActionResult<ApiResponse<int>>> GetMyRole(Guid projectId)
    {
        var userId = _currentUserService.UserId;
        var role = await _projectService.GetUserRoleInProjectAsync(projectId, userId);
        return Ok(ApiResponse<int>.SuccessResponse(role, "User role retrieved successfully"));
    }

    /// <summary>
    /// Get all tasks in a project.
    /// </summary>
    /// <param name="projectId">ID of the project</param>
    /// <returns>List of project tasks</returns>
    [HttpGet("{projectId}/tasks")]
    public async Task<ActionResult<ApiResponse<List<UserTaskDto>>>> GetProjectTasks(Guid projectId)
    {
        var tasks = await _projectService.GetProjectTasksAsync(projectId);
        return Ok(ApiResponse<List<UserTaskDto>>.SuccessResponse(tasks, ""));
    }
}