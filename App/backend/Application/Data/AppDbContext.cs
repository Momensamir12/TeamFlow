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
            modelBuilder.Entity<UserTask>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(u => u.AssigneeId);
        }

    }
}