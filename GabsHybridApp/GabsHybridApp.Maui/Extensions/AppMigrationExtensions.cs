using Microsoft.EntityFrameworkCore;

namespace GabsHybridApp.Maui.Extensions;

public static class AppMigrationExtensions
{
    // MAUI: call on MauiApp
    public static MauiApp MigrateDb<TContext>(
        this MauiApp app,
        bool enableWal = true,
        Action<TContext>? seed = null)
        where TContext : DbContext
    {
        using var scope = app.Services.CreateScope();
        var sp = scope.ServiceProvider;

        // Works whether you registered AddDbContextFactory or AddDbContext
        var factory = sp.GetService<IDbContextFactory<TContext>>();
        if (factory is not null)
        {
            using var db = factory.CreateDbContext();
            Apply(db, enableWal, seed);
        }
        else
        {
            using var db = sp.GetRequiredService<TContext>();
            Apply(db, enableWal, seed);
        }

        return app;
    }
    
    private static void Apply<TContext>(TContext db, bool enableWal, Action<TContext>? seed)
        where TContext : DbContext
    {
        db.Database.Migrate();

        if (enableWal)
        {
            try { db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;"); }
            catch { /* non-SQLite provider — ignore */ }
        }

        seed?.Invoke(db);
    }
}
