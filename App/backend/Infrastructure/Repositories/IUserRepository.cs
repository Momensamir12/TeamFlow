// In Application/Users/Interfaces/IUserRepository.cs
using App.Domain.Model;

public interface IUserRepository
{
    Task AddAsync(User user);
    Task SaveChangesAsync();
}
