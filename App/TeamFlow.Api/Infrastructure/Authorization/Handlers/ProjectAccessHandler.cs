using App.Domain.Model;
using App.Infrastructure.Authorization.Requirements;
using App.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace App.Infrastructure.Authorization.Handlers;

public class ProjectAccessHandler : AuthorizationHandler<ProjectAccessRequirement, Project>
{
    private readonly IProjectRepository _projectRepository;

    public ProjectAccessHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProjectAccessRequirement requirement,
        Project project)
    {
        var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userId))
        {
            var isMember = await _projectRepository.IsMemberAsync(project.Id, userId);
            if (isMember)
            {
                context.Succeed(requirement);
            }
        }
    }
}