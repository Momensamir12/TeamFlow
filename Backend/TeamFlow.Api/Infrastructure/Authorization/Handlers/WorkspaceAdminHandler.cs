using App.Domain.Model;
using App.Infrastructure.Authorization.Requirements;
using App.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace App.Infrastructure.Authorization.Handlers;

public class WorkspaceAdminHandler : AuthorizationHandler<WorkspaceAdminRequirement, Workspace>
{
    private readonly IWorkspaceRepository _workspaceRepository;

    public WorkspaceAdminHandler(IWorkspaceRepository workspaceRepository)
    {
        _workspaceRepository = workspaceRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        WorkspaceAdminRequirement requirement,
        Workspace workspace)
    {
        var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userId))
        {
            var member = await _workspaceRepository.GetMemberAsync(workspace.Id, userId);
            if (member != null && member.Role == WorkspaceRole.Admin)
            {
                context.Succeed(requirement);
            }
        }
    }
}