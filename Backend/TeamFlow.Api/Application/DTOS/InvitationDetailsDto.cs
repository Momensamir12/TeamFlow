namespace App.Application.Dto;

public class InvitationDetailsDto
{
    public required string WorkspaceName { get; set; }
    public required int Role { get; set; }
    public required string RoleName { get; set; }
    public required DateTime ExpiresAt { get; set; }
    public required bool IsExpired { get; set; }
    public required bool IsAccepted { get; set; }
}
