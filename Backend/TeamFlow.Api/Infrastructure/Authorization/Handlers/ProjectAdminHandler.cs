using App.Domain.Model;
using App.Infrastructure.Authorization.Requirements;
using App.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace App.Infrastructure.Authorization.Handlers;

public class ProjectAdminHandler : AuthorizationHandler<ProjectAdminRequirement, Project>
{
    private readonly IProjectRepository _projectRepository;

    public ProjectAdminHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProjectAdminRequirement requirement,
        Project project)
    {
        var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userId))
        {
            var member = await _projectRepository.GetMemberAsync(project.Id, userId);
            if (member != null && member.Role == ProjectRole.Admin)
            {
                context.Succeed(requirement);
            }
        }
    }
}