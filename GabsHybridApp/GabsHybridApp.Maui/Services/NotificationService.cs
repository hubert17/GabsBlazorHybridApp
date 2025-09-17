using GabsHybridApp.Shared.Data;
using GabsHybridApp.Shared.Models;
using GabsHybridApp.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace GabsHybridApp.Maui.Services;

public class NotificationService : INotificationService
{
    private readonly HybridAppDbContext _db;

    public NotificationService(IDbContextFactory<HybridAppDbContext> DbFactory)
    {
        _db = DbFactory.CreateDbContext();
    }

    public async Task PushNotificationAsync(Notification notification)
    {
        notification.CreatedOn = DateTime.Now;
        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();
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
