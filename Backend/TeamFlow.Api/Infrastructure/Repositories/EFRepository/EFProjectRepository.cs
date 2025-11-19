using App.Application.Data;
using App.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace App.Infrastructure.Repositories;

public class EFProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;

    public EFProjectRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Project> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (project == null)
            throw new KeyNotFoundException($"Project with ID {id} not found");

        return project;
    }

    public async Task<List<Project>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Include(p => p.Members)
            .Where(p => p.WorkspaceId == workspaceId && !p.IsArchived)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProjectMember?> GetMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId, cancellationToken);
    }

    public async Task<bool> IsMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId, cancellationToken);
    }

    public async Task<List<Project>> GetAllAsync()
    {
        return await _context.Projects
            .Include(p => p.Members)
            .Where(p => !p.IsArchived)
            .ToListAsync();
    }

    public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        await _context.Projects.AddAsync(project, cancellationToken);
    }

    public async Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        _context.Projects.Update(project);
    }

    public async Task DeleteAsync(Project project, CancellationToken cancellationToken = default)
    {
        _context.Projects.Remove(project);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetWorkspaceProjectCountAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .CountAsync(p => p.WorkspaceId == workspaceId && !p.IsArchived, cancellationToken);
    }

    public async Task<List<Project>> GetWorkspaceProjectsAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Include(p => p.Members)
            .Where(p => p.WorkspaceId == workspaceId && !p.IsArchived)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }
    public async Task<List<ProjectMember>> GetProjectMembersAsync (Guid projectId, CancellationToken cancellationToken = default)
    {
        var members = await _context.ProjectMembers
        .Where(pm => pm.ProjectId == projectId)
        .OrderBy(pm => pm.AddedAt)
        .ToListAsync(cancellationToken);

        return members;
    }
}