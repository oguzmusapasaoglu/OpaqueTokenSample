using OpaqueTokenSample.Infrastructure.Cache.Helper;
using OpaqueTokenSample.Infrastructure.Cache.Models;

namespace OpaqueTokenSample.Infrastructure.Cache.Abractions;

public interface ITokenGeneratorServices
{
    Task<RenewAccessTokenModel> GenerateTokensForCompany(string userId, string userMail, string companyId);
    Task<ResponseBase<RenewAccessTokenModel>> RenewAccessTokenForCompany(string refreshToken);
}