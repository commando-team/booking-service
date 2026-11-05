namespace Notification.Hubs;

public interface INotificationClient
{
    Task OnBookingCreatedEvent(string @event);
}