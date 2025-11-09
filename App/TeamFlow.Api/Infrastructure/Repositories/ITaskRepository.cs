using App.Domain.Model;

namespace App.Infrastructure.Repositories;
public interface ITaskRepository
{
    Task<UserTask> GetByIdAsync(Guid id);
    Task<List<UserTask>> GetAssigneeTasksAsync(Guid assigneeId);
    Task AddAsync(UserTask task);
    Task Update(UserTask task);
    Task DeleteAsync(UserTask task);
    Task SaveChangesAsync();
    Task<List<UserTask>> GetProjectTasksAsync(Guid ProjectId);
    
};