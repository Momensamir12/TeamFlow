using app.Migrations.AppDb;
using Microsoft.AspNetCore.SignalR;

namespace App.Domain.Model;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid WorkspaceId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<UserTask> Tasks { get; set; } = new List<UserTask>();
    public ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();

    public void Archive()
    {
        IsArchived = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string description)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }
    public void AddMember(Guid userId)
    {
        if (ProjectMembers.Any(m => m.UserId == userId))
            throw new InvalidOperationException("User is already a member of this project");

        ProjectMembers.Add(new ProjectMember
        {
            UserId = userId,
            ProjectId = Id,
            AddedAt = DateTime.UtcNow
        }
        );
    }

}