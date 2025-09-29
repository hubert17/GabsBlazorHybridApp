using GabsHybridApp.Shared.Services;

namespace GabsHybridApp.Maui.Services;

public sealed class MauiCameraService : ICameraService
{
    private const string PhotosFolderName = "Photos";

    public async Task<string?> CapturePhotoAsync(CancellationToken ct = default)
    {
        // 1) Camera permission at runtime (Android)
        var cam = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (cam != PermissionStatus.Granted)
            cam = await Permissions.RequestAsync<Permissions.Camera>();
        if (cam != PermissionStatus.Granted)
            return null; // user denied

        // 2) Device supports capture? (emulators without a camera will return false)
        if (!MediaPicker.Default.IsCaptureSupported)
            return null;

        try
        {
            var file = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
            {
                // keep it simple; no extension in Title (some OEMs are picky)
                Title = $"photo_{DateTime.UtcNow:yyyyMMdd_HHmmss}"
            });

            if (file == null) return null;

            return await SaveToAppPhotosAsync(file, ct);
        }
        catch (FeatureNotSupportedException)
        {
            // Camera not available on device
            return null;
        }
        catch (PermissionException)
        {
            return null;
        }
    }

    public async Task<string?> BrowsePhotoAsync(CancellationToken ct = default)
    {
        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            if (DeviceInfo.Version.Major >= 13)
                _ = await Permissions.RequestAsync<Permissions.Photos>();
            else
                _ = await Permissions.RequestAsync<Permissions.StorageRead>();
        }

        var file = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
        {
            Title = "Select a photo"
        });

        if (file == null) return null;

        // Copy into app storage for a consistent, reliable path
        var savedPath = await SaveToAppPhotosAsync(file, ct);
        return savedPath;
    }

    private static async Task<string> SaveToAppPhotosAsync(FileResult file, CancellationToken ct)
    {
        // Ensure folder
        var photosDir = Path.Combine(FileSystem.AppDataDirectory, PhotosFolderName);
        Directory.CreateDirectory(photosDir);

        // Generate final filename (preserve extension if present)
        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";
        var finalName = $"photo_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}{ext}";
        var destination = Path.Combine(photosDir, finalName);

        // Copy stream safely
        using var src = await file.OpenReadAsync();
        using var dst = File.OpenWrite(destination);
        await src.CopyToAsync(dst, ct);

        return destination;
    }
}
