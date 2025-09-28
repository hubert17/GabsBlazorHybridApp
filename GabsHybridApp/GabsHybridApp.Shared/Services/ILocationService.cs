namespace GabsHybridApp.Shared.Services;

public sealed record GeoPoint(
    double Latitude,
    double Longitude,
    double? AccuracyMeters = null,
    DateTimeOffset? Timestamp = null);

public interface ILocationService
{
    Task<GeoPoint?> GetCurrentAsync(CancellationToken ct = default);
}
