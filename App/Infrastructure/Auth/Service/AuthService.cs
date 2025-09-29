using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using App.Application.Auth.DTOS;
using App.Infrastructure.Auth.Entities;
using App.Infrastructure.Presistance;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
namespace App.Infrastructure.Auth.Service
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        public AuthService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        public async Task<SecurityUser?> RegisterUserAsync(SecurityUserDTO userDTO)
        {
            if (await _context.Users.AnyAsync(u => u.Username == userDTO.Username))
                return null;

            SecurityUser user = new SecurityUser();
            var password = new PasswordHasher<SecurityUser>()
            .HashPassword(user, userDTO.Password);
            user.PasswordHash = password;
            user.Username = userDTO.Username;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
        public async Task<string?> LoginAsync(SecurityUserDTO userDTO)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userDTO.Username);
            if (user is null)
            {
                return null;
            }
            if (new PasswordHasher<SecurityUser>().VerifyHashedPassword(user, user.PasswordHash, userDTO.Password) == PasswordVerificationResult.Failed)
            {
                return null;
            }
            String token = CreateToken(user);

            return token;
        }

        private string CreateToken(SecurityUser user)
        {
            var claims = new List<Claim>
            {
                new Claim (ClaimTypes.Name, user.Username),
                new Claim (ClaimTypes.NameIdentifier, user.Id.ToString())
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetValue<string>("AppSettings:Token")!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            var tokenDescriptor = new JwtSecurityToken(
                issuer: _config.GetValue<String>("AppSettings:Issuer"),
                audience: _config.GetValue<String>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}