using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

using OpaqueTokenSample.Infrastructure.Cache.Abractions;
using OpaqueTokenSample.Infrastructure.Cache.Cache.Abractions;
using OpaqueTokenSample.Infrastructure.Cache.Models;

using System.IdentityModel.Tokens.Jwt;

namespace OpaqueTokenSample.Infrastructure.Cache.Services;

public sealed class TokenValidationService : ITokenValidationService
{
    private readonly ITokenSessionCache _sessionCache;
    private readonly ILogger<TokenValidationService> _logger;
    private readonly RsaKeyProvider _keys;
    private readonly JwtConfigModel _jwtConfig;

    private static readonly JwtSecurityTokenHandler Handler = new();

    public TokenValidationService(
        ITokenSessionCache sessionCache,
        ILogger<TokenValidationService> logger,
        RsaKeyProvider keyProvider, 
        IConfiguration configuration)
    {
        _sessionCache = sessionCache;
        _logger = logger;
        _keys = keyProvider;
        _jwtConfig = configuration.GetSection(JwtConfigModel.SectionName).Get<JwtConfigModel>(); 
    }

    /// <summary>
    /// OPAQUE-FIRST token validation
    /// 1. Redis session lookup
    /// 2. Session TTL check
    /// 3. JWT signature & issuer validation
    /// </summary>
    public async Task<string> ValidateToken(
        HttpRequest request,
        string tokenHeaderKey = "authorization")
    {
        var accessToken = GetCurrentToken(request, tokenHeaderKey);
        if (string.IsNullOrEmpty(accessToken))
            return "Data not found";

        // 🔐 OPAQUE FIRST — Redis authority
        var session = await _sessionCache.GetByAccessTokenAsync(accessToken);
        if (session is null)
            return "Invalid Token";

        // ⏱ Session expiration kontrolü
        var now = DateTime.UtcNow;
        if (session.AccessTokenExpiresAt <= now)
            return "Access Token has expired";

        // 🔏 JWT sadece kriptografik doğrulama
        return ValidateJwtToken(accessToken);
    }

    public string GetCurrentToken(
        HttpRequest request,
        string tokenHeaderKey = "authorization")
    {
        var header = request.Headers[tokenHeaderKey].ToString();

        if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return header.Substring(7).Trim();

        return string.Empty;
    }

    // ---------------- PRIVATE ----------------

    private string ValidateJwtToken(string token)
    {
        try
        {
            Handler.ValidateToken(token, GetValidationParams(), out _);
            return string.Empty;
        }
        catch (SecurityTokenExpiredException)
        {
            return "Access Token has expired";
        }
        catch (SecurityTokenException)
        {
            return "Token validation failed";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "JWT validation failed");
            return "Unexpected process error";
        }
    }

    private TokenValidationParameters GetValidationParams() =>
        new()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _keys.PublicKey,

            ValidateIssuer = true,
            ValidIssuer = _jwtConfig.Issuer,

            ValidateAudience = true,
            ValidAudience = _jwtConfig.Audience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
}
