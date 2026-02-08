using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using StackExchange.Redis;
using OpaqueTokenSample.Infrastructure.Cache.Abstractions;
using OpaqueTokenSample.Infrastructure.Cache.Adapters;
using OpaqueTokenSample.Infrastructure.Cache.Services;
using OpaqueTokenSample.Infrastructure.Cache.ConfigModels;

namespace OpaqueTokenSample.Infrastructure.Cache.Register;

public static class CacheDependency
{
    public static void CacheDependencyRegister(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ---------- Memory Cache ----------
        var memOpts = new MemoryCacheConfigModel();
        configuration.GetSection("Cache:Memory").Bind(memOpts);

        if (memOpts.Enabled)
        {
            services.AddMemoryCache();
            services.AddScoped<IMemoryCacheService, MemoryCacheService>();
        }

        // ---------- Redis ----------
        var redisOpts = new RedisCacheConfigModel();
        configuration.GetSection("Cache:Redis").Bind(redisOpts);

        if (redisOpts.Enabled)
        {
            if (string.IsNullOrWhiteSpace(redisOpts.ConnectionString))
                throw new InvalidOperationException("Redis connection string is missing.");

            var redisConfig = new ConfigurationOptions
            {
                AbortOnConnectFail = redisOpts.AbortOnConnectFail,
                ConnectRetry = redisOpts.ConnectRetry,
                ConnectTimeout = redisOpts.ConnectTimeout,
                KeepAlive = redisOpts.KeepAlive
            };

            redisConfig.EndPoints.Add(redisOpts.ConnectionString);

            services.AddSingleton<IConnectionMultiplexer>(
                _ => ConnectionMultiplexer.Connect(redisConfig));

            services.AddScoped<IRedisCacheAdapter, RedisCacheAdapter>();
        }
    }
}