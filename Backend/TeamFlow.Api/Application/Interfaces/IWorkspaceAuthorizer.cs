using App.Domain.Model;

namespace App.Infrastructure.Authorization;

public interface IWorkspaceAuthorizer
{
    Task<bool> IsAdminAsync(Workspace workspace);
    Task<bool> IsMemberAsync(Workspace workspace);
    Task<bool> HasAccessAsync(Workspace workspace);
    Task EnsureIsAdminAsync(Workspace workspace);
    Task EnsureIsMemberAsync(Workspace workspace);
    Task EnsureHasAccessAsync(Workspace workspace);
}