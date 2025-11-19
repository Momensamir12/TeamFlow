using App.Application.Interfaces;
using App.Domain.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace App.Infrastructure.Authorization;

public class TaskAuthorizer : ITaskAuthorizer
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TaskAuthorizer(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? CurrentUser => _httpContextAccessor.HttpContext?.User;

    public async Task<bool> CanModifyAsync(UserTask task)
    {
        var user = CurrentUser;
        if (user is null) return false;

        var result = await _authorizationService.AuthorizeAsync(user, task, "TaskOwner");
        return result.Succeeded;
    }

    public async Task<bool> CanAccessAsync(UserTask task)
    {
        var user = CurrentUser;
        if (user == null) return false;

        var result = await _authorizationService.AuthorizeAsync(user, task, "TaskAccess");
        return result.Succeeded;
    }

    public async Task EnsureCanModifyAsync(UserTask task)
    {
        if (!await CanModifyAsync(task))
            throw new UnauthorizedAccessException("Only task owner can perform this action");
    }

    public async Task EnsureCanAccessAsync(UserTask task)
    {
        if (!await CanAccessAsync(task))
            throw new UnauthorizedAccessException("You don't have permission to access this task");
    }
}