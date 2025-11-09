using App.Domain.Model;

namespace App.Application.Dto;

public class UserTaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Status { get; set; }  
    public DateTime? Deadline { get; set; }
    public int Priority { get; set; }  
    public Guid? AssigneeId { get; set; }
    public string AssignerUsername { get; set; } = string.Empty;
    public Guid? ProjectId { get; set; }
}