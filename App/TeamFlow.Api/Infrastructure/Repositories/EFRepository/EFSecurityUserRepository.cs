using App.Infrastructure.Auth.Entities;
using App.Infrastructure.Data;
using App.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Auth.Repositories
{
    public class EFSecurityUserRepository : ISecurityUserRepository
    {
        private readonly SecurityDbContext _context;
        public EFSecurityUserRepository(SecurityDbContext context)
        {
            _context = context;
        }

        public async Task<SecurityUser?> GetByUsernameAsync(string username) =>
           await _context.SecurityUsers.FirstOrDefaultAsync(u => u.Username == username);

        public Task<SecurityUser?> GetByIdAsync(Guid id) =>
            _context.SecurityUsers.FindAsync(id).AsTask();

        public Task<bool> UsernameExistsAsync(string username) =>
            _context.SecurityUsers.AnyAsync(u => u.Username == username);

        public async Task AddAsync(SecurityUser user) =>
           await _context.SecurityUsers.AddAsync(user);

        public Task SaveChangesAsync() =>
            _context.SaveChangesAsync();
        public Task<SecurityUser> DeleteUserAsync(SecurityUser user)
        {
            _context.SecurityUsers.Remove(user);
            return Task.FromResult(user);
        }
    }
}