using App.Domain.Model;
using App.Infrastructure.Repositories;

public interface IWorkspaceRepository : IRepository<Workspace>
{
    public IQueryable<Workspace> GetByOwnerId(Guid ownerId);
    public  Task<WorkspaceMember?> GetMemberAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
    public  Task<bool> IsMemberAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
    public Task<List<WorkspaceMember>> GetWorkspaceMembersAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    public Task<List<Workspace>> GetUserWorkspacesAsync(Guid userId, CancellationToken cancellationToken = default);
    public Task<Workspace> GetByCodeAsync(string code, CancellationToken cancellationToken = default);




};