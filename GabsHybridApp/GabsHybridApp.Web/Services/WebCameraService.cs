using GabsHybridApp.Shared.Services;
using GabsHybridApp.Web.Components;
using MudBlazor;
using System.Text.RegularExpressions;

namespace GabsHybridApp.Web.Services;

public sealed class WebCameraService(IDialogService Dialog) : ICameraService
{
    public async Task<PhotoResult?> CapturePhotoAsync(CancellationToken ct = default)
    {
        var options = new DialogOptions
        {
            NoHeader = true,
            FullScreen = true,
            MaxWidth = MaxWidth.False,
            BackdropClick = false,
            CloseOnEscapeKey = true
        };

        var dlg = await Dialog.ShowAsync<Webcam>("", options);
        var res = await dlg.Result;

        if (res!.Canceled || res.Data is not string dataUrl || string.IsNullOrWhiteSpace(dataUrl))
            return null;

        // dataUrl: "data:image/jpeg;base64,...."
        var (mime, bytes) = ParseDataUrl(dataUrl);
        var fileName = $"photo_{DateTime.UtcNow:yyyyMMdd_HHmmss}.jpg";
        return new PhotoResult(Path: null, FileName: fileName, ContentType: mime, Bytes: bytes);
    }

    public Task<PhotoResult?> BrowsePhotoAsync(CancellationToken ct = default)
    {
        // TODO: implement InputFile picker conversion if needed
        return Task.FromResult<PhotoResult?>(null);
    }

    private static (string mime, byte[] bytes) ParseDataUrl(string dataUrl)
    {
        // Safe, small regex parser for "data:<mime>;base64,<b64>"
        var m = Regex.Match(dataUrl, @"^data:(?<mime>[^;]+);base64,(?<b64>.+)$", RegexOptions.Singleline);
        if (!m.Success)
            throw new InvalidOperationException("Invalid data URL format.");

        var mime = m.Groups["mime"].Value;
        var bytes = Convert.FromBase64String(m.Groups["b64"].Value);
        return (mime, bytes);
    }
}
