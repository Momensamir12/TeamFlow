using App.Application.DTOs;
using App.Application.Interfaces;
using App.Domain.Model;
using App.Infrastructure.Repositories;
using AutoMapper;

namespace App.Application.Services;

public class TaskCommentService
{
    private readonly ITaskCommentRepository _commentRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly UserValidator _userValidator;
    private readonly ITaskAuthorizer _taskAuthorizer;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public TaskCommentService(
        ITaskCommentRepository commentRepository,
        ITaskRepository taskRepository,
        UserValidator userValidator,
        ITaskAuthorizer taskAuthorizer,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _commentRepository = commentRepository;
        _taskRepository = taskRepository;
        _userValidator = userValidator;
        _taskAuthorizer = taskAuthorizer;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<TaskCommentDto> CreateCommentAsync(CreateTaskCommentDto dto, Guid userId)
    {
        await _userValidator.ActiveUserAsync(userId);

        var task = await _taskRepository.GetByIdAsync(dto.TaskId);
        await _taskAuthorizer.EnsureCanAccessAsync(task);

        var comment = new TaskComment(dto.TaskId, userId, dto.Content);
        await _commentRepository.CreateAsync(comment);
        await _commentRepository.SaveChangesAsync();

        var user = await _userRepository.GetByIdAsync(userId);
        var commentDto = _mapper.Map<TaskCommentDto>(comment);
        commentDto.Username = user?.Username ?? string.Empty;

        return commentDto;
    }

    public async Task<IEnumerable<TaskCommentDto>> GetTaskCommentsAsync(Guid taskId, Guid userId)
    {
        await _userValidator.ActiveUserAsync(userId);

        var task = await _taskRepository.GetByIdAsync(taskId);
        await _taskAuthorizer.EnsureCanAccessAsync(task);

        var comments = await _commentRepository.GetByTaskIdAsync(taskId);
        return _mapper.Map<IEnumerable<TaskCommentDto>>(comments);
    }

    public async Task<TaskCommentDto> UpdateCommentAsync(Guid commentId, UpdateTaskCommentDto dto, Guid userId)
    {
        await _userValidator.ActiveUserAsync(userId);

        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
            throw new InvalidOperationException($"Comment with id {commentId} not found");

        if (comment.UserId != userId)
            throw new UnauthorizedAccessException("You can only edit your own comments");

        comment.UpdateContent(dto.Content);
        await _commentRepository.UpdateAsync(comment);
        await _commentRepository.SaveChangesAsync();

        var commentDto = _mapper.Map<TaskCommentDto>(comment);
        commentDto.Username = comment.User?.Username ?? string.Empty;

        return commentDto;
    }

    public async Task DeleteCommentAsync(Guid commentId, Guid userId)
    {
        await _userValidator.ActiveUserAsync(userId);

        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
            throw new InvalidOperationException($"Comment with id {commentId} not found");

        // Verify user is the comment author or task owner
        var task = comment.Task ?? await _taskRepository.GetByIdAsync(comment.TaskId);
        if (comment.UserId != userId && task.OwnerId != userId)
            throw new UnauthorizedAccessException("You can only delete your own comments or are not the task owner");

        await _commentRepository.DeleteAsync(commentId);
        await _commentRepository.SaveChangesAsync();
    }
}
