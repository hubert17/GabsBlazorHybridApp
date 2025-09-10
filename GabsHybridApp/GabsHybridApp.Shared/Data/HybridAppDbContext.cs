using GabsHybridApp.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace GabsHybridApp.Shared.Data;

public class HybridAppDbContext : DbContext
{
    public HybridAppDbContext(DbContextOptions<HybridAppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Product config
        modelBuilder.Entity<Product>(e =>
        {
            // Seed data (uses your static SeedData() list)
            e.HasData(Product.SeedData());
        });
    }

    public DbSet<Product> Products { get; set; }

}