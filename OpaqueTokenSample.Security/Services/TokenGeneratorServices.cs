using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

using OpaqueTokenSample.Infrastructure.Cache.Abractions;
using OpaqueTokenSample.Infrastructure.Cache.Cache.Abractions;
using OpaqueTokenSample.Infrastructure.Cache.Helper;
using OpaqueTokenSample.Infrastructure.Cache.Models;
using OpaqueTokenSample.Security.Helper;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OpaqueTokenSample.Infrastructure.Cache.Services;

public sealed class TokenGeneratorServices : ITokenGeneratorServices
{
    private readonly ITokenSessionCache _sessionCache;
    private readonly RsaKeyProvider _rsaKeys;
    private readonly JwtConfigModel _jwtConfig;
    private readonly ILogger<TokenGeneratorServices> _logger;

    private static readonly JwtSecurityTokenHandler Handler = new();

    public TokenGeneratorServices(
        ITokenSessionCache sessionCache,
        ITokenValidationService validationService,
        RsaKeyProvider rsaKeys,
        ILogger<TokenGeneratorServices> logger,
        IConfiguration configuration)
    {
        _sessionCache = sessionCache;
        _rsaKeys = rsaKeys;
        _logger = logger;
        _jwtConfig = configuration.GetSection(JwtConfigModel.SectionName).Get<JwtConfigModel>();
    }

    // ------------------------------------------------------------------
    // TOKEN GENERATE
    // ------------------------------------------------------------------
    public async Task<RenewAccessTokenModel> GenerateTokensForCompany(
        string userId,
        string userMail,
        string companyId)
    {
        var now = DateTime.UtcNow;

        var accessExp = now.AddHours(_jwtConfig.AccessTokenExpirationHour);
        var refreshExp = now.AddDays(_jwtConfig.RefreshTokenExpirationDay);

        var session = new OpaqueSessionModel
        {
            SessionId = Guid.NewGuid().ToString("N"),
            UserId = userId,
            UserEmail = userMail,
            CompanyId = companyId,
            CreatedAt = now,
            AccessTokenExpiresAt = accessExp,
            RefreshTokenExpiresAt = refreshExp
        };

        var accessToken = CreateJwt("access", accessExp);
        var refreshToken = CreateJwt("refresh", refreshExp);

        // 🔐 STORE SESSION
        await _sessionCache.StoreSessionAsync(session);

        // 🔗 BIND TOKENS
        await _sessionCache.BindAccessTokenAsync(accessToken, session.SessionId);
        await _sessionCache.BindRefreshTokenAsync(refreshToken, session.SessionId);

        return new RenewAccessTokenModel
        {
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessExp,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshExp
        };
    }

    // ------------------------------------------------------------------
    // TOKEN RENEW
    // ------------------------------------------------------------------
    public async Task<ResponseBase<RenewAccessTokenModel>> RenewAccessTokenForCompany(
        string refreshToken)
    {
        try
        {
            // 🔐 Refresh → Session
            var session = await _sessionCache.GetByRefreshTokenAsync(refreshToken);
            if (session is null)
                return ResponseHelper.ErrorResponse<RenewAccessTokenModel>(
                    "Invalid Token");

            var now = DateTime.UtcNow;

            if (session.RefreshTokenExpiresAt <= now)
                return ResponseHelper.ErrorResponse<RenewAccessTokenModel>(
                    "Refresh Token has expired");

            // ⛔ Blacklist old refresh
            await _sessionCache.BlacklistRefreshTokenAsync(
                refreshToken,
                session.RefreshTokenExpiresAt - now);

            // 🔄 SESSION ROTATE
            var newAccessExp = now.AddHours(_jwtConfig.AccessTokenExpirationHour);
            var newRefreshExp = now.AddDays(_jwtConfig.RefreshTokenExpirationDay);

            session.AccessTokenExpiresAt = newAccessExp;
            session.RefreshTokenExpiresAt = newRefreshExp;

            var newAccessToken = CreateJwt("access", newAccessExp);
            var newRefreshToken = CreateJwt("refresh", newRefreshExp);

            await _sessionCache.StoreSessionAsync(session);
            await _sessionCache.BindAccessTokenAsync(newAccessToken, session.SessionId);
            await _sessionCache.BindRefreshTokenAsync(newRefreshToken, session.SessionId);

            return ResponseHelper.SuccessResponse(new RenewAccessTokenModel
            {
                AccessToken = newAccessToken,
                AccessTokenExpiresAt = newAccessExp,
                RefreshToken = newRefreshToken,
                RefreshTokenExpiresAt = newRefreshExp
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RenewAccessTokenForCompany failed");
            return ResponseHelper.ErrorResponse<RenewAccessTokenModel>(
                "Unexpected process error");
        }
    }

    // ------------------------------------------------------------------
    // JWT (OPAQUE CARRIER)
    // ------------------------------------------------------------------
    private string CreateJwt(string type, DateTime expiresAt)
    {
        var claims = new[]
        {
            new Claim("typ", type),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        var credentials = new SigningCredentials(
            _rsaKeys.PrivateKey,
            SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtConfig.Issuer,
            audience: _jwtConfig.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return Handler.WriteToken(token);
    }
}