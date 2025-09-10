using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GabsHybridApp.Shared.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<HybridAppDbContext>
{
    public HybridAppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<HybridAppDbContext>()
            .UseSqlite("Data Source=dev.db") // design-time only
            .Options;

        return new HybridAppDbContext(options);
    }
}
