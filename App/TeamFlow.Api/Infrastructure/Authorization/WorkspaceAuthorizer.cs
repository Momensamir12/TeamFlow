using App.Domain.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace App.Infrastructure.Authorization;

public class WorkspaceAuthorizer : IWorkspaceAuthorizer
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WorkspaceAuthorizer(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? CurrentUser => _httpContextAccessor.HttpContext?.User;

    public async Task<bool> IsAdminAsync(Workspace workspace)
    {
        var user = CurrentUser;
        if (user == null) return false;

        var result = await _authorizationService.AuthorizeAsync(user, workspace, "WorkspaceAdmin");
        return result.Succeeded;
    }

    public async Task<bool> IsMemberAsync(Workspace workspace)
    {
        var user = CurrentUser;
        if (user == null) return false;

        var result = await _authorizationService.AuthorizeAsync(user, workspace, "WorkspaceMember");
        return result.Succeeded;
    }

    public async Task<bool> HasAccessAsync(Workspace workspace)
    {
        var user = CurrentUser;
        if (user == null) return false;

        var result = await _authorizationService.AuthorizeAsync(user, workspace, "WorkspaceAccess");
        return result.Succeeded;
    }

    public async Task EnsureIsAdminAsync(Workspace workspace)
    {
        if (!await IsAdminAsync(workspace))
            throw new UnauthorizedAccessException("Only workspace admins can perform this action");
    }

    public async Task EnsureIsMemberAsync(Workspace workspace)
    {
        if (!await IsMemberAsync(workspace))
            throw new UnauthorizedAccessException("Only workspace members can perform this action");
    }

    public async Task EnsureHasAccessAsync(Workspace workspace)
    {
        if (!await HasAccessAsync(workspace))
            throw new UnauthorizedAccessException("You don't have access to this workspace");
    }
}