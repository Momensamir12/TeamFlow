using App.Application.Authentication.DTOS;
using App.Application.Data;
using App.Common;
using App.Domain.Model;
using App.Infrastructure.Authentication.Service;

namespace App.Application.Users.Service;

public class UserService
{
    private readonly AuthenticationService _authenticationService;
    private readonly AppDbContext _appDbContext;
    public UserService(AuthenticationService authenticationService, AppDbContext context)
{
    _authenticationService = authenticationService;
    _appDbContext = context;
}
public async Task<Result> RegisterUserAsync(RegisterRequestDTO request)
{
    var authResult = await _authenticationService.RegisterUserAsync(request);
    if (!authResult.Succeeded)
        return Result.Failure(authResult.Error ?? "Unknown authentication error");

    try
    {
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _appDbContext.Users.AddAsync(user);
        await _appDbContext.SaveChangesAsync();

        return Result.Success();
    }
    catch (Exception ex)
    {
        await _authenticationService.DeleteUserByUsernameAsync(request.Username);
        return Result.Failure("User registration failed: " + ex.Message);
    }
}

};