using App.Application.Data;
using App.Domain.Model;
using App.Application.Exceptions;
using Microsoft.EntityFrameworkCore;
using App.Application.EventDispatcher;


namespace App.Infrastructure.Repositories;

public class EFTaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;
    private readonly DomainEventDispatcher _domainEventDispatcher;

    public EFTaskRepository(AppDbContext appDbContext, DomainEventDispatcher domainEventDispatcher )
    {
        _context = appDbContext;
        _domainEventDispatcher  = domainEventDispatcher;
    }

    public async Task<UserTask> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await _context.Tasks.FindAsync(new object[] { id }, cancellationToken);
        if (task == null) 
            throw new TaskNotFoundException(id);
        return task;
    }

    public async Task AddAsync(UserTask task, CancellationToken cancellationToken = default)
    {
        await _context.Tasks.AddAsync(task, cancellationToken);
    }

    public async Task UpdateAsync(UserTask task, CancellationToken cancellationToken = default)
    {
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(UserTask task, CancellationToken cancellationToken = default)
    {
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
        await _domainEventDispatcher.DispatchEvents(_context, cancellationToken);
    }
    
    public async Task<List<UserTask>> GetAssigneeTasksAsync(Guid assigneeId, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
        .Where(t => t.AssigneeId == assigneeId)
        .OrderBy(t => t.Deadline)
        .ToListAsync(cancellationToken);
    }
    
    public async Task<List<UserTask>> GetProjectTasksAsync (Guid projectId, CancellationToken cancellationToken = default)
    {
        var tasks = await _context.Tasks
        .Where(t => t.ProjectId == projectId)
        .OrderBy(t => t.Deadline)
        .ToListAsync(cancellationToken);

        return tasks;
    }
}
