
using App.Domain.Model;

namespace App.Application.Dto;

public class UpdateTaskPriorityDto
{
    public Guid TaskId { get; set; }
    public TaskPriority Priority { get; set; }
}