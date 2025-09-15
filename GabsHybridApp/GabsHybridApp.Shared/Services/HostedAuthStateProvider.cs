using Microsoft.AspNetCore.Components.Authorization;

namespace GabsHybridApp.Shared.Services;

public class HostedAuthStateProvider(IAuthService auth) : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => auth.GetAuthenticationStateAsync();

    public void Notify() => NotifyAuthenticationStateChanged(auth.GetAuthenticationStateAsync());
}