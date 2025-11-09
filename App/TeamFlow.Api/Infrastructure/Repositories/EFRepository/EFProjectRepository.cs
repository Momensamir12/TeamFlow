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

    public async Task<Project> GetByIdAsync(Guid id)
    {
        var project = await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
            throw new KeyNotFoundException($"Project with ID {id} not found");

        return project;
    }

    public async Task<List<Project>> GetByWorkspaceIdAsync(Guid workspaceId)
    {
        return await _context.Projects
            .Include(p => p.Members)
            .Where(p => p.WorkspaceId == workspaceId && !p.IsArchived)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<ProjectMember?> GetMemberAsync(Guid projectId, Guid userId)
    {
        return await _context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
    }

    public async Task<bool> IsMemberAsync(Guid projectId, Guid userId)
    {
        return await _context.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
    }

    public async Task<List<Project>> GetAllAsync()
    {
        return await _context.Projects
            .Include(p => p.Members)
            .Where(p => !p.IsArchived)
            .ToListAsync();
    }

    public async Task AddAsync(Project project)
    {
        await _context.Projects.AddAsync(project);
    }

    public async Task UpdateAsync(Project project)
    {
        _context.Projects.Update(project);
    }

    public async Task DeleteAsync(Project project)
    {
        _context.Projects.Remove(project);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetWorkspaceProjectCountAsync(Guid workspaceId)
    {
        return await _context.Projects
            .CountAsync(p => p.WorkspaceId == workspaceId && !p.IsArchived);
    }

    public async Task<List<Project>> GetWorkspaceProjectsAsync(Guid workspaceId)
    {
        return await _context.Projects
            .Include(p => p.Members)
            .Where(p => p.WorkspaceId == workspaceId && !p.IsArchived)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
    public async Task<List<ProjectMember>> GetProjectMembersAsync (Guid projectId)
    {
        var members = await _context.ProjectMembers
        .Where(pm => pm.ProjectId == projectId)
        .OrderBy(pm => pm.AddedAt)
        .ToListAsync();

        return members;
    }
}