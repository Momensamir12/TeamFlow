using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using App.Application.Authentication.DTOS;
using App.Common;
using App.Infrastructure.Auth.Entities;
using App.Infrastructure.Repositories;
using App.Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using App.Application.Interfaces;
using App.Application.Common;
using App.Application.Services;

namespace App.Infrastructure.Authentication.Service;

public class AuthenticationService
{
    private readonly ISecurityUserRepository _securityUserRepository;
    private readonly JwtSettings _jwtSettings;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly TemplateRenderer _templateRenderer;

    public AuthenticationService(
        ISecurityUserRepository securityUserRepository,
        IOptions<JwtSettings> jwtOptions,
        IHttpContextAccessor httpContextAccessor,
        IEmailService emailService,
        IConfiguration configuration,
        TemplateRenderer templateRenderer)
    {
        _securityUserRepository = securityUserRepository;
        _jwtSettings = jwtOptions.Value;
        _httpContextAccessor = httpContextAccessor;
        _emailService = emailService;
        _configuration = configuration;
        _templateRenderer = templateRenderer;
    }

    public async Task<SecurityUser?> RegisterUserAsync(RegisterRequestDTO request, Guid userId)
    {
        if (await _securityUserRepository.UsernameExistsAsync(request.Username))
            throw new UsernameExistsException();

        var verificationToken = TokenGenerator.GenerateSecureToken();

        var user = new SecurityUser
        {
            Id = userId,
            Username = request.Username,
            Email = request.Email,
            IsActive = true,
            IsEmailVerified = false,
            EmailVerificationToken = verificationToken,
            EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        user.PasswordHash = new PasswordHasher<SecurityUser>().HashPassword(user, request.Password);

        await _securityUserRepository.AddAsync(user);
        await _securityUserRepository.SaveChangesAsync();

        // Send verification email
        var frontendUrl = _configuration["AppSettings:FrontendUrl"];
        var verificationLink = $"{frontendUrl}/verify-email?token={verificationToken}";
        
        var emailBody = _templateRenderer.Render("EmailVerification", new Dictionary<string, string>
        {
            { "Title", "Welcome to TeamFlow!" },
            { "Username", user.Username },
            { "Message", "Thank you for signing up! Please verify your email address to get started with TeamFlow." },
            { "VerificationLink", verificationLink }
        });

        await _emailService.SendEmailAsync(user.Email, "Verify Your Email - TeamFlow", emailBody, true);

        return user;
    }


    public async Task<TokenResponseDTO?> LoginAsync(LoginRequestDTO request)
    {
        var user = await _securityUserRepository.GetByUsernameAsync(request.Username);
        if (user is null)
            return null;

        var passwordResult = new PasswordHasher<SecurityUser>()
            .VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
            return null;

        return await GenerateToken(user);
    }

    public async Task<Result> DeleteUserByUsernameAsync(string username)
    {
        var user = await _securityUserRepository.GetByUsernameAsync(username);
        if (user == null)
            return Result.Failure("User not found");

        await _securityUserRepository.DeleteUserAsync(user);
        await _securityUserRepository.SaveChangesAsync();

        return Result.Success();
    }


    public async Task<TokenResponseDTO?> RefreshTokenAsync(RefreshTokenRequestDTO request)
    {
        var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
        if (user is null)
            return null;

        return await GenerateToken(user);
    }
    private async Task<TokenResponseDTO> GenerateToken(SecurityUser user)
    {
        return new TokenResponseDTO
        {
            AccessToken = CreateToken(user),
            RefreshToken = await GenerateAndSaveRefreshToken(user),
            UserId = user.Id,
            UserName = user.Username,
            Email = user.Email,
            IsEmailVerified = user.IsEmailVerified
        };
    }

    private string CreateToken(SecurityUser user)
    {
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("IsEmailVerified", user.IsEmailVerified.ToString())
            };

        if (user.Roles != null)
        {
            foreach (var role in user.Roles)
                claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.SecretKey!)
        );
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    private async Task<SecurityUser?> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
    {
        var user = await _securityUserRepository.GetByIdAsync(userId);
        if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return null;

        return user;
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }

    private async Task<string> GenerateAndSaveRefreshToken(SecurityUser user)
    {
        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(3);

        await _securityUserRepository.SaveChangesAsync();

        return refreshToken;
    }
    public Guid? GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user == null)
            return null;

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
            return null;

        return Guid.TryParse(userIdClaim.Value, out var userId) ? userId : null;
    }

    public async Task<Result> VerifyEmailAsync(string token)
    {
        var user = await _securityUserRepository.GetByEmailVerificationTokenAsync(token);
        
        if (user == null)
            return Result.Failure("Invalid verification token");

        if (user.EmailVerificationTokenExpiry < DateTime.UtcNow)
            return Result.Failure("Verification token has expired");

        if (user.IsEmailVerified)
            return Result.Failure("Email is already verified");

        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _securityUserRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> ResendVerificationEmailAsync(string email)
    {
        var user = await _securityUserRepository.GetByEmailAsync(email);
        
        if (user == null)
            return Result.Failure("User not found");

        if (user.IsEmailVerified)
            return Result.Failure("Email is already verified");

        var verificationToken = TokenGenerator.GenerateSecureToken();
        user.EmailVerificationToken = verificationToken;
        user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
        user.UpdatedAt = DateTime.UtcNow;

        await _securityUserRepository.SaveChangesAsync();

        // Send verification email
        var frontendUrl = _configuration["AppSettings:FrontendUrl"];
        var verificationLink = $"{frontendUrl}/verify-email?token={verificationToken}";
        
        var emailBody = _templateRenderer.Render("EmailVerification", new Dictionary<string, string>
        {
            { "Title", "Verify Your Email" },
            { "Username", user.Username },
            { "Message", "You requested a new verification email. Click the button below to verify your email address:" },
            { "VerificationLink", verificationLink }
        });

        await _emailService.SendEmailAsync(user.Email, "Verify Your Email - TeamFlow", emailBody, true);

        return Result.Success();
    }
}


