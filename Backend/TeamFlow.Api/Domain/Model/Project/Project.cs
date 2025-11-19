using Microsoft.AspNetCore.SignalR;

namespace App.Domain.Model;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsArchived { get; set; }

    public List<ProjectMember> Members { get; set; } = new();

    public void AddMember(Guid userId, ProjectRole role)
    {
        if (Members.Any(m => m.UserId == userId))
            throw new InvalidOperationException("User is already a member of this project");

        Members.Add(new ProjectMember
        {
            ProjectId = Id,
            UserId = userId,
            Role = role,
            AddedAt = DateTime.UtcNow
        });

    }

    public void RemoveMember(Guid userId)
    {
        var member = Members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
            throw new InvalidOperationException("User is not a member of this project");

        Members.Remove(member);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateMemberRole(Guid userId, ProjectRole newRole)
    {
        var member = Members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
            throw new InvalidOperationException("User is not a member of this project");

        member.Role = newRole;
        UpdatedAt = DateTime.UtcNow;
    }
}