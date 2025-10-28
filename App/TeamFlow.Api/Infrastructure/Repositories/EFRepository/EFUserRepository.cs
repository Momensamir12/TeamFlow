using App.Application.Data;
using App.Domain.Model;

namespace App.Infrastructure.Repositories;
public class EFUserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public EFUserRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<User?> GetByIdAsync (Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null)
        {
            throw new KeyNotFoundException($"User with id '{id}' not found.");
        }
        
        return user; 
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
