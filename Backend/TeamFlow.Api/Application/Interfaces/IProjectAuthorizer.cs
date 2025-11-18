using App.Domain.Model;

namespace App.Application.Interfaces;

public interface IProjectAuthorizer
{
    Task<bool> IsAdminAsync(Project project);
    Task<bool> IsMemberAsync(Project project);
    Task<bool> HasAccessAsync(Project project);
    Task EnsureIsAdminAsync(Project project);
    Task EnsureIsMemberAsync(Project project);
    Task EnsureHasAccessAsync(Project project);
}