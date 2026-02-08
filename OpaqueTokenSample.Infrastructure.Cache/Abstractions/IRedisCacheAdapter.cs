namespace OpaqueTokenSample.Infrastructure.Cache.Abstractions;

public interface IRedisCacheAdapter
{
    Task<List<T>?> GetDataAsync<T>(string key);
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, List<T> data, TimeSpan? ttl = null);
    Task SetAsync<T>(string key, T data, TimeSpan? ttl = null);
    Task<bool> DeleteAsync(string key);
}
