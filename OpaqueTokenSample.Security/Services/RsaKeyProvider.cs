using System.Security.Cryptography;

using Microsoft.IdentityModel.Tokens;
namespace OpaqueTokenSample.Infrastructure.Cache.Services;

// RsaKeyProvider – PEM formatlı anahtar çiftini tutar
public class RsaKeyProvider
{
    public RsaSecurityKey PrivateKey { get; }
    public RsaSecurityKey PublicKey { get; }

    public RsaKeyProvider(string privateKeyPem, string publicKeyPem)
    {
        var rsaPriv = RSA.Create();
        rsaPriv.ImportFromPem(privateKeyPem.ToCharArray());
        PrivateKey = new RsaSecurityKey(rsaPriv);

        var rsaPub = RSA.Create();
        rsaPub.ImportFromPem(publicKeyPem.ToCharArray());
        PublicKey = new RsaSecurityKey(rsaPub);
    }
}
