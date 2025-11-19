namespace App.Application.Dto;

public class UpdateWorkspaceDto
{
    public required Guid WorkspaceId { get; set; }
    public required string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
}