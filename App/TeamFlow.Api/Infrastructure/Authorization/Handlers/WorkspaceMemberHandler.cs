using System.Security.Claims;
using App.Domain.Model;
using App.Infrastructure.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

public class WorkspaceMemberHandler : AuthorizationHandler<WorkspaceMemberRequirement, Workspace>
{
    private readonly IWorkspaceRepository _workspaceRepository;

    public WorkspaceMemberHandler(IWorkspaceRepository workspaceRepository)
    {
        _workspaceRepository = workspaceRepository;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, WorkspaceMemberRequirement requirement, Workspace workspace)
    {
        var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userId))
        {
            var isMember = await _workspaceRepository.IsMemberAsync(workspace.Id, userId);
            if (isMember)
                context.Succeed(requirement);
        }
    }
};