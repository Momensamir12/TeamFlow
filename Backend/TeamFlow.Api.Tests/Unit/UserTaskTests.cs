using App.Domain.Model;
using FluentValidation;

namespace backend.Tests;

public class UserTaskTests
{
    [Fact]
    public void UpdateTitle_ShouldThrow_WhenTitleIsEmpty()
    {
        var task = new UserTask();
        Assert.Throws<ArgumentException>(() => task.UpdateTitle(" "));
    }

    [Fact]
    public void UpdateStatus_ShouldUpdateTimestamp_WhenStatusChanges()
    {
        var task = new UserTask { Status = TaskStatus.Todo };
        var before = task.UpdatedAt;

        task.UpdateStatus(TaskStatus.Done);

        Assert.Equal(TaskStatus.Done, task.Status);
        Assert.True(task.UpdatedAt > before);
    }

    [Fact]
    public void UpdateStatus_ShouldNotChangeTimestamp_WhenStatusUnchanged()
    {
        var task = new UserTask { Status = TaskStatus.Todo };
        var before = task.UpdatedAt;

        task.UpdateStatus(TaskStatus.Todo);

        Assert.Equal(before, task.UpdatedAt);
    }

    [Fact]
    public void AssignTo_ShouldThrow_WhenAssigningPersonalTaskToOtherUser()
    {
        var ownerId = Guid.NewGuid();
        var task = new UserTask { OwnerId = ownerId, ProjectId = null }; 

        Assert.Throws<InvalidOperationException>(() => task.AssignTo(Guid.NewGuid()));
    }

    [Fact]
    public void UpdateDeadline_ShouldThrow_WhenDeadlineInPast()
    {
        var task = new UserTask();
        Assert.Throws<ArgumentException>(() => task.UpdateDeadline(DateTime.UtcNow.AddMinutes(-1)));
    }

    [Fact]
    public void IsPersonalTask_ShouldBeTrue_WhenProjectIdIsNull()
    {
        var task = new UserTask { ProjectId = null };
        Assert.True(task.IsPersonalTask);
    }
}
