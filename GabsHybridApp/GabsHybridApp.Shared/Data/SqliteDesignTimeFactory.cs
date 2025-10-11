using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GabsHybridApp.Shared.Data;

public sealed class SqliteDesignTimeFactory : IDesignTimeDbContextFactory<HybridAppDbContext>
{
    public HybridAppDbContext CreateDbContext(string[] args)
    {
        // When running EF tools, CWD is typically the project folder of --project / Default project
        var basePath = Directory.GetCurrentDirectory();
        var dbPath = Path.Combine(basePath, "Data", "design.db");

        var options = new DbContextOptionsBuilder<HybridAppDbContext>()
            .UseSqlite($"Data Source={dbPath}",
                sqlite => sqlite.MigrationsAssembly(typeof(SqliteDesignTimeFactory).Assembly.GetName().Name))
            .Options;

        return new HybridAppDbContext(options);
    }
}
