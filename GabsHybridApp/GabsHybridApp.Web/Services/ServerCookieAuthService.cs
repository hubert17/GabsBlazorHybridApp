using GabsHybridApp.Shared.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace GabsHybridApp.Web.Services;

public class ServerCookieAuthService(IHttpContextAccessor http, UserService users) : IAuthService
{
    private ClaimsPrincipal _user = new(new ClaimsIdentity());

    public ClaimsPrincipal CurrentUser => _user;

    public async Task<bool> SignInAsync(string username, string password, string? returnUrl = null)
    {
        var u = users.Authenticate(username, password);
        if (u is null) return false;

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new(ClaimTypes.NameIdentifier, u.Id.ToString())
        };
        foreach (var r in (u.Roles ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries))
            claims.Add(new(ClaimTypes.Role, r));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        _user = new ClaimsPrincipal(identity);

        await http.HttpContext!.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            _user
        );
        return true;
    }

    public async Task SignOutAsync()
    {
        _user = new ClaimsPrincipal(new ClaimsIdentity());
        await http.HttpContext!.SignOutAsync();
    }

    public Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var ctxUser = http.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
        _user = ctxUser.Identity?.IsAuthenticated == true ? ctxUser : _user;
        return Task.FromResult(new AuthenticationState(_user));
    }
}