using App.Domain.Model;
using App.Infrastructure.Repositories;

public interface IWorkspaceRepository : IRepository<Workspace>
{
    public IQueryable<Workspace> GetByOwnerId(Guid ownerId);
    public  Task<WorkspaceMember?> GetMemberAsync(Guid workspaceId, Guid userId);
    public  Task<bool> IsMemberAsync(Guid workspaceId, Guid userId);
    public Task<List<WorkspaceMember>> GetWorkspaceMembersAsync(Guid workspaceId);
    public Task<List<Workspace>> GetUserWorkspacesAsync(Guid userId);
    public Task<Workspace> GetByCodeAsync(string code);




};