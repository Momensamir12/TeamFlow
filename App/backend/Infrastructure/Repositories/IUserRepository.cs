// In Application/Users/Interfaces/IUserRepository.cs
using App.Domain.Model;

namespace App.Infrastructure.Repositories;
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task AddAsync(User user);
    Task SaveChangesAsync();
}
