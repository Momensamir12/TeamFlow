public interface INotificationService
{
    Task SendTaskAssignedNotification(Guid userId, Guid taskId, CancellationToken cancellationToken = default);
    Task SendNotification(Guid userId, string notificationType, object payload, CancellationToken cancellationToken = default);
}
