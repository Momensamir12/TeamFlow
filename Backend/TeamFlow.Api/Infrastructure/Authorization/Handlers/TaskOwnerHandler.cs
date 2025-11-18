using App.Domain.Model;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace App.Infrastructure.Authorization.Handlers;

public class TaskOwnerHandler : AuthorizationHandler<TaskOwnerRequirement, UserTask>
{
    public TaskOwnerHandler()
    {
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TaskOwnerRequirement requirement,
        UserTask task)
    {
        var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userId))
        {
            if (task.OwnerId == userId)
            {
                context.Succeed(requirement);
            }
        }
        
        return Task.CompletedTask;
    }
}