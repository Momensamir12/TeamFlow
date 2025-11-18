using App.Domain.Model;

namespace App.Application.Dto;

public class UpdateMemberRoleDto
{
    public required Guid WorkspaceId { get; set; }
    public required Guid MemberUserId { get; set; }
    public required WorkspaceRole NewRole { get; set; }
}