using Microsoft.EntityFrameworkCore;

namespace GabsHybridApp.Web.Extensions;

public static class SqliteAppMigrationExtensions
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
        // Skip everything unless the provider is SQLite
        if (!db.Database.IsSqlite())
            return;

        // Apply EF Core migrations
        db.Database.Migrate();

        // Optional: enable WAL only for SQLite
        if (enableWal)
        {
            db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
        }

        // Optional: run seed only for SQLite
        seed?.Invoke(db);
    }
}
