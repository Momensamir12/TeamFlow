
namespace App.Application.Dto;

public class UpdateTaskDescriptionDto
{
    public Guid TaskId { get; set; }
    public string Description { get; set; } = string.Empty;
}