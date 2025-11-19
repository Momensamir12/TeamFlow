using App.Domain.Model;
using App.Infrastructure.Authorization.Requirements;
using App.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace App.Infrastructure.Authorization.Handlers;

public class WorkspaceAccessHandler : AuthorizationHandler<WorkspaceAccessRequirement, Workspace>
{
    private readonly IWorkspaceRepository _workspaceRepository;

    public WorkspaceAccessHandler(IWorkspaceRepository workspaceRepository)
    {
        _workspaceRepository = workspaceRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        WorkspaceAccessRequirement requirement,
        Workspace workspace)
    {
        var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userId))
        {
            var isMember = await _workspaceRepository.IsMemberAsync(workspace.Id, userId);
            if (isMember)
            {
                context.Succeed(requirement);
            }
        }
    }
}