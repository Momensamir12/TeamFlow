using App.Application.Common;
using App.Application.Dto;
using App.Application.Interfaces;
using App.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[Authorize(Policy = "EmailVerified")]
[Route("api/workspaces")]
[ApiController]
public class WorkspaceController : ControllerBase
{
    private readonly WorkspaceService _workspaceService;
    private readonly ICurrentUserService _currentUserService;

    public WorkspaceController(WorkspaceService workspaceService, ICurrentUserService currentUserService)
    {
        _workspaceService = workspaceService;
        _currentUserService = currentUserService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<string>>> CreateWorkspace(CreateWorkspaceDto dto)
    {
        var userId = _currentUserService.UserId;
        await _workspaceService.CreateWorkspaceAsync(dto, userId);
        return Ok(ApiResponse<string>.SuccessResponse("", "Workspace created successfully"));
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<string>>> UpdateWorkspace(UpdateWorkspaceDto dto)
    {
        var userId = _currentUserService.UserId;
        await _workspaceService.UpdateWorkspaceAsync(dto, userId);
        return Ok(ApiResponse<string>.SuccessResponse("", "Workspace updated successfully"));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<WorkspaceListDto>>>> GetUserWorkspaces()
    {
        var userId = _currentUserService.UserId;
        var workspaces = await _workspaceService.GetUserWorkspacesAsync(userId);
        return Ok(ApiResponse<List<WorkspaceListDto>>.SuccessResponse(workspaces, "Workspaces retrieved successfully"));
    }

    [HttpGet("{workspaceId}")]
    public async Task<ActionResult<ApiResponse<WorkspaceDetailsDto>>> GetWorkspaceDetails(Guid workspaceId)
    {
        var userId = _currentUserService.UserId;
        var workspace = await _workspaceService.GetWorkspaceDetailsAsync(workspaceId, userId);
        return Ok(ApiResponse<WorkspaceDetailsDto>.SuccessResponse(workspace, "Workspace details retrieved successfully"));
    }

    [HttpPost("{workspaceId}/archive")]
    public async Task<ActionResult<ApiResponse<string>>> ArchiveWorkspace(Guid workspaceId)
    {
        var userId = _currentUserService.UserId;
        await _workspaceService.ArchiveWorkspaceAsync(workspaceId, userId);
        return Ok(ApiResponse<string>.SuccessResponse("", "Workspace archived successfully"));
    }

    [HttpPost("{workspaceId}/unarchive")]
    public async Task<ActionResult<ApiResponse<string>>> UnarchiveWorkspace(Guid workspaceId)
    {
        var userId = _currentUserService.UserId;
        await _workspaceService.UnarchiveWorkspaceAsync(workspaceId, userId);
        return Ok(ApiResponse<string>.SuccessResponse("", "Workspace unarchived successfully"));
    }

    [HttpPost("join")]
    public async Task<ActionResult<ApiResponse<string>>> JoinByCode([FromQuery] string code)
    {
        var userId = _currentUserService.UserId;
        await _workspaceService.JoinWorkspaceByCode(userId, code);
        return Ok(ApiResponse<string>.SuccessResponse("", "Workspace joined successfully"));
    }

    [HttpGet("{workspaceId}/code")]
    public async Task<ActionResult<ApiResponse<string>>> GetWorkspaceCode(Guid workspaceId)
    {
        var userId = _currentUserService.UserId;
        var code = await _workspaceService.GetWorkspaceCodeAsync(workspaceId, userId);
        return Ok(ApiResponse<string>.SuccessResponse(code, "Workspace code retrieved successfully"));
    }

    [HttpPost("{workspaceId}/code/regenerate")]
    public async Task<ActionResult<ApiResponse<string>>> RegenerateWorkspaceCode(Guid workspaceId)
    {
        var userId = _currentUserService.UserId;
        await _workspaceService.RegenerateWorkspaceCodeAsync(workspaceId, userId);
        return Ok(ApiResponse<string>.SuccessResponse("", "Workspace code regenerated successfully"));
    }

    [HttpDelete("{workspaceId}/members/{memberUserId}")]
    public async Task<ActionResult<ApiResponse<string>>> RemoveMember(Guid workspaceId, Guid memberUserId)
    {
        var userId = _currentUserService.UserId;
        await _workspaceService.RemoveMemberAsync(workspaceId, memberUserId, userId);
        return Ok(ApiResponse<string>.SuccessResponse("", "Member removed successfully"));
    }

    [HttpPut("{workspaceId}/members/{memberUserId}/role")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateMemberRole(Guid workspaceId, Guid memberUserId, [FromBody] UpdateMemberRoleDto dto)
    {
        var userId = _currentUserService.UserId;
        await _workspaceService.UpdateMemberRoleAsync(workspaceId, memberUserId, dto.NewRole, userId);
        return Ok(ApiResponse<string>.SuccessResponse("", "Member role updated successfully"));
    }

    [HttpGet("{workspaceId}/my-role")]
    public async Task<ActionResult<ApiResponse<int>>> GetMyRole(Guid workspaceId)
    {
        var userId = _currentUserService.UserId;
        var role = await _workspaceService.GetUserRoleInWorkspaceAsync(workspaceId, userId);
        return Ok(ApiResponse<int>.SuccessResponse(role, "User role retrieved successfully"));
    }
}