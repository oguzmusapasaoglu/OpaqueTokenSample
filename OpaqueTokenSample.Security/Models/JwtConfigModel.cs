namespace OpaqueTokenSample.Infrastructure.Cache.Models;
public class JwtConfigModel
{
    public const string SectionName = "JwtConfig";
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int AccessTokenExpirationHour { get; set; }
    public int RefreshTokenExpirationDay { get; set; }
}