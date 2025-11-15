using App.Infrastructure.Auth.Entities;

namespace App.Infrastructure.Repositories;

    public interface ISecurityUserRepository
{
    Task<SecurityUser?> GetByUsernameAsync(string username);
    Task<SecurityUser?> GetByIdAsync(Guid id);
    Task<SecurityUser?> GetByEmailAsync(string email);
    Task<SecurityUser?> GetByEmailVerificationTokenAsync(string token);
    Task<bool> UsernameExistsAsync(string username);
    Task AddAsync(SecurityUser user);
    public Task<SecurityUser> DeleteUserAsync(SecurityUser user);

    Task SaveChangesAsync();
}

