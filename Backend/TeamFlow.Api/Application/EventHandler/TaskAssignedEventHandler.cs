namespace App.Application.EventHandler;

using MediatR;
using Microsoft.Extensions.Logging;

public class TaskAssignedEventHandler : INotificationHandler<TaskAssignedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<TaskAssignedEventHandler> _logger;

    public TaskAssignedEventHandler(INotificationService notificationService, ILogger<TaskAssignedEventHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(TaskAssignedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("TaskAssignedEvent received - TaskId: {TaskId}, AssigneeId: {AssigneeId}", 
            notification.TaskId, notification.AssigneeId);
        
        try
        {
            await _notificationService.SendTaskAssignedNotification(
                notification.AssigneeId, notification.TaskId, cancellationToken);
            _logger.LogInformation("TaskAssignedEvent handled successfully for TaskId: {TaskId}", notification.TaskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle TaskAssignedEvent for TaskId: {TaskId}", notification.TaskId);
            throw;
        }
    }
}
