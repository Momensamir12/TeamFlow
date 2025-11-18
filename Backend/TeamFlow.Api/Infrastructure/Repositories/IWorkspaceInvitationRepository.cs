using App.Domain.Model;

namespace App.Infrastructure.Repositories;

public interface IWorkspaceInvitationRepository : IRepository<WorkspaceInvitation>
{
    Task<WorkspaceInvitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> HasPendingInvitationAsync(Guid workspaceId, string email, CancellationToken cancellationToken = default);
}