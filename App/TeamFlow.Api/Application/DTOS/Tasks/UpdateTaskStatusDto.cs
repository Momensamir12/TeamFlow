
using Microsoft.AspNetCore.SignalR;

namespace App.Application.Dto;

public class UpdateTaskStatusDto
{
    public Guid TaskId { get; set; }
    public TaskStatus Status { get; set; }
};