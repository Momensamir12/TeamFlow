namespace App.Domain.Model;

public class TaskComment : BaseEntity
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public UserTask? Task { get; set; }
    public User? User { get; set; }

    public TaskComment() { }

    public TaskComment(Guid taskId, Guid userId, string content)
    {
        Id = Guid.NewGuid();
        TaskId = taskId;
        UserId = userId;
        Content = content ?? throw new ArgumentNullException(nameof(content));
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Comment content cannot be empty", nameof(content));
        
        Content = content;
        UpdatedAt = DateTime.UtcNow;
    }
}
