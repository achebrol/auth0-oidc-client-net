using System.IdentityModel.Tokens.Jwt;

namespace Ping.OidcClient.Tokens
{
    internal interface ISignatureVerifier
    {
        JwtSecurityToken VerifySignature(string token);
    }
}
