using GabsHybridApp.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GabsHybridApp.Web.Extensions;

public sealed class WebDesignTimeFactory : IDesignTimeDbContextFactory<HybridAppDbContext>
{
    public HybridAppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<HybridAppDbContext>()
            .UseSqlServer(
                "Server=.;Database=GabsHybridApp;Trusted_Connection=True;TrustServerCertificate=True;",
                sql => sql.MigrationsAssembly("GabsHybridApp.Web"))
            .Options;

        return new HybridAppDbContext(options);
    }
}