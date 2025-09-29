using GabsHybridApp.Shared.Services;

namespace GabsHybridApp.Web.Services;

public sealed class WebFlashlightService : IFlashlightService
{
    public bool IsOn => false;

    public Task<bool> ToggleAsync()
        => Task.FromException<bool>(new NotSupportedException("Flashlight isn’t supported in web."));

    public void OpenSettingsAsync()
    {

    }
}