using Microsoft.AspNetCore.SignalR;

public class SignalRNotificationService : INotificationService 
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendTaskAssignedNotification(Guid userId, Guid taskId, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.User(userId.ToString()).SendAsync("NewTaskAssignedNotification", taskId, cancellationToken);
    }
    
    public async Task SendNotification(Guid userId, string notificationType, object payload, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.User(userId.ToString()).SendAsync(notificationType, payload, cancellationToken);
    }
}