using App.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace App.Application.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserTask> Tasks { get; set; }
        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<WorkspaceMember> WorkspaceMembers { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // UserTask -> Assignee (optional)
            modelBuilder.Entity<UserTask>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(u => u.AssigneeId)
                .OnDelete(DeleteBehavior.SetNull);

            // UserTask -> Owner (required) 
            modelBuilder.Entity<UserTask>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(t => t.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);  

            // UserTask -> Project (optional)
            modelBuilder.Entity<UserTask>()
                .HasOne<Project>()
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);

            // Workspace -> Owner
            modelBuilder.Entity<Workspace>()
                .HasOne(w => w.Owner)
                .WithMany()
                .HasForeignKey(w => w.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // WorkspaceMember (Many-to-Many)
            modelBuilder.Entity<WorkspaceMember>()
                .HasKey(wm => wm.Id);

            modelBuilder.Entity<WorkspaceMember>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(wm => wm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkspaceMember>()
                .HasOne<Workspace>()
                .WithMany(w => w.Members)
                .HasForeignKey(wm => wm.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

          
            modelBuilder.Entity<WorkspaceMember>()
                .HasIndex(wm => new { wm.WorkspaceId, wm.UserId })
                .IsUnique()
                .HasFilter("[RemovedAt] IS NULL");

            // Project -> Workspace
            modelBuilder.Entity<Project>()
                .HasOne<Workspace>()
                .WithMany(w => w.Projects)
                .HasForeignKey(p => p.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Project -> CreatedByUser
            modelBuilder.Entity<Project>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(p => p.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ProjectMember (Many-to-Many)
            modelBuilder.Entity<ProjectMember>()
                .HasKey(pm => pm.Id);

            modelBuilder.Entity<ProjectMember>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(pm => pm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjectMember>()
                .HasOne<Project>()
                .WithMany(p => p.ProjectMembers)
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique: one user per project
            modelBuilder.Entity<ProjectMember>()
                .HasIndex(pm => new { pm.ProjectId, pm.UserId })
                .IsUnique();
        }
    }
}