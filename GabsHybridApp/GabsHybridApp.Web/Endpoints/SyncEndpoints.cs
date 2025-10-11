using GabsHybridApp.Shared.Data;
using GabsHybridApp.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GabsHybridApp.Web.Endpoints;

public static class SyncEndpoints
{
    public static WebApplication MapSyncEndpoints(this WebApplication app)
    {
        app.MapGet("/api/sync/products/all",
            [Authorize(Policy = "SyncAccess")] async (IDbContextFactory<HybridAppDbContext> factory) =>
        {
            await using var db = await factory.CreateDbContextAsync();
            var products = await db.Products.AsNoTracking().ToListAsync();
            return Results.Ok(products);
        }).DisableAntiforgery();

        return app;

    }
}
