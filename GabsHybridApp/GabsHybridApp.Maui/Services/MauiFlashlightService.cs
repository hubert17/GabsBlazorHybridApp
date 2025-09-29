﻿using GabsHybridApp.Shared.Services;
using Microsoft.Maui.ApplicationModel;

namespace GabsHybridApp.Maui.Services;

public sealed class MauiFlashlightService : IFlashlightService
{
    private volatile bool _isOn;
    public bool IsOn => _isOn;

    public async Task<bool> ToggleAsync()
    {
#if ANDROID
        await EnsureCameraPermissionAsync();

        var activity = Platform.CurrentActivity;
        var context = activity ?? Android.App.Application.Context;

        var camMgr = (Android.Hardware.Camera2.CameraManager)
                     context.GetSystemService(Android.Content.Context.CameraService)!;

        // cache the torch camera id so we don’t scan every time
        _cachedTorchId ??= FindTorchCameraId(camMgr)
            ?? throw new NotSupportedException("No camera with flash detected.");

        try
        {
            _isOn = !_isOn;
            camMgr.SetTorchMode(_cachedTorchId, _isOn);
            return _isOn;
        }
        catch (Android.Hardware.Camera2.CameraAccessException ex)
        {
            _isOn = false;
            throw new InvalidOperationException("Torch access failed.", ex);
        }
#else
        // iOS/macCatalyst (Essentials) — web/Windows will be handled by your platform stubs
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (_isOn) { await Flashlight.Default.TurnOffAsync(); _isOn = false; }
            else       { await Flashlight.Default.TurnOnAsync();  _isOn = true;  }
        });
        return _isOn;
#endif
    }

    public void OpenSettingsAsync() => AppInfo.ShowSettingsUI();

#if ANDROID
    private static string? _cachedTorchId;

    private static async Task EnsureCameraPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
                throw new UnauthorizedAccessException("Camera permission denied. Enable it in App Settings.");
        }
    }

    private static string? FindTorchCameraId(Android.Hardware.Camera2.CameraManager camMgr)
    {
        // Prefer a back-facing camera with flash
        foreach (var id in camMgr.GetCameraIdList())
        {
            var chars = camMgr.GetCameraCharacteristics(id);
            var hasFlash = (bool?)chars.Get(Android.Hardware.Camera2.CameraCharacteristics.FlashInfoAvailable) == true;
            var facing = (Java.Lang.Integer?)chars.Get(Android.Hardware.Camera2.CameraCharacteristics.LensFacing);
            if (hasFlash && facing != null &&
                facing.IntValue() == (int)Android.Hardware.Camera2.LensFacing.Back)
                return id;
        }

        // Fallback: any camera that reports a flash
        foreach (var id in camMgr.GetCameraIdList())
        {
            var chars = camMgr.GetCameraCharacteristics(id);
            var hasFlash = (bool?)chars.Get(Android.Hardware.Camera2.CameraCharacteristics.FlashInfoAvailable) == true;
            if (hasFlash) return id;
        }
        return null;
    }
#endif
}
