namespace App.Application.DTOs;

public class TaskCommentDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateTaskCommentDto
{
    public Guid TaskId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class UpdateTaskCommentDto
{
    public string Content { get; set; } = string.Empty;
}
