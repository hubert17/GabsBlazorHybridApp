﻿using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace GabsHybridApp.Shared.Services;

public interface IAuthService
{
    Task<bool> SignInAsync(string username, string password, string? returnUrl = null);
    Task SignOutAsync();
    Task<AuthenticationState> GetAuthenticationStateAsync();
    ClaimsPrincipal CurrentUser { get; }
}
