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

        public async Task<SecurityUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
           await _context.SecurityUsers.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

        // IRepository<SecurityUser> implementation
        async Task<SecurityUser> IRepository<SecurityUser>.GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var user = await _context.SecurityUsers.FindAsync(new object[] { id }, cancellationToken);
            if (user is null)
                throw new KeyNotFoundException($"SecurityUser with id '{id}' not found.");
            return user;
        }

        public async Task<SecurityUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            await _context.SecurityUsers.FindAsync(new object[] { id }, cancellationToken);

        public async Task<SecurityUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
            await _context.SecurityUsers.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        public async Task<SecurityUser?> GetByEmailVerificationTokenAsync(string token, CancellationToken cancellationToken = default) =>
            await _context.SecurityUsers.FirstOrDefaultAsync(u => u.EmailVerificationToken == token, cancellationToken);

        public Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default) =>
            _context.SecurityUsers.AnyAsync(u => u.Username == username, cancellationToken);

        public async Task AddAsync(SecurityUser entity, CancellationToken cancellationToken = default)
            => await _context.SecurityUsers.AddAsync(entity, cancellationToken);

        public async Task UpdateAsync(SecurityUser entity, CancellationToken cancellationToken = default)
        {
            _context.SecurityUsers.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(SecurityUser entity, CancellationToken cancellationToken = default)
        {
            _context.SecurityUsers.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
            _context.SaveChangesAsync(cancellationToken);

        public Task<SecurityUser> DeleteUserAsync(SecurityUser user, CancellationToken cancellationToken = default)
        {
            _context.SecurityUsers.Remove(user);
            return Task.FromResult(user);
        }
    }
}