using App.Domain.Model;
namespace App.Application.Dto;

public class WorkspaceMemberDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public int Role { get; set; }  // Changed from WorkspaceRole enum to int
    public DateTime JoinedAt { get; set; }
}