using global::GabsHybridApp.Shared.Data;
using global::GabsHybridApp.Shared.Models;
using global::GabsHybridApp.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GabsHybridApp.Web.Endpoints;

public static class AuthExchangeEndpoints
{
    public static WebApplication MapAuthExchangeEndpoints(this WebApplication app)
    {
        // GET /api/auth/serversalt?u={username}
        app.MapGet("/api/auth/serversalt", async (
            string u,
            IDbContextFactory<HybridAppDbContext> factory) =>
        {
            await using var db = await factory.CreateDbContextAsync();
            var user = await db.Set<UserAccount>()
                .FirstOrDefaultAsync(x => x.Username == u && x.IsActive);
            if (user is null) return Results.NotFound();

            if (string.IsNullOrWhiteSpace(user.ServerSalt))
            {
                user.ServerSalt = Guid.NewGuid().ToString("N");
                await db.SaveChangesAsync();
            }
            return Results.Text(user.ServerSalt!, "text/plain");
        }).AllowAnonymous();

        // POST /api/auth/exchange-hmac
        app.MapPost("/api/auth/exchange-hmac", async (
            ExchangeRequest req,
            IConfiguration cfg,
            IDbContextFactory<HybridAppDbContext> factory,
            INonceCache nonces,
            ApiJwtIssuer issuer) =>
        {
            if (string.IsNullOrWhiteSpace(req.Assertion)) return Results.BadRequest();

            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken token;
            try { token = handler.ReadJwtToken(req.Assertion); }
            catch { return Results.Unauthorized(); }

            var username = token.Claims.FirstOrDefault(c => c.Type is "username" or ClaimTypes.Name)?.Value;
            var deviceId = token.Claims.FirstOrDefault(c => c.Type == "device_id")?.Value;
            var nonce = token.Claims.FirstOrDefault(c => c.Type == "nonce")?.Value;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(deviceId) || string.IsNullOrWhiteSpace(nonce))
                return Results.Unauthorized();

            await using var db = await factory.CreateDbContextAsync();
            var user = await db.Set<UserAccount>()
                .FirstOrDefaultAsync(x => x.Username == username && x.IsActive);
            if (user is null || string.IsNullOrWhiteSpace(user.ServerSalt))
                return Results.Unauthorized();

            var master = cfg["AuthExchange:AppMasterSecret"]!;
            var derivedKey = AuthExchangeKeyDerivation.DeriveUserDeviceKey(master, username!, deviceId!, user.ServerSalt);

            var parms = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "GabsHybridApp.Maui",
                ValidateAudience = true,
                ValidAudience = "GabsHybridApp.Web.AuthExchange",
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Convert.ToBase64String(derivedKey))),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(10)
            };

            try
            {
                // Recreate the same key object without re-base64 (either way is fine as long as both sides match).
                parms.IssuerSigningKey = new SymmetricSecurityKey(derivedKey);
                handler.ValidateToken(req.Assertion, parms, out _);

                if (!await nonces.TryConsumeAsync(nonce!, TimeSpan.FromMinutes(2)))
                    return Results.Unauthorized();

                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, username!),
                    new("device_id", deviceId!),
                    new(ClaimTypes.Role, "MobileSync")
                };

                var (tokenStr, expUtc) = issuer.Issue(claims, TimeSpan.FromHours(8));
                return Results.Ok(new AuthResponse { AccessToken = tokenStr, ExpiresUtc = expUtc });
            }
            catch
            {
                return Results.Unauthorized();
            }
        }).AllowAnonymous();

        return app;
    }
}

// ---- Shared DTOs/Helpers used by endpoints ----
public sealed record ExchangeRequest(string Assertion);

public sealed class AuthResponse
{
    public string AccessToken { get; set; } = "";
    public DateTime ExpiresUtc { get; set; }
    public string TokenType { get; set; } = "Bearer";
}

public static class AuthExchangeKeyDerivation
{
    public static byte[] DeriveUserDeviceKey(string appMasterSecret, string username, string deviceId, string? serverSalt)
    {
        var material = $"{username}:{deviceId}:{(serverSalt ?? string.Empty)}";
        using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(appMasterSecret));
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(material));
    }
}
