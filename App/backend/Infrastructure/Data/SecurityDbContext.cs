using App.Infrastructure.Auth.Entities;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Data
{
    public class SecurityDbContext : DbContext
    {
        public SecurityDbContext(DbContextOptions<SecurityDbContext> options) : base(options) { }

        public DbSet<SecurityUser> SecurityUsers { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
            modelBuilder.Entity<SecurityUser>()
                .HasIndex(u => u.Username)
                .IsUnique();
    }
    }
}