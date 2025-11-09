using App.Application.Data;
using App.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class EFWorkspaceInvitationRepository : IWorkspaceInvitationRepository
{
    private readonly AppDbContext _context;

    public EFWorkspaceInvitationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<WorkspaceInvitation> GetByIdAsync(Guid id)
    {
        var invitation = await _context.WorkspaceInvitations.FindAsync(id);
        if (invitation == null)
            throw new KeyNotFoundException($"Invitation with id '{id}' not found.");
        return invitation;
    }

    public async Task<WorkspaceInvitation?> GetByTokenAsync(string token)
    {
        return await _context.WorkspaceInvitations
            .FirstOrDefaultAsync(i => i.Token == token);
    }

    public async Task<bool> HasPendingInvitationAsync(Guid workspaceId, string email)
    {
        return await _context.WorkspaceInvitations
            .AnyAsync(i => i.WorkspaceId == workspaceId && 
                          i.Email == email && 
                          !i.IsAccepted && 
                          i.ExpiresAt > DateTime.UtcNow);
    }

    public async Task AddAsync(WorkspaceInvitation invitation)
    {
        await _context.WorkspaceInvitations.AddAsync(invitation);
    }

    public Task UpdateAsync(WorkspaceInvitation invitation)
    {
        _context.WorkspaceInvitations.Update(invitation);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(WorkspaceInvitation invitation)
    {
        _context.WorkspaceInvitations.Remove(invitation);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}