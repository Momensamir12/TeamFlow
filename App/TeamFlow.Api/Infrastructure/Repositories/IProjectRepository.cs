using App.Domain.Model;

namespace App.Infrastructure.Repositories;

public interface IProjectRepository : IRepository<Project>
{
    Task<List<Project>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<ProjectMember?> GetMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetWorkspaceProjectCountAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<List<Project>> GetWorkspaceProjectsAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<List<ProjectMember>> GetProjectMembersAsync(Guid id, CancellationToken cancellationToken = default);


}