using App.Application.Common;
using App.Application.Dto;
using App.Application.Interfaces;
using App.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[Authorize]
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

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateProject(CreateProjectDto dto)
    {
        var userId = _currentUserService.UserId;
        await _projectService.CreateProjectAsync(dto, userId);
        return Ok(ApiResponse<string>.SuccessResponse("", "Project created successfully"));
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<string>>> UpdateProject(UpdateProjectDto dto)
    {
        var userId = _currentUserService.UserId;
        await _projectService.UpdateProjectAsync(dto, userId);
        return Ok(ApiResponse<string>.SuccessResponse("", "Project updated successfully"));
    }

    [HttpGet("{projectId}")]
    public async Task<ActionResult<ApiResponse<ProjectDetailsDto>>> GetProjectDetails(Guid projectId)
    {
        var userId = _currentUserService.UserId;
        var project = await _projectService.GetProjectDetailsAsync(projectId, userId);
        return Ok(ApiResponse<ProjectDetailsDto>.SuccessResponse(project, "Project retrieved successfully"));
    }

    [HttpGet("workspace/{workspaceId}")]
    public async Task<ActionResult<ApiResponse<List<ProjectListDto>>>> GetWorkspaceProjects(Guid workspaceId)
    {
        var userId = _currentUserService.UserId;
        var projects = await _projectService.GetWorkspaceProjectsAsync(workspaceId, userId);
        return Ok(ApiResponse<List<ProjectListDto>>.SuccessResponse(projects, "Projects retrieved successfully"));
    }

    [HttpPost("members")]
    public async Task<ActionResult<ApiResponse<string>>> AddMemberToProject(AddProjectMemberDto dto)
    {
        var userId = _currentUserService.UserId;
        await _projectService.AddMemberToProjectAsync(dto, userId);
        return Ok(ApiResponse<string>.SuccessResponse("", "Member added to project successfully"));
    }

    [HttpDelete("{projectId}/members/{memberUserId}")]
    public async Task<ActionResult<ApiResponse<string>>> RemoveMemberFromProject(Guid projectId, Guid memberUserId)
    {
        var userId = _currentUserService.UserId;
        await _projectService.RemoveMemberFromProjectAsync(projectId, memberUserId, userId);
        return Ok(ApiResponse<string>.SuccessResponse("", "Member removed from project successfully"));
    }
}