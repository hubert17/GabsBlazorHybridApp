using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Maui.Storage;
using GabsHybridApp.Shared.Services;

namespace GabsHybridApp.Maui.Services;

// The service must be public and non-abstract
public sealed class MauiLocalAuthService : IAuthService
{
    // simple local “cookie” keys
    private const string SignedInKey = "auth_ok";
    private const string NameKey = "auth_name";
    private const string IdKey = "auth_id";
    private const string RolesKey = "auth_roles";

    // Start with an anonymous user, this is the safe initial state.
    private ClaimsPrincipal _user = new ClaimsPrincipal(new ClaimsIdentity());
    private readonly UserService _users;

    // Flag to ensure the disk read only happens once
    private bool _isStateLoaded = false;

    public ClaimsPrincipal CurrentUser => _user;

    // The constructor is now very fast and safe, it only handles DI and
    // sets the initial anonymous state.
    public MauiLocalAuthService(UserService users)
    {
        _users = users;
    }

    public Task<bool> SignInAsync(string username, string password, string? returnUrl = null)
    {
        var u = _users.Authenticate(username, password);
        if (u is null) return Task.FromResult(false);

        // persist minimal identity to disk
        Preferences.Default.Set(SignedInKey, true);
        Preferences.Default.Set(NameKey, username);
        Preferences.Default.Set(IdKey, u.Id.ToString());
        Preferences.Default.Set(RolesKey, u.Roles ?? string.Empty);

        // Update the in-memory cache instantly
        _user = BuildPrincipalFromPrefs();
        // Since we signed in, the state is definitely loaded.
        _isStateLoaded = true;
        return Task.FromResult(true);
    }

    public Task SignOutAsync()
    {
        // Remove persistence from disk
        Preferences.Default.Remove(SignedInKey);
        Preferences.Default.Remove(NameKey);
        Preferences.Default.Remove(IdKey);
        Preferences.Default.Remove(RolesKey);

        // Update the in-memory cache instantly
        _user = new ClaimsPrincipal(new ClaimsIdentity());
        _isStateLoaded = true;
        return Task.CompletedTask;
    }

    // ⚡️ FIX: This method performs the initial, potentially slow disk read only once.
    public Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!_isStateLoaded)
        {
            // First time loading: perform the synchronous disk read here.
            _user = Preferences.Default.Get(SignedInKey, false)
                ? BuildPrincipalFromPrefs()
                : new ClaimsPrincipal(new ClaimsIdentity()); // anonymous

            _isStateLoaded = true;
        }

        // Subsequent calls are fast, returning the cached state
        return Task.FromResult(new AuthenticationState(_user));
    }

    // Helper method to build the ClaimsPrincipal from storage
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