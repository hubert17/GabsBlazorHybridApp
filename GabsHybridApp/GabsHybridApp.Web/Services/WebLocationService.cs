using System.Text.Json;
using GabsHybridApp.Shared.Services;
using Microsoft.JSInterop;

namespace GabsHybridApp.Web.Services;

public sealed class WebLocationService : ILocationService
{
    private readonly IJSRuntime _js;
    public WebLocationService(IJSRuntime js) => _js = js;

    public async Task<GeoPoint?> GetCurrentAsync(CancellationToken ct = default)
    {
        var result = await _js.InvokeAsync<JsonElement?>("gabsGeo.getCurrent");
        if (result is null || result?.ValueKind != JsonValueKind.Object)
            return null;

        var obj = result.Value;
        if (!obj.TryGetProperty("latitude", out var latP) ||
            !obj.TryGetProperty("longitude", out var lonP))
            return null;

        double lat = latP.GetDouble();
        double lon = lonP.GetDouble();
        double? acc = obj.TryGetProperty("accuracy", out var aP) ? aP.GetDouble() : null;
        DateTimeOffset? ts = obj.TryGetProperty("timestamp", out var tP)
            ? DateTimeOffset.FromUnixTimeMilliseconds(tP.GetInt64())
            : null;

        return new GeoPoint(lat, lon, acc, ts);
    }
}
