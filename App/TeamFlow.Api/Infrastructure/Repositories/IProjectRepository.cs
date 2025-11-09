using App.Domain.Model;

namespace App.Infrastructure.Repositories;

public interface IProjectRepository : IRepository<Project>
{
    Task<List<Project>> GetByWorkspaceIdAsync(Guid workspaceId);
    Task<ProjectMember?> GetMemberAsync(Guid projectId, Guid userId);
    Task<bool> IsMemberAsync(Guid projectId, Guid userId);
    Task<int> GetWorkspaceProjectCountAsync(Guid workspaceId);
    Task<List<Project>> GetWorkspaceProjectsAsync(Guid workspaceId);
    Task<List<ProjectMember>> GetProjectMembersAsync(Guid id);


}