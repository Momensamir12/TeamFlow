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

    public async Task<WorkspaceInvitation> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invitation = await _context.WorkspaceInvitations.FindAsync(new object[] { id }, cancellationToken);
        if (invitation == null)
            throw new KeyNotFoundException($"Invitation with id '{id}' not found.");
        return invitation;
    }

    public async Task<WorkspaceInvitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.WorkspaceInvitations
            .FirstOrDefaultAsync(i => i.Token == token, cancellationToken);
    }

    public async Task<bool> HasPendingInvitationAsync(Guid workspaceId, string email, CancellationToken cancellationToken = default)
    {
        return await _context.WorkspaceInvitations
            .AnyAsync(i => i.WorkspaceId == workspaceId && 
                          i.Email == email && 
                          !i.IsAccepted && 
                          i.ExpiresAt > DateTime.UtcNow, cancellationToken);
    }

    public async Task AddAsync(WorkspaceInvitation invitation, CancellationToken cancellationToken = default)
    {
        await _context.WorkspaceInvitations.AddAsync(invitation, cancellationToken);
    }

    public Task UpdateAsync(WorkspaceInvitation invitation, CancellationToken cancellationToken = default)
    {
        _context.WorkspaceInvitations.Update(invitation);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(WorkspaceInvitation invitation, CancellationToken cancellationToken = default)
    {
        _context.WorkspaceInvitations.Remove(invitation);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}