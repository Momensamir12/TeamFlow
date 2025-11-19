using App.Application.Interfaces;
using App.Domain.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace App.Infrastructure.Authorization;

public class ProjectAuthorizer : IProjectAuthorizer
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProjectAuthorizer(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? CurrentUser => _httpContextAccessor.HttpContext?.User;

    public async Task<bool> IsAdminAsync(Project project)
    {
        var user = CurrentUser;
        if (user == null) return false;

        var result = await _authorizationService.AuthorizeAsync(user, project, "ProjectAdmin");
        return result.Succeeded;
    }

    public async Task<bool> IsMemberAsync(Project project)
    {
        var user = CurrentUser;
        if (user == null) return false;

        var result = await _authorizationService.AuthorizeAsync(user, project, "ProjectMember");
        return result.Succeeded;
    }

    public async Task<bool> HasAccessAsync(Project project)
    {
        var user = CurrentUser;
        if (user == null) return false;

        var result = await _authorizationService.AuthorizeAsync(user, project, "ProjectAccess");
        return result.Succeeded;
    }

    public async Task EnsureIsAdminAsync(Project project)
    {
        if (!await IsAdminAsync(project))
            throw new UnauthorizedAccessException("Only project admins can perform this action");
    }

    public async Task EnsureIsMemberAsync(Project project)
    {
        if (!await IsMemberAsync(project))
            throw new UnauthorizedAccessException("Only project members can perform this action");
    }

    public async Task EnsureHasAccessAsync(Project project)
    {
        if (!await HasAccessAsync(project))
            throw new UnauthorizedAccessException("You don't have access to this project");
    }
}