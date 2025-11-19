// In Application/Users/Interfaces/IUserRepository.cs
using App.Domain.Model;


namespace App.Infrastructure.Repositories;
public interface IUserRepository : IRepository<User>
{
    // Additional user-specific methods can be added here
}
