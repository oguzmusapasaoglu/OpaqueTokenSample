using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using OpaqueTokenSample.Infrastructure.Cache.Abstractions;
using OpaqueTokenSample.Infrastructure.Cache.ConfigModels;

using StackExchange.Redis;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpaqueTokenSample.Infrastructure.Cache.Adapters;

public sealed class RedisCacheAdapter : IRedisCacheAdapter
{
    private readonly IDatabase _db;
    private readonly ILogger<RedisCacheAdapter> _logger;
    private readonly TimeSpan _defaultTtl;
    private readonly JsonSerializerOptions _jsonOptions;
    public RedisCacheAdapter(
        IConnectionMultiplexer multiplexer,
        IOptions<RedisCacheConfigModel> options,
        ILogger<RedisCacheAdapter> logger)
    {
        _db = multiplexer.GetDatabase();
        _logger = logger;

        _defaultTtl = TimeSpan.FromSeconds(options.Value.DefaultTtlSeconds);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }
    public async Task<List<T>?> GetDataAsync<T>(string key)
    {
        try
        {
            var value = await _db.StringGetAsync(key);
            if (!value.HasValue)
                return null;
            return JsonSerializer.Deserialize<List<T>>(value.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis GET failed: {Key}", key);
            return null;
        }
    }
    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var value = await _db.StringGetAsync(key);
            if (!value.HasValue)
                return default;
            return JsonSerializer.Deserialize<T>(value.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis GET failed: {Key}", key);
            return default;
        }
    }
    public async Task SetAsync<T>(string key, List<T> data, TimeSpan? ttl = null)
    {
        if (data == null || data.Count == 0)
            return;

        var effectiveTtl = ttl ?? _defaultTtl;
        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            await _db.StringSetAsync(key, json, effectiveTtl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis SET failed: {Key}, TTL: {TtlSeconds}s", key, (int)effectiveTtl.TotalSeconds);
            // deliberately swallow: cache must not break flow
        }
    }
    public async Task SetAsync<T>(string key, T data, TimeSpan? ttl = null)
    {
        if (data == null)
            return;

        var effectiveTtl = ttl ?? _defaultTtl;
        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            await _db.StringSetAsync(key, json, effectiveTtl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis SET failed: {Key}, TTL: {TtlSeconds}s", key, (int)effectiveTtl.TotalSeconds);
            // deliberately swallow: cache must not break flow
        }
    }
    public async Task<bool> DeleteAsync(string key)
    {
        try
        {
            return await _db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis DEL failed: {Key}", key);
            return false;
        }
    }
}