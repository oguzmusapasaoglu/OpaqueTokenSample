using Microsoft.AspNetCore.Http;

namespace OpaqueTokenSample.Infrastructure.Cache.Abractions;

public interface ITokenValidationService
{
    Task<string> ValidateToken(HttpRequest request, string tokenHeaderKey = "authorization");
    string GetCurrentToken(HttpRequest request, string tokenHeaderKey = "authorization");
}