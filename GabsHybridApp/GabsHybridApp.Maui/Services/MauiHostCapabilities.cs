using GabsHybridApp.Shared.Services;

namespace GabsHybridApp.Maui.Services;

public sealed class MauiHostCapabilities : IHostCapabilities
{
    public bool RequiresFullReloadAfterAuth => false;
}
