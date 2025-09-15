// GabsHybridApp.Shared/DependencyInjection.cs
using BlazorState;
using GabsHybridApp.Shared.Services;
using GabsHybridApp.Shared.States;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using MudExtensions.Services;
using System.Reflection;

namespace GabsHybridApp.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedCore(this IServiceCollection services, Assembly? statesAssembly = null)
    {
        services.AddOptions();

        services.AddBlazorState(opts => opts.Assemblies = new[] { typeof(CounterState).GetTypeInfo().Assembly });

        // Auth state provider (used by both hosts)
        services.AddScoped<HostedAuthStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<HostedAuthStateProvider>());

        // App services that are host-agnostic
        services.AddScoped<UserService>();

        // UI libs
        services.AddMudServices();
        services.AddMudExtensions();

        return services;
    }
}
