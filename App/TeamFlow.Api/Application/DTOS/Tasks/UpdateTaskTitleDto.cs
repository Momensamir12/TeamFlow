
namespace App.Application.Dto;

public class UpdateTaskTitleDto
{
    public Guid TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
}