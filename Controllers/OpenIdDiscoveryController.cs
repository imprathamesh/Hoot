
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Hoot.Controllers;

[AllowAnonymous]
[ApiController]
[Route(".well-known")]
public class OpenIdDiscoveryController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public OpenIdDiscoveryController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("openid-configuration")]
    public IActionResult GetDiscoveryDocument()
    {
        var authorityUrl = "https://localhost:7098"; //_configuration["AuthServer:Authority"]; // Example: https://localhost:7098

        var discoveryData = new Dictionary<string, object>
    {
        { "issuer", authorityUrl },
        { "authorization_endpoint", $"{authorityUrl}/connect/authorize" },
        { "token_endpoint", $"{authorityUrl}/connect/token" },
        { "userinfo_endpoint", $"{authorityUrl}/connect/userinfo" },
        { "jwks_uri", $"{authorityUrl}/.well-known/jwks" },
        { "response_types_supported", new[] { "code", "token", "id_token" } },
        { "grant_types_supported", new[] { "authorization_code", "refresh_token", "client_credentials" } },
        { "subject_types_supported", new[] { "public" } },
        { "id_token_signing_alg_values_supported", new[] { "RS256" } }
    };

        return Ok(discoveryData);
    }
    [HttpGet("jwks")]
    public IActionResult GetJwks()
    {
        var rsaKey = RSA.Create(2048);
        var key = new RsaSecurityKey(rsaKey)
        {
            KeyId = "907A6D3546002CE8744DC03DC455A556"
        };

        var parameters = rsaKey.ExportParameters(false);

        var jwk = new JsonWebKey
        {
            Kty = "RSA",
            Use = "sig",
            Kid = key.KeyId,
            E = Base64UrlEncoder.Encode(parameters.Exponent),
            N = Base64UrlEncoder.Encode(parameters.Modulus),
            Alg = "RS256"
        };

        var jwks = new
        {
            keys = new List<JsonWebKey> { jwk }
        };

        return Ok(jwks);
    }
}
