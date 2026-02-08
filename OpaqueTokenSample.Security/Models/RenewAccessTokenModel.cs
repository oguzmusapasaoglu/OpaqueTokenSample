namespace OpaqueTokenSample.Infrastructure.Cache.Models;
public class RenewAccessTokenModel
{
    public string AccessToken { get; set; }
    public DateTime AccessTokenExpiresAt { get; set; }
    public string RefreshToken { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }
 }