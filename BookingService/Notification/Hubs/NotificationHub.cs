using Microsoft.AspNetCore.SignalR;

namespace Notification.Hubs;

public class NotificationHub : Hub<INotificationClient>
{
    public async Task SubscribeToResource(string resourceId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, resourceId);
    }
    
    public async Task UnsubscribeFromResource(string resourceId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, resourceId);
    }
}