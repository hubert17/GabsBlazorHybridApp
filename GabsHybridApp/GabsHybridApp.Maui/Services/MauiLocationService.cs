using GabsHybridApp.Shared.Services;

namespace GabsHybridApp.Maui.Services;

public sealed class MauiLocationService : ILocationService
{
    public async Task<GeoPoint?> GetCurrentAsync(CancellationToken ct = default)
    {
        // Ensure permission
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                return null;
        }

        // Fast attempt, then fall back to more accurate
        var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
        Location? loc;
        try
        {
            loc = await Geolocation.GetLocationAsync(request, ct);
            loc ??= await Geolocation.GetLastKnownLocationAsync();
        }
        catch
        {
            loc = await Geolocation.GetLastKnownLocationAsync();
        }

        return loc is null
            ? null
            : new GeoPoint(
                loc.Latitude,
                loc.Longitude,
                loc.Accuracy,
                loc.Timestamp == default ? null : loc.Timestamp);
    }
}
