using App.Application.Dto;
using App.Application.Interfaces;
using App.Domain.Model;
using App.Infrastructure.Repositories;
using AutoMapper;

namespace App.Application.Services;

public class TaskService
{
    private readonly UserValidator _userValidator;
    private readonly ITaskRepository _taskRepository;
    private readonly IMapper _mapper;
    private readonly ITaskAuthorizer _taskAuthorizer;

    public TaskService(
        UserValidator userValidator,
        ITaskRepository taskRepository,
        IMapper mapper,
        ITaskAuthorizer taskAuthorizer)
    {
        _userValidator = userValidator;
        _taskRepository = taskRepository;
        _mapper = mapper;
        _taskAuthorizer = taskAuthorizer;
    }

    public async Task CreateTaskAsync(CreateTaskDto taskDTO, Guid userId)
    {
        await _userValidator.ActiveUserAsync(userId);

        var task = _mapper.Map<UserTask>(taskDTO);
        task.OwnerId = userId;
        task.ProjectId = taskDTO.ProjectId;
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;
        task.AssignTo(taskDTO.AssigneeId ?? userId);

        await _taskRepository.AddAsync(task);
        await _taskRepository.SaveChangesAsync();
    }

    public async Task<List<UserTaskDto>> GetAssigneeTasksAsync(Guid userId)
    {
        await _userValidator.ActiveUserAsync(userId);

        var tasks = await _taskRepository.GetAssigneeTasksAsync(userId);
        var dtos = _mapper.Map<List<UserTaskDto>>(tasks);
        
        return dtos;
    }

    public async Task UpdateTaskStatusAsync(UpdateTaskStatusDto dto)
    {
        var task = await GetAndAuthorizeTaskAsync(dto.TaskId, requireModify: false);
        task.UpdateStatus(dto.Status);
        await _taskRepository.SaveChangesAsync();
    }

    public async Task UpdateTaskTitleAsync(UpdateTaskTitleDto dto)
    {
        var task = await GetAndAuthorizeTaskAsync(dto.TaskId);
        task.UpdateTitle(dto.Title);
        await _taskRepository.SaveChangesAsync();
    }

    public async Task UpdateTaskDescriptionAsync(UpdateTaskDescriptionDto dto)
    {
        var task = await GetAndAuthorizeTaskAsync(dto.TaskId);
        task.UpdateDescription(dto.Description);
        await _taskRepository.SaveChangesAsync();
    }

    public async Task UpdateTaskDeadlineAsync(UpdateTaskDeadlineDto dto)
    {
        var task = await GetAndAuthorizeTaskAsync(dto.TaskId);
        task.UpdateDeadline(dto.Deadline);
        await _taskRepository.SaveChangesAsync();
    }

    public async Task UpdateTaskPriorityAsync(UpdateTaskPriorityDto dto)
    {
        var task = await GetAndAuthorizeTaskAsync(dto.TaskId);
        task.UpdatePriority(dto.Priority);
        await _taskRepository.SaveChangesAsync();
    }

    public async Task AssignTaskAsync(AssignTaskDto dto)
    {
        var task = await GetAndAuthorizeTaskAsync(dto.TaskId);

        if (dto.AssigneeId.HasValue)
        {
            await _userValidator.ActiveUserAsync(dto.AssigneeId.Value);
        }

        task.AssignTo(dto.AssigneeId);
        await _taskRepository.SaveChangesAsync();
    }

    public async Task UpdateTaskAsync(UpdateTaskDto dto)
    {
        var task = await GetAndAuthorizeTaskAsync(dto.Id);
        
        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Status = dto.Status;
        task.Priority = dto.Priority;
        
        if (dto.Deadline.HasValue)
        {
            task.Deadline = dto.Deadline.Value;
        }
        
        if (dto.AssigneeId.HasValue)
        {
            await _userValidator.ActiveUserAsync(dto.AssigneeId.Value);
            task.AssigneeId = dto.AssigneeId;
        }
        
        if (dto.ProjectId.HasValue)
        {
            task.ProjectId = dto.ProjectId;
        }
        
        task.UpdatedAt = DateTime.UtcNow;
        await _taskRepository.SaveChangesAsync();
    }

    public async Task DeleteTaskAsync(Guid taskId)
    {
        var task = await GetAndAuthorizeTaskAsync(taskId);
        await _taskRepository.DeleteAsync(task);
        await _taskRepository.SaveChangesAsync();
    }

    // loads task and applies authorization
    private async Task<UserTask> GetAndAuthorizeTaskAsync(Guid taskId, bool requireModify = true)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);

        if (requireModify)
            await _taskAuthorizer.EnsureCanModifyAsync(task);
        else
            await _taskAuthorizer.EnsureCanAccessAsync(task);

        return task;
    }
    
}
