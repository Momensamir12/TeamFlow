using App.Domain.Model;

namespace App.Application.Dto;

public class AddProjectMemberDto
{
    public required Guid ProjectId { get; set; }
    public required Guid UserId { get; set; }
    public required ProjectRole Role { get; set; }
}