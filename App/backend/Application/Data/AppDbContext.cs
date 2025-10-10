using App.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace App.Application.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserTask> Tasks { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
            .HasMany(e => e.Tasks)
            .WithOne()
            .HasForeignKey(t => t.AssigneeId);

            modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
        }
    }
}