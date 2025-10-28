using App.Application.Data;
using App.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class EFWorkspaceRepository : IWorkspaceRepository
{
    private readonly AppDbContext _appDbContext;

    public EFWorkspaceRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<Workspace> GetByIdAsync(Guid id)
    {
        var workspace = await _appDbContext.Workspaces.FindAsync(id);
        if (workspace is null)
            throw new KeyNotFoundException($"Workspace with id '{id}' not found.");
            
        return workspace;    
    }

    public async Task AddAsync(Workspace workspace)
    {
        await _appDbContext.Workspaces.AddAsync(workspace);
    }

    public Task UpdateAsync(Workspace workspace)
    {
        _appDbContext.Workspaces.Update(workspace);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Workspace workspace)
    {
        _appDbContext.Workspaces.Remove(workspace);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _appDbContext.SaveChangesAsync();
    }

    public async Task<List<Workspace>> GetUserWorkspacesAsync(Guid userId)
    {
        // Join WorkspaceMembers with Workspaces
        return await _appDbContext.WorkspaceMembers
            .Where(wm => wm.UserId == userId && wm.RemovedAt == null)
            .Join(
                _appDbContext.Workspaces,
                wm => wm.WorkspaceId,
                w => w.Id,
                (wm, w) => w
            )
            .ToListAsync();
    }

    public IQueryable<Workspace> GetByOwnerId(Guid ownerId)
    {
        return _appDbContext.Workspaces.Where(w => w.OwnerId == ownerId);
    }

    public async Task<WorkspaceMember?> GetMemberAsync(Guid workspaceId, Guid userId)
    {
        return await _appDbContext.WorkspaceMembers
            .FirstOrDefaultAsync(wm => 
                wm.WorkspaceId == workspaceId && 
                wm.UserId == userId && 
                wm.RemovedAt == null);
    }

    public async Task<bool> IsMemberAsync(Guid workspaceId, Guid userId)
    {
        return await _appDbContext.WorkspaceMembers
            .AnyAsync(wm => 
                wm.WorkspaceId == workspaceId && 
                wm.UserId == userId && 
                wm.RemovedAt == null);
    }

    public async Task<List<WorkspaceMember>> GetWorkspaceMembersAsync(Guid workspaceId)
    {
        return await _appDbContext.WorkspaceMembers
            .Where(wm => wm.WorkspaceId == workspaceId && wm.RemovedAt == null)
            .OrderByDescending(wm => wm.Role)
            .ThenBy(wm => wm.JoinedAt)
            .ToListAsync();
    }
}