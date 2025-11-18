namespace App.Domain.Model;

public class UserTask : BaseEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime Deadline { get; set; }
    public TaskPriority Priority { get; set; }
    public Guid OwnerId { get; set; }
    public Guid? AssigneeId { get; set; }
    public string AssignerUsername { get; set; } = string.Empty;
    public Guid? ProjectId { get; set; }
    public bool IsPersonalTask => ProjectId == null;

    // Navigation properties
    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();

    public void UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty");
        Title = title;
        UpdatedAt = DateTime.UtcNow;
    }
    public void UpdateDescription(string description)
    {
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }
    public void UpdateStatus(TaskStatus newStatus)
    {
        if (Status == newStatus)
            return;
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignTo(Guid assigneeId)
    {
        if (IsPersonalTask && assigneeId != OwnerId)
            throw new InvalidOperationException("Cannot assign personal tasks to others");

        var previousAssigneeId = this.AssigneeId;
        this.AssigneeId = assigneeId;
        UpdatedAt = DateTime.UtcNow;
        
        if (previousAssigneeId != assigneeId && assigneeId != OwnerId)
        {
            AddDomainEvent(new TaskAssignedEvent(this.Id, this.AssigneeId.Value));
        }
    }
    public void UpdateDeadline(DateTime deadline)
    {
        if (deadline < DateTime.UtcNow)
            throw new ArgumentException("Deadline cannot be in the past");

        Deadline = deadline;
        UpdatedAt = DateTime.UtcNow;
    }
    public void UpdatePriority (TaskPriority priority)
    {
        Priority = priority;
        UpdatedAt = DateTime.UtcNow;
    }

};