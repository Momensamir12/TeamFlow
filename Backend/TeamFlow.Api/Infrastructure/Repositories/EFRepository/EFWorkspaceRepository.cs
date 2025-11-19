using App.Application.Data;
using App.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Prng;

namespace App.Infrastructure.Repositories;

public class EFWorkspaceRepository : IWorkspaceRepository
{
    private readonly AppDbContext _appDbContext;

    public EFWorkspaceRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<Workspace> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workspace = await _appDbContext.Workspaces
            .Include(w => w.Members.Where(m => m.RemovedAt == null))  // Change this line
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);                     // Change this line
            
        if (workspace is null)
            throw new KeyNotFoundException($"Workspace with id '{id}' not found.");
            
        return workspace;    
    }

    public async Task AddAsync(Workspace workspace, CancellationToken cancellationToken = default)
    {
        await _appDbContext.Workspaces.AddAsync(workspace, cancellationToken);
    }

    public Task UpdateAsync(Workspace workspace, CancellationToken cancellationToken = default)
    {
        _appDbContext.Workspaces.Update(workspace);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Workspace workspace, CancellationToken cancellationToken = default)
    {
        _appDbContext.Workspaces.Remove(workspace);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _appDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Workspace>> GetUserWorkspacesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _appDbContext.WorkspaceMembers
            .Where(wm => wm.UserId == userId && wm.RemovedAt == null)
            .Join(
                _appDbContext.Workspaces,
                wm => wm.WorkspaceId,
                w => w.Id,
                (wm, w) => w
            )
            .Include(w => w.Members.Where(m => m.RemovedAt == null))
            .Include(w => w.Projects.Where(p => !p.IsArchived))
            .ToListAsync(cancellationToken);
    }

    public IQueryable<Workspace> GetByOwnerId(Guid ownerId)
    {
        return _appDbContext.Workspaces.Where(w => w.OwnerId == ownerId);
    }

    public async Task<WorkspaceMember?> GetMemberAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _appDbContext.WorkspaceMembers
            .FirstOrDefaultAsync(wm => 
                wm.WorkspaceId == workspaceId && 
                wm.UserId == userId && 
                wm.RemovedAt == null, cancellationToken);
    }

    public async Task<bool> IsMemberAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _appDbContext.WorkspaceMembers
            .AnyAsync(wm => 
                wm.WorkspaceId == workspaceId && 
                wm.UserId == userId && 
                wm.RemovedAt == null, cancellationToken);
    }

    public async Task<List<WorkspaceMember>> GetWorkspaceMembersAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await _appDbContext.WorkspaceMembers
            .Where(wm => wm.WorkspaceId == workspaceId && wm.RemovedAt == null)
            .OrderByDescending(wm => wm.Role)
            .ThenBy(wm => wm.JoinedAt)
            .ToListAsync(cancellationToken);
    }
    public async Task<Workspace> GetByCodeAsync (string code, CancellationToken cancellationToken = default)
    {
        var workspace = await _appDbContext.Workspaces
        .Include(w => w.Members.Where(m => m.RemovedAt == null))
        .FirstOrDefaultAsync(w => w.Code == code, cancellationToken);
        
        if (workspace is null)
            throw new KeyNotFoundException("Workspace code is invalid");

        return workspace;    
    }
}