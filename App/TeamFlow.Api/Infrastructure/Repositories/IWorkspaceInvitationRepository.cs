using App.Domain.Model;

namespace App.Infrastructure.Repositories;

public interface IWorkspaceInvitationRepository : IRepository<WorkspaceInvitation>
{
    Task<WorkspaceInvitation?> GetByTokenAsync(string token);
    Task<bool> HasPendingInvitationAsync(Guid workspaceId, string email);
}