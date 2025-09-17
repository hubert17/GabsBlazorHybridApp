using GabsHybridApp.Maui.Extensions;
using GabsHybridApp.Maui.Services;
using GabsHybridApp.Shared;
using GabsHybridApp.Shared.Data;
using GabsHybridApp.Shared.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GabsHybridApp.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddSharedCore();

        builder.Services.AddPooledDbContextFactory<HybridAppDbContext>(opt =>
            opt.UseSqlite("Data Source=hybrid_mauiDb.db", FileSystem.AppDataDirectory));

        // Add device-specific services used by the GabsHybridApp.Shared project
        builder.Services.AddSingleton<IFormFactor, FormFactor>();
        builder.Services.AddScoped<IHostCapabilities, MauiHostCapabilities>();
        builder.UseStatusBarTheme("#607D8B", lightContent: true);

        builder.Services.AddMauiBlazorWebView();

        builder.Services.AddAuthorizationCore(); // not AddAuthorization()
        builder.Services.AddScoped<IAuthService, MauiLocalAuthService>();
        builder.Services.AddScoped<AuthenticationStateProvider, HostedAuthStateProvider>();
        builder.Services.AddScoped<INotificationService, NotificationService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // ✨ One-liner: migrate DB (and optionally seed) on startup; WAL enabled for mobile.
        app.MigrateDb<HybridAppDbContext>(enableWal: true);

        return app;
    }
}
