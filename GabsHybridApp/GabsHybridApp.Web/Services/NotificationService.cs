using GabsHybridApp.Shared.Data;
using GabsHybridApp.Shared.Models;
using GabsHybridApp.Shared.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GabsHybridApp.Web.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly HybridAppDbContext _db;

    public NotificationService(IHubContext<NotificationHub> hubContext, HybridAppDbContext db)
    {
        _hubContext = hubContext;
        _db = db;
    }

    public async Task PushNotificationAsync(Notification notification)
    {
        notification.CreatedOn = DateTime.Now;
        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();

        // Push to all users
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(Guid excludeSenderUserId)
    {
        return await _db.Notifications
            .Where(n => n.UserId != excludeSenderUserId)
            .OrderByDescending(n => n.CreatedOn)
            .AsNoTracking()
            .ToListAsync();
    }
}
