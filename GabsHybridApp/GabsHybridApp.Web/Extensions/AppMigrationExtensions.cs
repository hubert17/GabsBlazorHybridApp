using Microsoft.EntityFrameworkCore;

namespace GabsHybridApp.Web.Extensions;

public static class AppMigrationExtensions
{
    public static WebApplication MigrateDb<TContext>(
        this WebApplication app,
        bool enableWal = false,
        Action<TContext>? seed = null)
        where TContext : DbContext
    {
        using var scope = app.Services.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<TContext>();
        Apply(db, enableWal, seed);
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
