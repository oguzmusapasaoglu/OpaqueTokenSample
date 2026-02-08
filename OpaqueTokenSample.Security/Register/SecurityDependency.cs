using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OpaqueTokenSample.Infrastructure.Cache.Abractions;
using OpaqueTokenSample.Infrastructure.Cache.Cache;
using OpaqueTokenSample.Infrastructure.Cache.Cache.Abractions;
using OpaqueTokenSample.Infrastructure.Cache.Services;

public static class SecurityDependency
{
    public static void SecurityDependencyRegister(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITokenValidationService, TokenValidationService>();
        services.AddScoped<ITokenGeneratorServices, TokenGeneratorServices>(); 
        services.AddScoped<ITokenSessionCache, TokenSessionCache>();
        services.AddSingleton(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var privateKeyPem = configuration["RsaKeys:PrivateKeyPem"];
            var publicKeyPem = configuration["RsaKeys:PublicKeyPem"];
            return new RsaKeyProvider(privateKeyPem, publicKeyPem);
        });
    }
}
