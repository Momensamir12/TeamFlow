using System.Transactions;
using App.Application.Authentication.DTOS;
using App.Application.Data;
using App.Common;
using App.Domain.Model;
using App.Infrastructure.Authentication.Service;

namespace App.Application.Users.Service;

public class RegisterationService
{
    private readonly AuthenticationService _authenticationService;
    private readonly IUserRepository _userRepository;

    public RegisterationService(AuthenticationService authenticationService, IUserRepository userRepository)
    {
        _authenticationService = authenticationService;
        _userRepository = userRepository;
    }

    public async Task<Result> RegisterUserAsync(RegisterRequestDTO request)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

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

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            var securityUser = await _authenticationService.RegisterUserAsync(request, user.Id);

            if (securityUser is null)
                return Result.Failure("Error while registering security user");

            scope.Complete();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"User registration failed: {ex.Message}");
        }
    }
}
