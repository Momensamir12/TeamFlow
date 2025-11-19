using App.Infrastructure.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace App.Infrastructure.Authorization.Handlers;

public class EmailVerifiedHandler : AuthorizationHandler<EmailVerifiedRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        EmailVerifiedRequirement requirement)
    {
        var isVerifiedClaim = context.User.FindFirst("IsEmailVerified");

        if (isVerifiedClaim != null && bool.TryParse(isVerifiedClaim.Value, out var isVerified) && isVerified)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
