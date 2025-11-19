using App.Application.Common;
using App.Application.Dto;
using App.Application.Interfaces;
using App.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

/// <summary>
/// Workspace management endpoints for creating, managing, and organizing team workspaces.
/// </summary>
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

    /// <summary>
    /// Create a new workspace for organizing team projects and tasks.
    /// </summary>
    /// <param name="dto">Workspace details including name and description</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The newly created workspace</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<WorkspaceListDto>>> CreateWorkspace(CreateWorkspaceDto dto, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var workspace = await _workspaceService.CreateWorkspaceAsync(dto, userId, cancellationToken);
        return Ok(ApiResponse<WorkspaceListDto>.SuccessResponse(workspace, "Workspace created successfully"));
    }

    /// <summary>
    /// Update workspace details such as name and description.
    /// </summary>
    /// <param name="dto">Workspace ID and updated details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    [HttpPut]
    public async Task<ActionResult<ApiResponse<string>>> UpdateWorkspace(UpdateWorkspaceDto dto, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        await _workspaceService.UpdateWorkspaceAsync(dto, userId, cancellationToken);
        return Ok(ApiResponse<string>.SuccessResponse("", "Workspace updated successfully"));
    }

    /// <summary>
    /// Get all workspaces the current user is a member of.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of user's workspaces with member and project counts</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<WorkspaceListDto>>>> GetUserWorkspaces(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var workspaces = await _workspaceService.GetUserWorkspacesAsync(userId, cancellationToken);
        return Ok(ApiResponse<List<WorkspaceListDto>>.SuccessResponse(workspaces, "Workspaces retrieved successfully"));
    }

    /// <summary>
    /// Get detailed information about a specific workspace including members and projects.
    /// </summary>
    /// <param name="workspaceId">ID of the workspace to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detailed workspace information</returns>
    [HttpGet("{workspaceId}")]
    public async Task<ActionResult<ApiResponse<WorkspaceDetailsDto>>> GetWorkspaceDetails(Guid workspaceId, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var workspace = await _workspaceService.GetWorkspaceDetailsAsync(workspaceId, userId, cancellationToken);
        return Ok(ApiResponse<WorkspaceDetailsDto>.SuccessResponse(workspace, "Workspace details retrieved successfully"));
    }

    /// <summary>
    /// Archive a workspace to hide it from the active workspace list.
    /// </summary>
    /// <param name="workspaceId">ID of the workspace to archive</param>
    /// <returns>Success message</returns>
    [HttpPost("{workspaceId}/archive")]
    public async Task<ActionResult<ApiResponse<string>>> ArchiveWorkspace(Guid workspaceId)
    {
        var userId = _currentUserService.UserId;
        await _workspaceService.ArchiveWorkspaceAsync(workspaceId, userId);
        return Ok(ApiResponse<string>.SuccessResponse("", "Workspace archived successfully"));
    }

    /// <summary>
    /// Unarchive a workspace to restore it to the active workspace list.
    /// </summary>
    /// <param name="workspaceId">ID of the workspace to unarchive</param>
    /// <returns>Success message</returns>
    [HttpPost("{workspaceId}/unarchive")]
    public async Task<ActionResult<ApiResponse<string>>> UnarchiveWorkspace(Guid workspaceId)
    {
        var userId = _currentUserService.UserId;
        await _workspaceService.UnarchiveWorkspaceAsync(workspaceId, userId);
        return Ok(ApiResponse<string>.SuccessResponse("", "Workspace unarchived successfully"));
    }

    /// <summary>
    /// Join an existing workspace using an invitation code.
    /// </summary>
    /// <param name="code">Workspace invitation code</param>
    /// <returns>Success message</returns>
    [HttpPost("join")]
    public async Task<ActionResult<ApiResponse<string>>> JoinByCode([FromQuery] string code)
    {
        var userId = _currentUserService.UserId;
        await _workspaceService.JoinWorkspaceByCode(userId, code);
        return Ok(ApiResponse<string>.SuccessResponse("", "Workspace joined successfully"));
    }

    /// <summary>
    /// Get the invitation code for a workspace to share with other users.
    /// </summary>
    /// <param name="workspaceId">ID of the workspace</param>
    /// <returns>The workspace invitation code</returns>
    [HttpGet("{workspaceId}/code")]
    public async Task<ActionResult<ApiResponse<string>>> GetWorkspaceCode(Guid workspaceId)
    {
        var userId = _currentUserService.UserId;
        var code = await _workspaceService.GetWorkspaceCodeAsync(workspaceId, userId);
        return Ok(ApiResponse<string>.SuccessResponse(code, "Workspace code retrieved successfully"));
    }

    /// <summary>
    /// Regenerate the invitation code for a workspace, invalidating the previous code.
    /// </summary>
    /// <param name="workspaceId">ID of the workspace</param>
    /// <returns>Success message</returns>
    [HttpPost("{workspaceId}/code/regenerate")]
    public async Task<ActionResult<ApiResponse<string>>> RegenerateWorkspaceCode(Guid workspaceId)
    {
        var userId = _currentUserService.UserId;
        await _workspaceService.RegenerateWorkspaceCodeAsync(workspaceId, userId);
        return Ok(ApiResponse<string>.SuccessResponse("", "Workspace code regenerated successfully"));
    }

    /// <summary>
    /// Remove a member from a workspace.
    /// </summary>
    /// <param name="workspaceId">ID of the workspace</param>
    /// <param name="memberUserId">ID of the member to remove</param>
    /// <returns>Success message</returns>
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