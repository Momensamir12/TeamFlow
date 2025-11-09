using App.Application.Data;
using App.Domain.Model;
using App.Application.Exceptions;
using Microsoft.EntityFrameworkCore;


namespace App.Infrastructure.Repositories;

public class EFTaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public EFTaskRepository(AppDbContext appDbContext)
    {
        _context = appDbContext;
    }

    public async Task<UserTask> GetByIdAsync(Guid id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) 
            throw new TaskNotFoundException(id);
        return task;
    }

    public async Task AddAsync(UserTask task)
    {
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();
    }

    public async Task Update(UserTask task)
    {
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(UserTask task)
    {
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    public async Task<List<UserTask>> GetAssigneeTasksAsync(Guid assigneeId)
    {
        return await _context.Tasks
        .Where(t => t.AssigneeId == assigneeId)
        .OrderBy(t => t.Deadline)
        .ToListAsync();
    }
    public async Task<List<UserTask>> GetProjectTasksAsync (Guid projectId)
    {
        var tasks = await _context.Tasks
        .Where(t => t.ProjectId == projectId)
        .OrderBy(t => t.Deadline)
        .ToListAsync();

        return tasks;
    }
}
