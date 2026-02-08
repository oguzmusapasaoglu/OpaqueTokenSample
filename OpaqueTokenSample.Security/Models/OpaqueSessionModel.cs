namespace OpaqueTokenSample.Infrastructure.Cache.Models;

public sealed class OpaqueSessionModel
{
    public string SessionId { get; init; }           // opaque token (random / uuid / base64)
    public string UserId { get; init; }
    public string? CompanyId { get; init; } = null;
    public string UserEmail { get; init; }

    public DateTime AccessTokenExpiresAt { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime LastAccessAt { get; set; }

    public bool IsRevoked { get; set; }
}
