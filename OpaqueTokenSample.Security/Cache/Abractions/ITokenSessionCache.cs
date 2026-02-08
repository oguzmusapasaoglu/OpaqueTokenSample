using OpaqueTokenSample.Infrastructure.Cache.Models;

namespace OpaqueTokenSample.Infrastructure.Cache.Cache.Abractions;

public interface ITokenSessionCache
{
    Task StoreSessionAsync(OpaqueSessionModel session, TimeSpan ttl = default);

    Task<OpaqueSessionModel?> GetSessionAsync(string sessionId);

    Task BindAccessTokenAsync(string accessToken, string sessionId, TimeSpan ttl = default);

    Task BindRefreshTokenAsync(string refreshToken, string sessionId, TimeSpan ttl = default);

    Task<OpaqueSessionModel?> GetByAccessTokenAsync(string accessToken);
    Task<OpaqueSessionModel?> GetByRefreshTokenAsync(string refreshToken);

    Task BlacklistAccessTokenAsync(string accessToken, TimeSpan ttl);

    Task<bool> IsAccessTokenBlacklistedAsync(string accessToken);

    Task BlacklistRefreshTokenAsync(string refreshToken, TimeSpan ttl);

    Task<bool> IsRefreshTokenBlacklistedAsync(string refreshToken);

    Task RevokeSessionAsync(string sessionId);
}