using App.Domain.Model;

namespace App.Application.Dto;

public class UpdateTaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? Deadline { get; set; }
    public Guid? AssigneeId { get; set; }
    public Guid? ProjectId { get; set; }
}
