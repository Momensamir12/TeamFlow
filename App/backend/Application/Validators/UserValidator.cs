using App.Domain.Model;
using App.Infrastructure.Repositories;

public class UserValidator
{
    private readonly IUserRepository _userRepository;

    public UserValidator(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User> EnsureActiveUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null || !user.IsActive)
            throw new KeyNotFoundException($"User with ID {userId} not found or inactive.");

        return user;
    }

    public async Task<User> EnsureUserExistsAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new KeyNotFoundException($"User with ID {userId} not found.");

        return user;
    }
}
