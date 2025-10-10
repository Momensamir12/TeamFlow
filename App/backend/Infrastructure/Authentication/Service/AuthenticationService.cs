using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using App.Application.Authentication.DTOS;
using App.Common;
using App.Infrastructure.Auth.Entities;
using App.Infrastructure.Auth.Repositories;
using App.Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace App.Infrastructure.Authentication.Service
{
    public class AuthenticationService
    {
        private readonly ISecurityUserRepository _securityUserRepository;
        private readonly JwtSettings _jwtSettings;

        public AuthenticationService(ISecurityUserRepository securityUserRepository, IOptions<JwtSettings> jwtOptions)
        {
            _securityUserRepository = securityUserRepository;
            _jwtSettings = jwtOptions.Value;
        }

        public async Task<SecurityUser?> RegisterUserAsync(RegisterRequestDTO request, Guid userId)
        {
            if (await _securityUserRepository.UsernameExistsAsync(request.Username))
                return null;

            var user = new SecurityUser
            {
                Id = userId,
                Username = request.Username,
                Email = request.Email,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            user.PasswordHash = new PasswordHasher<SecurityUser>().HashPassword(user, request.Password);

            await _securityUserRepository.AddAsync(user);
            await _securityUserRepository.SaveChangesAsync();

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
                RefreshToken = await GenerateAndSaveRefreshToken(user)
            };
        }

        private string CreateToken(SecurityUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
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
    }
}
