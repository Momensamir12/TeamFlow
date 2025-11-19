using App.Infrastructure.Authorization.Requirements;
using App.Domain.Model;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace App.Infrastructure.Authorization.Handlers;

public class TaskAccessHandler : AuthorizationHandler<TaskAccessRequirement, UserTask>
{
    public TaskAccessHandler()
    {
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TaskAccessRequirement requirement,
        UserTask task)
    {
        var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userId))
        {
            if (task.OwnerId == userId || task.AssigneeId == userId)
            {
                context.Succeed(requirement);
            }
        }
        
        return Task.CompletedTask;
    }
}