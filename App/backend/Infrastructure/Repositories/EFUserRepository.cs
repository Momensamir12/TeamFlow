using App.Infrastructure.Auth.Entities;
using App.Infrastructure.Presistance;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Auth.Repositories
{
    public class EFUserRepository : ISecurityUserRepository
    {
        private readonly AppDbContext _context;
        public EFUserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SecurityUser?> GetByUsernameAsync(string username) =>
           await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

        public Task<SecurityUser?> GetByIdAsync(Guid id) =>
            _context.Users.FindAsync(id).AsTask();

        public Task<bool> UsernameExistsAsync(string username) =>
            _context.Users.AnyAsync(u => u.Username == username);

        public async Task AddAsync(SecurityUser user) =>
           await _context.Users.AddAsync(user);

        public Task SaveChangesAsync() =>
            _context.SaveChangesAsync();
    }
}