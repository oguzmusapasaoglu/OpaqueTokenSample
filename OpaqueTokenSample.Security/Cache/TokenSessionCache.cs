using OpaqueTokenSample.Infrastructure.Cache.Abstractions;
using OpaqueTokenSample.Infrastructure.Cache.Cache.Abractions;
using OpaqueTokenSample.Infrastructure.Cache.Models;

namespace OpaqueTokenSample.Infrastructure.Cache.Cache;

internal sealed class TokenSessionCache : ITokenSessionCache
{
    private static readonly TimeSpan AccessTokenTtl = TimeSpan.FromHours(1);
    private static readonly TimeSpan RefreshTokenTtl = TimeSpan.FromDays(7);
    private readonly IRedisCacheAdapter _cache;

    public TokenSessionCache(IRedisCacheAdapter cache)
    {
        _cache = cache;
    }

    // ---------------- SESSION ----------------
    public Task StoreSessionAsync(OpaqueSessionModel session, TimeSpan ttl = default)
    {
        if (ttl == default)
            ttl = RefreshTokenTtl;

        return _cache.SetAsync(
            $"session:{session.SessionId}",
            session,
            ttl);
    }

    public Task<OpaqueSessionModel?> GetSessionAsync(string sessionId)
    {
        return _cache.GetAsync<OpaqueSessionModel>(
            $"session:{sessionId}");
    }

    // ---------------- BINDINGS ----------------
    public Task BindAccessTokenAsync(string accessToken, string sessionId, TimeSpan ttl = default)
    {
        if (ttl == default)
            ttl = AccessTokenTtl;

        return _cache.SetAsync(
            $"access:{accessToken}",
            new TokenBindingModel { SessionId = sessionId },
            ttl);
    }

    public Task BindRefreshTokenAsync(string refreshToken, string sessionId, TimeSpan ttl = default)
    {
        if (ttl == default)
            ttl = RefreshTokenTtl;

        return _cache.SetAsync(
            $"refresh:{refreshToken}",
            new TokenBindingModel { SessionId = sessionId },
            ttl);
    }

    // ---------------- LOOKUPS ----------------
    public async Task<OpaqueSessionModel?> GetByAccessTokenAsync(string accessToken)
    {
        if (await IsAccessTokenBlacklistedAsync(accessToken))
            return null;

        var binding = await _cache.GetAsync<TokenBindingModel>(
            $"access:{accessToken}");

        if (binding is null || string.IsNullOrEmpty(binding.SessionId))
            return null;

        return await GetSessionAsync(binding.SessionId);
    }

    public async Task<OpaqueSessionModel?> GetByRefreshTokenAsync(string refreshToken)
    {
        if (await IsRefreshTokenBlacklistedAsync(refreshToken))
            return null;

        var binding = await _cache.GetAsync<TokenBindingModel>(
            $"refresh:{refreshToken}");

        if (binding is null || string.IsNullOrEmpty(binding.SessionId))
            return null;

        return await GetSessionAsync(binding.SessionId);
    }

    // ---------------- BLACKLIST ----------------
    public Task BlacklistAccessTokenAsync(string accessToken, TimeSpan ttl)
    {
        return _cache.SetAsync($"blacklist:access:{accessToken}", true, ttl);
    }

    public async Task<bool> IsAccessTokenBlacklistedAsync(string accessToken)
    {
        return await _cache.GetAsync<bool>($"blacklist:access:{accessToken}") == true;
    }

    public Task BlacklistRefreshTokenAsync(string refreshToken, TimeSpan ttl)
    {
        return _cache.SetAsync($"blacklist:refresh:{refreshToken}", true, ttl);
    }

    public async Task<bool> IsRefreshTokenBlacklistedAsync(string refreshToken)
    {
        return await _cache.GetAsync<bool>($"blacklist:refresh:{refreshToken}") == true;
    }

    // ---------------- REVOKE ----------------
    public Task RevokeSessionAsync(string sessionId)
    {
        // session silinirse access & refresh otomatik ölür
        return _cache.DeleteAsync($"session:{sessionId}");
    }
}
