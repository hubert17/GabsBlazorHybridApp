using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace GabsHybridApp.Web.Services;

// --- DTOs ---
public sealed record ExchangeRequest(string Assertion);

public sealed class AuthResponse
{
    public string AccessToken { get; set; } = "";
    public DateTime ExpiresUtc { get; set; }
    public string TokenType { get; set; } = "Bearer";
}

// --- Key derivation (MAUI & Web MUST match) ---
public static class AuthExchangeKeyDerivation
{
    public static byte[] DeriveUserDeviceKey(string appMasterSecret, string username, string deviceId, string? serverSalt)
    {
        var material = $"{username}:{deviceId}:{(serverSalt ?? string.Empty)}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(appMasterSecret));
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(material));
    }
}

// --- Nonce anti-replay cache ---
public interface INonceCache
{
    Task<bool> TryConsumeAsync(string nonce, TimeSpan ttl);
}

public sealed class MemoryNonceCache : INonceCache
{
    private readonly IMemoryCache _cache;
    public MemoryNonceCache(IMemoryCache cache) => _cache = cache;

    public Task<bool> TryConsumeAsync(string nonce, TimeSpan ttl)
    {
        if (string.IsNullOrWhiteSpace(nonce)) return Task.FromResult(false);
        var key = "nonce:" + nonce;
        if (_cache.TryGetValue(key, out _)) return Task.FromResult(false);
        _cache.Set(key, true, ttl);
        return Task.FromResult(true);
    }
}

// --- API JWT issuer for /api/sync/* ---
public sealed class ApiJwtIssuer
{
    private readonly SymmetricSecurityKey _key;
    private readonly string _issuer;
    private readonly string _audience;

    public ApiJwtIssuer(IConfiguration cfg)
    {
        _issuer = cfg["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer missing");
        _audience = cfg["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience missing");
        var key = cfg["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key missing");
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    }

    public (string Token, DateTime ExpiresUtc) Issue(IEnumerable<Claim> claims, TimeSpan life)
    {
        var now = DateTime.UtcNow;
        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(_issuer, _audience, claims, now, now.Add(life), creds);
        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        return (token, jwt.ValidTo);
    }
}
