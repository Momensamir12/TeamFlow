namespace App.Application.Dto;

public class UpdateProjectDto
{
    public required Guid ProjectId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}