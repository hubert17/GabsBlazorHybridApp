namespace GabsHybridApp.Shared.Services;

public interface IFlashlightService
{
    /// Toggles the flashlight. Returns current state after toggle.
    Task<bool> ToggleAsync();

    /// Open the OS "App Settings" screen (no-op or NotSupported on platforms that can't).
    void OpenSettingsAsync();

    /// Best-effort cached state.
    bool IsOn { get; }
}
