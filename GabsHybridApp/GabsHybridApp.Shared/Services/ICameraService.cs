namespace GabsHybridApp.Shared.Services;

public sealed record PhotoResult(
    string? Path,            // null on Web; absolute path on MAUI
    string? FileName,        // e.g., "photo_2025-10-05_144512.jpg"
    string ContentType,      // "image/jpeg", "image/png", etc.
    byte[] Bytes             // raw bytes (efficient; base64 computed on demand)
)
{
    /// <summary>
    /// Converts the photo bytes into a base64 data URL (for <img src> usage).
    /// </summary>
    public string ToDataUrl()
        => $"data:{ContentType};base64,{Convert.ToBase64String(Bytes)}";
}

public interface ICameraService
{
    /// <summary>
    /// Opens the device camera, saves or captures a photo, and returns the photo result.
    /// Returns null if user cancels.
    /// </summary>
    Task<PhotoResult?> CapturePhotoAsync(CancellationToken ct = default);

    /// <summary>
    /// Opens the gallery picker and returns the selected photo result.
    /// Returns null if user cancels.
    /// </summary>
    Task<PhotoResult?> BrowsePhotoAsync(CancellationToken ct = default);
}
