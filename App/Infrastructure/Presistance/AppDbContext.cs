using App.Infrastructure.Auth.Entities;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Presistance
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<SecurityUser> Users { get; set; }
    }
}