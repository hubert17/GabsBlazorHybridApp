using GabsHybridApp.Shared.Services;
using GabsHybridApp.Shared.States;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GabsHybridApp.Shared.Services;

public sealed class HmacAuthTokenProvider
{
    private readonly HttpClient _http;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private string? _token;
    private DateTimeOffset _expiresUtc;
    private string? _cacheKey; // baseUrl|username|deviceId

    public HmacAuthTokenProvider(HttpClient http) => _http = http;

    public void Clear()
    {
        _token = null;
        _expiresUtc = default;
        _cacheKey = null;
        _http.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<string?> EnsureAccessTokenAsync(
        string apiBaseUrl, string username, string deviceId, CancellationToken ct = default)
    {
        var baseUrl = apiBaseUrl.TrimEnd('/');
        var key = $"{baseUrl}|{username}|{deviceId}";

        // fast path
        if (_token is not null &&
            _cacheKey == key &&
            DateTimeOffset.UtcNow < _expiresUtc - TimeSpan.FromMinutes(1))
        {
            return _token;
        }

        await _gate.WaitAsync(ct);
        try
        {
            // re-check after entering the gate
            if (_token is not null &&
                _cacheKey == key &&
                DateTimeOffset.UtcNow < _expiresUtc - TimeSpan.FromMinutes(1))
            {
                return _token;
            }

            // 1) serversalt
            var saltUrl = $"{baseUrl}/api/auth/serversalt?u={Uri.EscapeDataString(username)}";
            Debug.WriteLine($"[AUTH] GET {saltUrl}");
            var saltResp = await _http.GetAsync(saltUrl, ct);
            if (!saltResp.IsSuccessStatusCode)
            {
                var body = await saltResp.Content.ReadAsStringAsync(ct);
                Debug.WriteLine($"[AUTH] serversalt failed: {(int)saltResp.StatusCode} {saltResp.ReasonPhrase}\n{body}");
                return null;
            }
            var serverSalt = (await saltResp.Content.ReadAsStringAsync(ct)).Trim();
            if (string.IsNullOrWhiteSpace(serverSalt)) return null;

            // 2) derive key
            var appMasterSecret = StorageConstants.AppMasterSecret; // TODO: secure storage
            var material = $"{username}:{deviceId}:{serverSalt}";
            byte[] derivedKey;
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(appMasterSecret)))
                derivedKey = hmac.ComputeHash(Encoding.UTF8.GetBytes(material));

            // 3) short-lived assertion
            var now = DateTime.UtcNow;
            var nonce = Guid.NewGuid().ToString("N");
            var claims = new[]
            {
                new Claim("username", username),
                new Claim("device_id", deviceId),
                new Claim("nonce", nonce)
            };
            var creds = new SigningCredentials(new SymmetricSecurityKey(derivedKey), SecurityAlgorithms.HmacSha256);
            var assertion = new JwtSecurityToken(
                issuer: "GabsHybridApp.Maui",
                audience: "GabsHybridApp.Web.AuthExchange",
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(1),
                signingCredentials: creds);
            var assertionStr = new JwtSecurityTokenHandler().WriteToken(assertion);

            // 4) exchange
            var exchUrl = $"{baseUrl}/api/auth/exchange-hmac";
            Debug.WriteLine($"[AUTH] POST {exchUrl}");
            var exchResp = await _http.PostAsJsonAsync(exchUrl, new { Assertion = assertionStr }, ct);
            if (!exchResp.IsSuccessStatusCode)
            {
                var body = await exchResp.Content.ReadAsStringAsync(ct);
                Debug.WriteLine($"[AUTH] exchange failed: {(int)exchResp.StatusCode} {exchResp.ReasonPhrase}\n{body}");
                return null;
            }

            var dto = await exchResp.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: ct);
            if (dto is null || string.IsNullOrWhiteSpace(dto.AccessToken)) return null;

            _token = dto.AccessToken;
            _expiresUtc = dto.ExpiresUtc; // if server sends local time, consider DateTimeOffset.Parse and assume UTC
            _cacheKey = key;

            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

            return _token;
        }
        finally
        {
            _gate.Release();
        }
    }

    private sealed class AuthResponse
    {
        public string AccessToken { get; set; } = "";
        public DateTimeOffset ExpiresUtc { get; set; }
        public string TokenType { get; set; } = "Bearer";
    }
}
