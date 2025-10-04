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

        public async Task<Result> RegisterUserAsync(RegisterRequestDTO userDTO)
        {
            if (await _securityUserRepository.UsernameExistsAsync(userDTO.Username))
                return Result.Failure("Username already exists");

            var user = new SecurityUser { Username = userDTO.Username };
            user.PasswordHash = new PasswordHasher<SecurityUser>().HashPassword(user, userDTO.Password);

            await _securityUserRepository.AddAsync(user);
            await _securityUserRepository.SaveChangesAsync();

            return Result.Success();
        }


        public async Task<TokenResponseDTO?> LoginAsync(LoginRequestDTO userDTO)
        {
            var user = await _securityUserRepository.GetByUsernameAsync(userDTO.Username);
            if (user is null)
                return null;

            var passwordResult = new PasswordHasher<SecurityUser>()
                .VerifyHashedPassword(user, user.PasswordHash, userDTO.Password);

            if (passwordResult == PasswordVerificationResult.Failed)
                return null;

            return await GenerateToken(user);
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
