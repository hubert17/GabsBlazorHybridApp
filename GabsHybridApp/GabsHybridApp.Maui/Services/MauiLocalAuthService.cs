using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Maui.Storage;
using GabsHybridApp.Shared.Services;

namespace GabsHybridApp.Maui.Services;

public sealed class MauiLocalAuthService(UserService users) : IAuthService
{
    // simple local “cookie” keys
    private const string SignedInKey = "auth_ok";
    private const string NameKey = "auth_name";
    private const string IdKey = "auth_id";
    private const string RolesKey = "auth_roles";

    private ClaimsPrincipal _user = new(new ClaimsIdentity()); // anonymous
    public ClaimsPrincipal CurrentUser => _user;

    public Task<bool> SignInAsync(string username, string password, string? returnUrl = null)
    {
        var u = users.Authenticate(username, password);
        if (u is null) return Task.FromResult(false);

        // persist minimal identity
        Preferences.Default.Set(SignedInKey, true);
        Preferences.Default.Set(NameKey, username);
        Preferences.Default.Set(IdKey, u.Id.ToString());
        Preferences.Default.Set(RolesKey, u.Roles ?? string.Empty);

        _user = BuildPrincipalFromPrefs();
        return Task.FromResult(true);
    }

    public Task SignOutAsync()
    {
        Preferences.Default.Remove(SignedInKey);
        Preferences.Default.Remove(NameKey);
        Preferences.Default.Remove(IdKey);
        Preferences.Default.Remove(RolesKey);
        _user = new ClaimsPrincipal(new ClaimsIdentity());
        return Task.CompletedTask;
    }

    public Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        _user = Preferences.Default.Get(SignedInKey, false)
            ? BuildPrincipalFromPrefs()
            : new ClaimsPrincipal(new ClaimsIdentity()); // anonymous

        return Task.FromResult(new AuthenticationState(_user));
    }

    private static ClaimsPrincipal BuildPrincipalFromPrefs()
    {
        var name = Preferences.Default.Get(NameKey, string.Empty);
        var id = Preferences.Default.Get(IdKey, string.Empty);
        var roles = Preferences.Default.Get(RolesKey, string.Empty);

        var claims = new List<Claim>();
        if (!string.IsNullOrEmpty(name)) claims.Add(new Claim(ClaimTypes.Name, name));
        if (!string.IsNullOrEmpty(id)) claims.Add(new Claim(ClaimTypes.NameIdentifier, id));

        foreach (var r in roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            claims.Add(new Claim(ClaimTypes.Role, r));

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "MauiLocal"));
    }
}
