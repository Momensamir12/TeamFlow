namespace App.Application.Dto;

public class UpdateTaskDeadlineDto
{
    public Guid TaskId { get; set; }
    public DateTime Deadline { get; set; }
}