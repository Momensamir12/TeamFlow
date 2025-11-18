namespace App.Application.EventHandler;

using MediatR;

public class TaskAssignedEventHandler : INotificationHandler<TaskAssignedEvent>
{
    private readonly INotificationService _notificationService;

    public TaskAssignedEventHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Handle(TaskAssignedEvent notification, CancellationToken cancellationToken)
    {
        await _notificationService.SendTaskAssignedNotification(
            notification.AssigneeId, notification.TaskId, cancellationToken);
    }
}
