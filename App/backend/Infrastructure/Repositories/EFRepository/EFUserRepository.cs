using App.Application.Data;
using App.Domain.Model;

namespace App.Infrastructure.Auth.Repositories;
public class EFUserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public EFUserRepository(AppDbContext context)
    {
        _context = context;
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
