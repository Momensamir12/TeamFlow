using App.Domain.Model;

namespace App.Application.Dto;


public class InvitationRequestDto
{
    public Guid WorkspaceId { get; set; }
    public string Email { get; set; } = string.Empty;
    public WorkspaceRole Role { get; set; }
}