using GabsHybridApp.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace GabsHybridApp.Web.Extensions;

public sealed class WebDesignTimeFactory : IDesignTimeDbContextFactory<HybridAppDbContext>
{
    public HybridAppDbContext CreateDbContext(string[] args)
    {
        // Resolve configuration the same way the app would
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var basePath = Directory.GetCurrentDirectory(); // when PMC Default Project is the Web project

        var cfg = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: false)
            .AddUserSecrets(typeof(WebDesignTimeFactory).Assembly, optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connString = cfg.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection in appsettings.");

        var options = new DbContextOptionsBuilder<HybridAppDbContext>()
            .UseSqlServer(connString, sql =>
                sql.MigrationsAssembly(typeof(WebDesignTimeFactory).Assembly.GetName().Name))
            .Options;

        // If your DbContext ctor takes a contentRoot path, pass basePath here.
        return new HybridAppDbContext(options);
        // return new HybridAppDbContext(options, basePath); // <- if you have that overload
    }
}
