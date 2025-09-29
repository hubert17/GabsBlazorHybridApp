using GabsHybridApp.Shared.Services;

namespace GabsHybridApp.Web.Services;

public sealed class WebCameraService : ICameraService
{
    public Task<string?> BrowsePhotoAsync(CancellationToken ct = default)
    {
        // throw new NotImplementedException();
        return Task.FromResult<string?>(string.Empty);
    }

    public Task<string?> CapturePhotoAsync(CancellationToken ct = default)
    {
        // throw new NotImplementedException();
        return Task.FromResult<string?>(string.Empty);
    }
}
