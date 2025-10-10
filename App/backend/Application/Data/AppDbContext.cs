using App.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace App.Application.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
    }
    }
}