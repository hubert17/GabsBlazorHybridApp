using GabsHybridApp.Shared.Models;

namespace GabsHybridApp.Shared.Services;

public interface INotificationService
{
    Task PushNotificationAsync(Notification notification);
    Task<List<Notification>> GetUserNotificationsAsync(Guid excludeSenderUserId);
}
