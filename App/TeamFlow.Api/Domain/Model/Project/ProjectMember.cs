namespace App.Domain.Model;

public class ProjectMember
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public DateTime AddedAt { get; set; }
}