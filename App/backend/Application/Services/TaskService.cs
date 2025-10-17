using App.Application.Data;
using App.Application.Interfaces;
using App.Domain.Model;
using App.Infrastructure.Authentication.Service;
using App.Infrastructure.Repositories;
using App.Infrastructure.Services;
using AutoMapper;

namespace App.Application.Services;

public class TaskService
{
    private readonly UserValidator _userValidator;
    private readonly ITaskRepository _taskRepository;
    private readonly IMapper _mapper;


    public TaskService(
        UserValidator userValidator,
        ITaskRepository taskRepository,
        IMapper mapper)
    {
        _userValidator = userValidator;
        _taskRepository = taskRepository;
        _mapper = mapper;
    }

    // for now you can only add tasks to yourself , to be extended later with workspaces and teams
    public async Task CreateTaskAsync(UserTaskDTO taskDTO, Guid userId)
    {
        var user = await _userValidator.EnsureActiveUserAsync(userId);

        var task = _mapper.Map<UserTask>(taskDTO);

        task.AssigneeId = userId;
        task.Status = TaskStatus.Todo;
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;

        await _taskRepository.AddAsync(task);
        await _taskRepository.SaveChangesAsync();
    }

    public async Task<List<UserTaskDTO>> GetAssigneeTasksAsync(Guid userId)
    {
        var user = await _userValidator.EnsureActiveUserAsync(userId);
        var tasks = await _taskRepository.GetAssigneeTasksAsync(userId);

        return _mapper.Map<List<UserTaskDTO>>(tasks);
    }
}
