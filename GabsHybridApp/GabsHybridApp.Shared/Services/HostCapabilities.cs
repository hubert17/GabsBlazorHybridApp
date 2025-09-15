using Microsoft.AspNetCore.Components;

namespace GabsHybridApp.Shared.Services;

public interface IHostCapabilities
{
    bool RequiresFullReloadAfterAuth { get; }   // Web: true, MAUI: false
}

public static class HostCapabilitiesExtensions
{
    public static void NavigateToAfterAuth(this NavigationManager nav, string? target, bool requiresFullReloadAfterAuth)
    {
        target = NormalizeTarget(target);

        if (requiresFullReloadAfterAuth)
        {
            // Web: full GET so the cookie is seen; also replace history
            nav.NavigateTo(target, new NavigationOptions
            {
                ForceLoad = true,
                ReplaceHistoryEntry = true
            });
        }
        else
        {
            // MAUI: no full reload; replace to avoid back to login
            nav.NavigateTo(target, replace: true);
        }
    }

    // Handy for “redirect to login with ReturnUrl”
    public static void NavigateToLogin(this NavigationManager nav, bool requiresFullReloadAfterAuth)
    {
        var rel = "/" + nav.ToBaseRelativePath(nav.Uri);
        if (string.IsNullOrWhiteSpace(rel) || rel == "/")
            rel = "/";

        var target = $"/account/login?ReturnUrl={Uri.EscapeDataString(rel)}";
        nav.NavigateToAfterAuth(target, requiresFullReloadAfterAuth);
    }

    private static string NormalizeTarget(string? target)
    {
        if (string.IsNullOrWhiteSpace(target)) return "/";
        return target!.StartsWith("/") ? target : "/" + target;
    }
}