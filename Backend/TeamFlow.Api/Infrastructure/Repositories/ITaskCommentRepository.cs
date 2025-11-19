using App.Domain.Model;

namespace App.Infrastructure.Repositories;

public interface ITaskCommentRepository
{
    Task<TaskComment?> GetByIdAsync(Guid id);
    Task<IEnumerable<TaskComment>> GetByTaskIdAsync(Guid taskId);
    Task CreateAsync(TaskComment comment);
    Task UpdateAsync(TaskComment comment);
    Task DeleteAsync(Guid id);
    Task SaveChangesAsync();
}
