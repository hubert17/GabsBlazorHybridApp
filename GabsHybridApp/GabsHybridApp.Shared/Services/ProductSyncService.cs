using GabsHybridApp.Shared.Data;
using GabsHybridApp.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace GabsHybridApp.Shared.Services;

public sealed class ProductSyncService
{
    private readonly IDbContextFactory<HybridAppDbContext> _dbFactory;
    private readonly HttpClient _http;
    private readonly IFormFactor _formFactor;

    public ProductSyncService(
        IDbContextFactory<HybridAppDbContext> dbFactory,
        HttpClient http,
        IFormFactor formFactor)
    {
        _dbFactory = dbFactory;
        _http = http;
        _formFactor = formFactor;
    }

    // Pull-all + AddRange; skip by name; swallow errors. No deletes, no versioning.
    public async Task<int> SyncAsync(string apiBaseUrl, CancellationToken ct = default)
    {
        // Only do network sync off the Web host (i.e., MAUI/WinUI)
        if (string.Equals(_formFactor.GetFormFactor(), "Web", StringComparison.OrdinalIgnoreCase))
            return 0;

        List<Product> list;
        try
        {
            list = await _http.GetFromJsonAsync<List<Product>>($"{apiBaseUrl}/api/sync/products/all", ct)
                   ?? new List<Product>();
        }
        catch
        {
            // network/deserialize failure → swallow
            return 0;
        }

        int applied = 0;

        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct);

            HashSet<string> existingNames;
            try
            {
                existingNames = new HashSet<string>(
                    await db.Products.AsNoTracking()
                        .Where(x => x.Name != null && x.Name != "")
                        .Select(x => x.Name!.Trim())
                        .ToListAsync(ct),
                    StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                // if reading existing names fails, proceed as if none exist
                existingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            var toInsert = new List<Product>();
            foreach (var p in list)
            {
                try
                {
                    var name = p.Name?.Trim();
                    if (string.IsNullOrWhiteSpace(name)) continue;
                    if (existingNames.Contains(name)) continue;

                    existingNames.Add(name); // also dedupe within the same batch

                    toInsert.Add(new Product
                    {
                        // If you want to keep server IDs, uncomment the next line:
                        // Id = p.Id,
                        Name = name,
                        Description = p.Description,
                        UnitPrice = p.UnitPrice,
                        Unit = p.Unit,
                        PictureFilename = p.PictureFilename
                    });
                }
                catch
                {
                    // swallow row-shaping error and move on
                }
            }

            if (toInsert.Count == 0) return 0;

            try
            {
                await db.Products.AddRangeAsync(toInsert, ct);
                applied = await db.SaveChangesAsync(ct);
            }
            catch
            {
                // If bulk insert fails, fall back to per-row insert and swallow errors
                foreach (var p in toInsert)
                {
                    try
                    {
                        await db.Products.AddAsync(p, ct);
                        applied += await db.SaveChangesAsync(ct);
                    }
                    catch
                    {
                        // swallow (likely PK/constraint). No update-on-duplicate here by request.
                    }
                }
            }
        }
        catch
        {
            // any DB factory/context failure → swallow
            return 0;
        }

        return applied;
    }

    // Query from the same context (MAUI uses local SQLite; Web uses its own DB if called there)
    public async Task<List<Product>> GetProductsAsync(string? search = null, CancellationToken ct = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct);
            var q = db.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLowerInvariant();
                q = q.Where(p => p.Name != null && p.Name.ToLower().Contains(s));
            }

            return await q.OrderBy(p => p.Id).ToListAsync(ct);
        }
        catch
        {
            // swallow and return empty on any failure
            return new List<Product>();
        }
    }
}
