using App.Domain.Model;

namespace App.Infrastructure.Repositories;
public interface ITaskRepository : IRepository<UserTask>
{
    Task<List<UserTask>> GetAssigneeTasksAsync(Guid assigneeId, CancellationToken cancellationToken = default);
    Task<List<UserTask>> GetProjectTasksAsync(Guid projectId, CancellationToken cancellationToken = default);
}