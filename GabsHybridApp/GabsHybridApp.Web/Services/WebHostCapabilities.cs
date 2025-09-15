using GabsHybridApp.Shared.Services;

namespace GabsHybridApp.Web.Services;

public sealed class WebHostCapabilities : IHostCapabilities
{
    public bool RequiresFullReloadAfterAuth => true;
}