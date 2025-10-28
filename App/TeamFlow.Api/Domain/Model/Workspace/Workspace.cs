namespace App.Domain.Model;

public class Workspace
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }

    public User Owner { get; set; } = null!;
    public ICollection<WorkspaceMember> Members { get; set; } = new List<WorkspaceMember>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();

    public void Archive()
    {
        IsArchived = true;
    }

    public void Unarchive()
    {
        IsArchived = false;
    }

    public void AddMember(Guid userId, WorkspaceRole role)
    {
        if (Members.Any(m => m.UserId == userId && m.RemovedAt == null))
            throw new InvalidOperationException("User is already a member of this workspace");

        Members.Add(new WorkspaceMember
        {
            UserId = userId,
            WorkspaceId = Id,
            Role = role,
            JoinedAt = DateTime.UtcNow
        });
    }

    public void RemoveMember(Guid userId)
    {
        var member = Members.FirstOrDefault(m => m.UserId == userId && m.RemovedAt == null);
        if (member == null)
            throw new InvalidOperationException("User is not a member of this workspace");

        member.RemovedAt = DateTime.UtcNow;
    }

    public void UpdateMemberRole(Guid userId, WorkspaceRole newRole)
    {
        var member = Members.FirstOrDefault(m => m.UserId == userId && m.RemovedAt == null);
        if (member == null)
            throw new InvalidOperationException("User is not a member of this workspace");

        member.Role = newRole;
    }
}