using App.Infrastructure.Auth.Entities;

namespace App.Infrastructure.Repositories;


public interface ISecurityUserRepository : IRepository<SecurityUser>
{
    Task<SecurityUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<SecurityUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<SecurityUser?> GetByEmailVerificationTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
    Task<SecurityUser> DeleteUserAsync(SecurityUser user, CancellationToken cancellationToken = default);
}

