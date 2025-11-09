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
        public DbSet<Project> Projects { get; set; }
        public DbSet<WorkspaceMember> WorkspaceMembers { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }
        public DbSet<WorkspaceInvitation> WorkspaceInvitations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User
            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);

            // UserTask
            modelBuilder.Entity<UserTask>()
                .HasKey(t => t.Id);

            modelBuilder.Entity<UserTask>()
                .Property(t => t.OwnerId)
                .IsRequired();

            modelBuilder.Entity<UserTask>()
                .Property(t => t.AssigneeId)
                .IsRequired(false);

            modelBuilder.Entity<UserTask>()
                .Property(t => t.ProjectId)
                .IsRequired(false);

            // Workspace
            modelBuilder.Entity<Workspace>()
                .HasKey(w => w.Id);

            modelBuilder.Entity<Workspace>()
                .Property(w => w.OwnerId)
                .IsRequired();

            // Project
            modelBuilder.Entity<Project>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Project>()
                .Property(p => p.WorkspaceId)
                .IsRequired();

            modelBuilder.Entity<Project>()
                .Property(p => p.CreatedByUserId)
                .IsRequired();

            // WorkspaceMember
            modelBuilder.Entity<WorkspaceMember>()
                .HasKey(wm => wm.Id);

            modelBuilder.Entity<WorkspaceMember>()
                .Property(wm => wm.WorkspaceId)
                .IsRequired();

            modelBuilder.Entity<WorkspaceMember>()
                .Property(wm => wm.UserId)
                .IsRequired();

            modelBuilder.Entity<WorkspaceMember>()
                .HasIndex(wm => new { wm.WorkspaceId, wm.UserId })
                .IsUnique();

            // ProjectMember - CONSOLIDATED (remove duplicates)
            modelBuilder.Entity<ProjectMember>(entity =>
            {
                entity.HasKey(pm => pm.Id);
                
                entity.Property(pm => pm.ProjectId)
                    .IsRequired();
                    
                entity.Property(pm => pm.UserId)
                    .IsRequired();
                
                entity.HasOne<Project>()
                    .WithMany(p => p.Members)
                    .HasForeignKey(pm => pm.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(pm => pm.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(pm => new { pm.ProjectId, pm.UserId })
                    .IsUnique();
            });

            // WorkspaceInvitation
            modelBuilder.Entity<WorkspaceInvitation>()
                .HasKey(wi => wi.Id);

            modelBuilder.Entity<WorkspaceInvitation>()
                .Property(wi => wi.WorkspaceId)
                .IsRequired();

            modelBuilder.Entity<WorkspaceInvitation>()
                .HasIndex(wi => wi.Token)
                .IsUnique();
        }
    }
}