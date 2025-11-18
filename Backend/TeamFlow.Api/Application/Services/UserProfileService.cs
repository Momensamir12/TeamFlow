using App.Application.Dto;
using App.Application.Interfaces;
using App.Common;
using App.Domain.Model;
using App.Infrastructure.Auth.Entities;
using App.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;

namespace App.Application.Services;

public class UserProfileService
{
    private readonly IUserRepository _userRepository;
    private readonly ISecurityUserRepository _securityUserRepository;
    private readonly IPasswordHasher<SecurityUser> _passwordHasher;
    private readonly IEmailService _emailService;

    public UserProfileService(
        IUserRepository userRepository,
        ISecurityUserRepository securityUserRepository,
        IPasswordHasher<SecurityUser> passwordHasher,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _securityUserRepository = securityUserRepository;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
    }

    public async Task<Result<UserProfileDto>> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return Result<UserProfileDto>.Failure("User not found");

            var securityUser = await _securityUserRepository.GetByIdAsync(userId, cancellationToken);
            if (securityUser == null)
                return Result<UserProfileDto>.Failure("Security user not found");

            var profileDto = new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl,
                IsEmailVerified = securityUser.IsEmailVerified,
                CreatedAt = user.CreatedAt
            };

            return Result<UserProfileDto>.Success(profileDto);
        }
        catch (Exception ex)
        {
            return Result<UserProfileDto>.Failure($"Error retrieving user profile: {ex.Message}");
        }
    }

    public async Task<Result> UpdateUserProfileAsync(Guid userId, UpdateUserProfileDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return Result.Failure("User not found");

            var securityUser = await _securityUserRepository.GetByIdAsync(userId, cancellationToken);
            if (securityUser == null)
                return Result.Failure("Security user not found");

            if (updateDto.Email != user.Email && updateDto.Email != securityUser.Email)
            {
                var existingSecurityUser = await _securityUserRepository.GetByEmailAsync(updateDto.Email, cancellationToken);
                if (existingSecurityUser != null && existingSecurityUser.Id != userId)
                    return Result.Failure("Email is already in use by another user");

                securityUser.Email = updateDto.Email;
                securityUser.IsEmailVerified = false;
            }

            user.FirstName = updateDto.FirstName;
            user.LastName = updateDto.LastName;
            user.Email = updateDto.Email;
            user.AvatarUrl = updateDto.AvatarUrl;
            user.UpdatedAt = DateTime.UtcNow;

            // Update SecurityUser entity
            securityUser.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _securityUserRepository.UpdateAsync(securityUser, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error updating user profile: {ex.Message}");
        }
    }

    public async Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var securityUser = await _securityUserRepository.GetByIdAsync(userId, cancellationToken);
            if (securityUser == null)
                return Result.Failure("User not found");

     
            var verificationResult = _passwordHasher.VerifyHashedPassword(
                securityUser,
                securityUser.PasswordHash,
                changePasswordDto.CurrentPassword);

            if (verificationResult == PasswordVerificationResult.Failed)
                return Result.Failure("Current password is incorrect");

            
            securityUser.PasswordHash = _passwordHasher.HashPassword(securityUser, changePasswordDto.NewPassword);
            securityUser.UpdatedAt = DateTime.UtcNow;

            await _securityUserRepository.UpdateAsync(securityUser, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error changing password: {ex.Message}");
        }
    }

    public async Task<Result> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var securityUser = await _securityUserRepository.GetByEmailAsync(forgotPasswordDto.Email, cancellationToken);
            if (securityUser == null)
            {
                return Result.Success();
            }

            var resetToken = Guid.NewGuid().ToString();
            securityUser.EmailVerificationToken = resetToken; 
            securityUser.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(1);
            securityUser.UpdatedAt = DateTime.UtcNow;

            await _securityUserRepository.UpdateAsync(securityUser, cancellationToken);

            await _emailService.SendPasswordResetEmailAsync(securityUser.Email, resetToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error processing password reset request: {ex.Message}");
        }
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordDto resetPasswordDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var securityUser = await _securityUserRepository.GetByEmailVerificationTokenAsync(resetPasswordDto.Token, cancellationToken);
            if (securityUser == null)
                return Result.Failure("Invalid or expired reset token");

            if (securityUser.EmailVerificationTokenExpiry == null || securityUser.EmailVerificationTokenExpiry < DateTime.UtcNow)
                return Result.Failure("Reset token has expired");

            securityUser.PasswordHash = _passwordHasher.HashPassword(securityUser, resetPasswordDto.NewPassword);
            securityUser.EmailVerificationToken = null;
            securityUser.EmailVerificationTokenExpiry = null;
            securityUser.UpdatedAt = DateTime.UtcNow;

            await _securityUserRepository.UpdateAsync(securityUser, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error resetting password: {ex.Message}");
        }
    }
}
