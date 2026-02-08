using Microsoft.Extensions.Logging;

using OpaqueTokenSample.Infrastructure.Cache.Abstractions;

namespace OpaqueTokenSample.Infrastructure.Cache.Repositories;

public abstract class RedisCacheRepository<TModel> where TModel : class
{
    protected readonly IRedisCacheAdapter cache;
    private readonly ILogger<RedisCacheRepository<TModel>> logger;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    protected RedisCacheRepository(IRedisCacheAdapter _cache)
    {
        cache = _cache;
    }
    public abstract string CacheKey { get; }
    public async Task<IEnumerable<TModel>> GetAllDataAsync(CancellationToken ct = default)
    {
        var list = await GetOrFillCacheAsync(ct).ConfigureAwait(false);
        return list ?? Enumerable.Empty<TModel>();
    }
    public async Task<IEnumerable<TModel>> GetDataByFilterAsync(Func<TModel, bool> predicate, CancellationToken ct = default)
    {
        var list = await GetOrFillCacheAsync(ct).ConfigureAwait(false);
        return list?.Where(predicate) ?? Enumerable.Empty<TModel>();
    }
    public async Task ReFillCacheAsync(CancellationToken ct = default)
    {
        await FillAndSetCacheAsync(ct).ConfigureAwait(false);
    }
    protected abstract Task<List<TModel>> FillDataAsync(CancellationToken ct = default);
    private async Task<List<TModel>> GetOrFillCacheAsync(CancellationToken ct)
    {
        try
        {
            var result = await cache.GetDataAsync<TModel>(CacheKey).ConfigureAwait(false);
            if (result == null || !result.Any())
            {
                // Stampede prevention: only one caller fills cache
                await _semaphore.WaitAsync(ct).ConfigureAwait(false);
                try
                {
                    // double-check after acquiring lock
                    result = await cache.GetDataAsync<TModel>(CacheKey).ConfigureAwait(false);
                    if (result == null || !result.Any())
                    {
                        result = await FillDataAsync(ct).ConfigureAwait(false) ?? new List<TModel>();
                        // Set cache (assume cache has SetCachedDataAsync)
                        await cache.SetAsync(CacheKey, result).ConfigureAwait(false);
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            return result;
        }
        catch (Exception ex)
        {
            // log but don't throw to keep caller resilient
            logger.LogError(ex, "Redis GET failed. CacheKey={CacheKey}, Repository={Repository}", CacheKey, GetType().Name);
            return null;
        }
    }
    private async Task FillAndSetCacheAsync(CancellationToken ct)
    {
        List<TModel>? data;

        try
        {
            data = await FillDataAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Cache source fill failed. CacheKey={CacheKey}, Repository={Repository}", CacheKey, GetType().Name);
            return;
        }

        if (data == null || data.Count == 0)
        {
            logger.LogDebug("Cache fill skipped (empty). CacheKey={CacheKey}, Repository={Repository}", CacheKey, GetType().Name);
            return;
        }
        await cache.SetAsync(CacheKey, data).ConfigureAwait(false);
    }
}