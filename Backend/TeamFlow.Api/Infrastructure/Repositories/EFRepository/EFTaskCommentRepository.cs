using App.Application.Data;
using App.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class EFTaskCommentRepository : ITaskCommentRepository
{
    private readonly AppDbContext _context;

    public EFTaskCommentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TaskComment?> GetByIdAsync(Guid id)
    {
        return await _context.TaskComments
            .Include(tc => tc.User)
            .FirstOrDefaultAsync(tc => tc.Id == id);
    }

    public async Task<IEnumerable<TaskComment>> GetByTaskIdAsync(Guid taskId)
    {
        return await _context.TaskComments
            .Include(tc => tc.User)
            .Where(tc => tc.TaskId == taskId)
            .OrderBy(tc => tc.CreatedAt)
            .ToListAsync();
    }

    public async Task CreateAsync(TaskComment comment)
    {
        await _context.TaskComments.AddAsync(comment);
    }

    public async Task UpdateAsync(TaskComment comment)
    {
        _context.TaskComments.Update(comment);
    }

    public async Task DeleteAsync(Guid id)
    {
        var comment = await GetByIdAsync(id);
        if (comment != null)
        {
            _context.TaskComments.Remove(comment);
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
