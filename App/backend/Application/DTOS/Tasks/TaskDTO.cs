
using App.Domain.Model;

public class UserTaskDTO
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
    public string Priority { get; set; } = string.Empty;
    public Guid? AssigneeId { get; set; }
    public string AssignerUsername { get; set; } = string.Empty;
};