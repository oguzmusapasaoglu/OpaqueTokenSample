namespace OpaqueTokenSample.Infrastructure.Cache.Abstractions;

public interface IMemoryCacheService
{
    T Get<T>(string key);
    void Set<T>(string key, T value, TimeSpan? ttl = null);
    bool Exists(string key);
    void Remove(string key);
}