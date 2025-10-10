namespace App.Domain.Model;

public class UserTask
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    public TaskPriority Priority { get; set; }
    public Guid? AssigneeId { get; set; }
    public string AssignerUsername { get; set; } = string.Empty;
};