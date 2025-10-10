using App.Infrastructure.Auth.Entities;

namespace App.Infrastructure.Auth.Repositories
{
    public interface ISecurityUserRepository
{
    Task<SecurityUser?> GetByUsernameAsync(string username);
    Task<SecurityUser?> GetByIdAsync(Guid id);
    Task<bool> UsernameExistsAsync(string username);
    Task AddAsync(SecurityUser user);
    public Task<SecurityUser> DeleteUserAsync(SecurityUser user);

    Task SaveChangesAsync();
}

}