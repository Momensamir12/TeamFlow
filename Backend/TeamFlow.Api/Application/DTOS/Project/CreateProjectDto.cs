namespace App.Application.Dto;

public class CreateProjectDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required Guid WorkspaceId { get; set; }
}