namespace GabsHybridApp.Shared.Services;

public interface ICameraService
{
    /// <summary>
    /// Opens the device camera, saves the photo into app storage, and returns the final saved path.
    /// Returns null if user cancels.
    /// </summary>
    Task<string?> CapturePhotoAsync(CancellationToken ct = default);

    /// <summary>
    /// Opens the gallery picker and returns a copy saved into app storage (normalized location).
    /// Returns null if user cancels.
    /// </summary>
    Task<string?> BrowsePhotoAsync(CancellationToken ct = default);
}
