using Microsoft.Extensions.Caching.Memory;

using OpaqueTokenSample.Infrastructure.Cache.Abstractions;

namespace OpaqueTokenSample.Infrastructure.Cache.Services;

public class MemoryCacheService : IMemoryCacheService
{
    private readonly IMemoryCache _cache;

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public T Get<T>(string key)
        => _cache.TryGetValue(key, out T value) ? value : default;

    public void Set<T>(string key, T value, TimeSpan? ttl = null)
    {
        if (!ttl.HasValue)
        {
            var exprDate = DateTime.UtcNow.AddHours(24).Ticks;
            ttl = new TimeSpan(exprDate);
        }
        var options = ttl.HasValue
            ? new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            }
            : null;

        _cache.Set(key, value, options);
    }

    public bool Exists(string key)
        => _cache.TryGetValue(key, out _);

    public void Remove(string key)
        => _cache.Remove(key);
}
