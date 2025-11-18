using App.Application.Dto;
using App.Application.Interfaces;
using App.Application.Services;
using App.Domain.Model;
using App.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace TeamFlow.Api.Tests.Integration;

public class TaskServiceIntegrationTests : BaseIntegrationTest
{
    private readonly TaskService _taskService;
    private readonly Mock<ITaskAuthorizer> _taskAuthorizerMock;
    private readonly Project _testProject;

    public TaskServiceIntegrationTests()
    {
        var mediatorMock = new Mock<MediatR.IMediator>();
        var domainEventDispatcher = new App.Application.EventDispatcher.DomainEventDispatcher(mediatorMock.Object);
        var taskRepository = new EFTaskRepository(AppDbContext, domainEventDispatcher);
        var userRepository = new EFUserRepository(AppDbContext);
        var userValidator = new UserValidator(userRepository);
        _taskAuthorizerMock = new Mock<ITaskAuthorizer>();

        _taskService = new TaskService(userValidator, taskRepository, Mapper, _taskAuthorizerMock.Object);

        _testProject = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            WorkspaceId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        AppDbContext.Projects.Add(_testProject);
        AppDbContext.SaveChanges();

        // Setup default mock behaviors for happy path
        _taskAuthorizerMock.Setup(a => a.EnsureCanAccessAsync(It.IsAny<UserTask>())).Returns(Task.CompletedTask);
        _taskAuthorizerMock.Setup(a => a.EnsureCanModifyAsync(It.IsAny<UserTask>())).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task CreateTask_WithValidData_ShouldCreateTask()
    {
        // Arrange
        var createDto = new CreateTaskDto
        {
            Title = "New Task",
            Description = "Task description",
            Priority = TaskPriority.High,
            Status = TaskStatus.Todo,
            ProjectId = _testProject.Id
        };

        // Act
        await _taskService.CreateTaskAsync(createDto, TestUser.Id);

        // Assert
        var taskInDb = await AppDbContext.Tasks.FirstOrDefaultAsync();
        Assert.NotNull(taskInDb);
        Assert.Equal(createDto.Title, taskInDb.Title);
        Assert.Equal(createDto.Description, taskInDb.Description);
        Assert.Equal(TaskPriority.High, taskInDb.Priority);
        Assert.Equal(TestUser.Id, taskInDb.OwnerId);
        Assert.Equal(_testProject.Id, taskInDb.ProjectId);
    }

    [Fact]
    public async Task GetAssigneeTasks_ShouldReturnUserTasks()
    {
        // Arrange
        var task1 = new UserTask
        {
            Id = Guid.NewGuid(),
            Title = "User Task 1",
            Description = "Description",
            Priority = TaskPriority.High,
            Status = TaskStatus.Todo,
            ProjectId = _testProject.Id,
            OwnerId = TestUser.Id,
            AssigneeId = TestUser.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var task2 = new UserTask
        {
            Id = Guid.NewGuid(),
            Title = "User Task 2",
            Description = "Description",
            Priority = TaskPriority.Medium,
            Status = TaskStatus.InProgress,
            ProjectId = _testProject.Id,
            OwnerId = TestUser.Id,
            AssigneeId = TestUser.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        AppDbContext.Tasks.AddRange(task1, task2);
        await AppDbContext.SaveChangesAsync();

        // Act
        var result = await _taskService.GetAssigneeTasksAsync(TestUser.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, t => t.Title == "User Task 1");
        Assert.Contains(result, t => t.Title == "User Task 2");
    }

    [Fact]
    public async Task UpdateTaskStatus_WithValidData_ShouldUpdateStatus()
    {
        // Arrange
        var task = new UserTask
        {
            Id = Guid.NewGuid(),
            Title = "Task to Update",
            Priority = TaskPriority.Low,
            Status = TaskStatus.Todo,
            ProjectId = _testProject.Id,
            OwnerId = TestUser.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        AppDbContext.Tasks.Add(task);
        await AppDbContext.SaveChangesAsync();

        var updateDto = new UpdateTaskStatusDto
        {
            TaskId = task.Id,
            Status = TaskStatus.Done
        };

        // Act
        await _taskService.UpdateTaskStatusAsync(updateDto);

        // Assert
        var updatedTask = await AppDbContext.Tasks.FindAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal(TaskStatus.Done, updatedTask.Status);
    }

    [Fact]
    public async Task UpdateTaskTitle_WithValidData_ShouldUpdateTitle()
    {
        // Arrange
        var task = new UserTask
        {
            Id = Guid.NewGuid(),
            Title = "Original Title",
            Priority = TaskPriority.Medium,
            Status = TaskStatus.Todo,
            ProjectId = _testProject.Id,
            OwnerId = TestUser.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        AppDbContext.Tasks.Add(task);
        await AppDbContext.SaveChangesAsync();

        var updateDto = new UpdateTaskTitleDto
        {
            TaskId = task.Id,
            Title = "Updated Title"
        };

        // Act
        await _taskService.UpdateTaskTitleAsync(updateDto);

        // Assert
        var updatedTask = await AppDbContext.Tasks.FindAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal("Updated Title", updatedTask.Title);
    }

    [Fact]
    public async Task UpdateTaskTitle_WhenUserCannotModify_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var task = new UserTask
        {
            Id = Guid.NewGuid(),
            Title = "Task",
            ProjectId = _testProject.Id,
            OwnerId = Guid.NewGuid(), // Different owner
            CreatedAt = DateTime.UtcNow
        };
        AppDbContext.Tasks.Add(task);
        await AppDbContext.SaveChangesAsync();

        var updateDto = new UpdateTaskTitleDto
        {
            TaskId = task.Id,
            Title = "Updated Title"
        };

        _taskAuthorizerMock.Setup(a => a.EnsureCanModifyAsync(It.IsAny<UserTask>()))
            .ThrowsAsync(new UnauthorizedAccessException("Only task owner can perform this action"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _taskService.UpdateTaskTitleAsync(updateDto));
    }

    [Fact]
    public async Task UpdateTaskPriority_WithValidData_ShouldUpdatePriority()
    {
        // Arrange
        var task = new UserTask
        {
            Id = Guid.NewGuid(),
            Title = "Task",
            Priority = TaskPriority.Low,
            Status = TaskStatus.Todo,
            ProjectId = _testProject.Id,
            OwnerId = TestUser.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        AppDbContext.Tasks.Add(task);
        await AppDbContext.SaveChangesAsync();

        var updateDto = new UpdateTaskPriorityDto
        {
            TaskId = task.Id,
            Priority = TaskPriority.Urgent
        };

        // Act
        await _taskService.UpdateTaskPriorityAsync(updateDto);

        // Assert
        var updatedTask = await AppDbContext.Tasks.FindAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal(TaskPriority.Urgent, updatedTask.Priority);
    }

    [Fact]
    public async Task DeleteTask_WithValidId_ShouldDeleteTask()
    {
        // Arrange
        var task = new UserTask
        {
            Id = Guid.NewGuid(),
            Title = "Task to Delete",
            Priority = TaskPriority.Low,
            Status = TaskStatus.Todo,
            ProjectId = _testProject.Id,
            OwnerId = TestUser.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        AppDbContext.Tasks.Add(task);
        await AppDbContext.SaveChangesAsync();

        // Act
        await _taskService.DeleteTaskAsync(task.Id);

        // Assert
        var deletedTask = await AppDbContext.Tasks.FindAsync(task.Id);
        Assert.Null(deletedTask);
    }

    [Fact]
    public async Task UpdateTask_WithAllFields_ShouldUpdateAllFields()
    {
        // Arrange
        var task = new UserTask
        {
            Id = Guid.NewGuid(),
            Title = "Original",
            Description = "Original Desc",
            Priority = TaskPriority.Low,
            Status = TaskStatus.Todo,
            ProjectId = _testProject.Id,
            OwnerId = TestUser.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        AppDbContext.Tasks.Add(task);
        await AppDbContext.SaveChangesAsync();

        var updateDto = new UpdateTaskDto
        {
            Id = task.Id,
            Title = "Updated Title",
            Description = "Updated Description",
            Priority = TaskPriority.High,
            Status = TaskStatus.InProgress,
            Deadline = DateTime.UtcNow.AddDays(7)
        };

        // Act
        await _taskService.UpdateTaskAsync(updateDto);

        // Assert
        var updatedTask = await AppDbContext.Tasks.FindAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal("Updated Title", updatedTask.Title);
        Assert.Equal("Updated Description", updatedTask.Description);
        Assert.Equal(TaskPriority.High, updatedTask.Priority);
        Assert.Equal(TaskStatus.InProgress, updatedTask.Status);
        Assert.NotEqual(default(DateTime), updatedTask.Deadline);
    }

    [Fact]
    public async Task AssignTask_ToValidUser_ShouldAssignTask()
    {
        // Arrange
        var assignee = TestUserFactory.CreateTestUser("assignee", "assignee@example.com");
        AppDbContext.Users.Add(assignee);
        
        var task = new UserTask
        {
            Id = Guid.NewGuid(),
            Title = "Task",
            ProjectId = _testProject.Id,
            OwnerId = TestUser.Id,
            CreatedAt = DateTime.UtcNow
        };
        AppDbContext.Tasks.Add(task);
        await AppDbContext.SaveChangesAsync();

        var assignDto = new AssignTaskDto
        {
            TaskId = task.Id,
            AssigneeId = assignee.Id
        };

        // Act
        await _taskService.AssignTaskAsync(assignDto);

        // Assert
        var updatedTask = await AppDbContext.Tasks.FindAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal(assignee.Id, updatedTask.AssigneeId);
    }

    [Fact]
    public async Task UpdateTaskDescription_WithValidData_ShouldUpdateDescription()
    {
        // Arrange
        var task = new UserTask
        {
            Id = Guid.NewGuid(),
            Title = "Task",
            Description = "Original Description",
            ProjectId = _testProject.Id,
            OwnerId = TestUser.Id,
            CreatedAt = DateTime.UtcNow
        };
        AppDbContext.Tasks.Add(task);
        await AppDbContext.SaveChangesAsync();

        var updateDto = new UpdateTaskDescriptionDto
        {
            TaskId = task.Id,
            Description = "Updated Description"
        };

        // Act
        await _taskService.UpdateTaskDescriptionAsync(updateDto);

        // Assert
        var updatedTask = await AppDbContext.Tasks.FindAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal("Updated Description", updatedTask.Description);
    }
}
