using BlazorState;
using GabsHybridApp.Shared.States;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using MudExtensions.Services;
using System.Reflection;

namespace GabsHybridApp.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedCore(this IServiceCollection services, Assembly? statesAssembly = null)
    {
        // UI libs used by both Web & Hybrid

        services.AddMudServices();
        services.AddMudExtensions();

        services.AddBlazorState(opts => opts.Assemblies = new[] { typeof(CounterState).GetTypeInfo().Assembly });

        return services;
    }
}