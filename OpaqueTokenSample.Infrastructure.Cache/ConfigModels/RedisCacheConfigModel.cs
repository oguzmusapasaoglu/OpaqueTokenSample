namespace OpaqueTokenSample.Infrastructure.Cache.ConfigModels;

public sealed class RedisCacheConfigModel
{
    public bool Enabled { get; set; } = true;
    public string ConnectionString { get; set; } = string.Empty;
    public bool AbortOnConnectFail { get; set; } = false;
    public int ConnectRetry { get; set; } = 5;
    public int ConnectTimeout { get; set; } = 5000;
    public int KeepAlive { get; set; } = 180;
    public int DefaultTtlSeconds { get; set; } = 300;
}