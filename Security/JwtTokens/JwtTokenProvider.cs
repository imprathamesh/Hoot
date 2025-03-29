using Hoot.Data;
using Hoot.ViewModels;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hoot.Security.JwtTokens;

public class JwtTokenProvider : IJwtTokenProvider
{
    private readonly JwtConfig _jwtConfig;
    private readonly ILogger<JwtTokenProvider> _logger;
    public JwtTokenProvider(IOptions<JwtConfig> jwtConfig, ILogger<JwtTokenProvider> logger)
    {
        _jwtConfig = jwtConfig.Value;
        _logger = logger;
    }

    public TokenViewModel Generate(ApplicationUser user)
    {
        //user claims
        var claims = new Claim[] {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Iss, _jwtConfig.Issuer),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        //signing credentials with security algorithm HS256
        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.SecretKey)), SecurityAlgorithms.HmacSha256);

        //jwt token
        #region Write Token With JwtSecurityToken
        //var token = new JwtSecurityToken(_jwtOption.Issuer, _jwtOption.Audience, claims, null, DateTime.Now.AddMinutes(60), signingCredentials);

        //var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        #endregion

        #region Create Token With SecurityTokenDescriptor
        var token = new SecurityTokenDescriptor
        {
            IssuedAt = DateTime.UtcNow,
            Issuer = _jwtConfig.Issuer,
            Audience = _jwtConfig.Audience,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddMinutes(60),
            SigningCredentials = signingCredentials
        };

        var accessToken = ""; // new JsonWebTokenHandler().CreateToken(token);
        #endregion

        var result = new TokenViewModel
        {
            Id_token = accessToken,
            Access_token = accessToken,
        };

        _logger.LogInformation(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));

        return result;
    }
    public TokenViewModel Create(TokenRequestViewModel tokenRequest, ApplicationUser user)
    {
        //signing credentials with security algorithm HS256
        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.SecretKey)), SecurityAlgorithms.HmacSha256);

        string sessionId = Guid.NewGuid().ToString("N"); // Generate sid
        string subjectId = "f5fabdb2-3d19-48f6-bc89-b8f73133fffa"; // Use actual user ID
        string jti = Guid.NewGuid().ToString("N"); // Unique token ID

        //Generate access_token
        string accessToken = GenerateAccessToken(tokenRequest.ClientId, subjectId, sessionId, jti, signingCredentials);

        //Generate id_token
        string idToken = GenerateIdToken(tokenRequest.ClientId, subjectId, sessionId, accessToken, signingCredentials, user);

        var result = new TokenViewModel
        {
            Id_token = idToken,
            Access_token = accessToken,
        };

        _logger.LogInformation(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));

        return result;
    }

    private string GenerateAccessToken(string clientId, string subjectId, string sessionId, string jti, SigningCredentials signingCredentials)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var claims = new List<Claim>{
        new(JwtRegisteredClaimNames.Nbf, now.ToString()),
        new(JwtRegisteredClaimNames.Exp, (now + 3600).ToString()), // 1 hour expiration
        new(JwtRegisteredClaimNames.Iss, _jwtConfig.Issuer),
        new(JwtRegisteredClaimNames.Aud, "https://localhost:7098/resources"),
        new("client_id", clientId),
        new(JwtRegisteredClaimNames.Sub, subjectId),
        new("auth_time", now.ToString()),
        new("idp", "local"),
        new(JwtRegisteredClaimNames.Jti, jti),
        new("sid", sessionId),
        new(JwtRegisteredClaimNames.Iat, now.ToString()),
         new("scope", "openid"),
        new("scope", "profile"),
        new("scope", "email"),
        new("scope", "api1"),
        new("amr", "pwd")
        };

        //var token = new JwtSecurityToken(
        //    issuer: "https://localhost:7098",
        //    audience: "https://localhost:7098/resources",
        //    claims: claims,
        //    expires: DateTime.UtcNow.AddHours(1),
        //    signingCredentials: signingCredentials
        //);
        var tok = new SecurityTokenDescriptor
        {
            Issuer = _jwtConfig.Issuer,
            Audience = "https://localhost:7098/resources",
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = signingCredentials
        };
        _logger.LogInformation("this is Access Token " + JsonSerializer.Serialize(tok, new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.Preserve, WriteIndented = true }));
        //var write = new JwtSecurityTokenHandler().WriteToken(token);
        var write = new JsonWebTokenHandler().CreateToken(tok);
        return write;
    }
    private string GenerateIdToken(string clientId, string subjectId, string sessionId, string accessToken, SigningCredentials signingCredentials, ApplicationUser user)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Nbf, now.ToString()),
        new(JwtRegisteredClaimNames.Exp, (now + 300).ToString()), // 5 minutes expiration
        new(JwtRegisteredClaimNames.Iss, _jwtConfig.Issuer),
        new(JwtRegisteredClaimNames.Aud, clientId),
        new(JwtRegisteredClaimNames.Iat, now.ToString()),
        new("sid", sessionId),
        new(JwtRegisteredClaimNames.Sub, subjectId),
        new("auth_time", now.ToString()),
        new("idp", "local"),
        new(ClaimTypes.NameIdentifier, user.Id),
        new("preferred_username", user.UserName),
        new("name", user.UserName),
        new(JwtRegisteredClaimNames.Email, user.Email),
        new("email_verified", "true"),
        new("amr", "pwd")
    };

        // Generate `at_hash` and `s_hash`
        string atHash = ComputeAtHash(accessToken);
        string sHash = ComputeAtHash(sessionId);
        claims.Add(new Claim("at_hash", atHash));
        claims.Add(new Claim("s_hash", sHash));

        var token = new SecurityTokenDescriptor
        {
            Issuer = "https://localhost:7098",
            Audience = clientId,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = signingCredentials
        };


        _logger.LogInformation("This is Id Token " + JsonSerializer.Serialize(token, new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.Preserve, WriteIndented = true }));

        return new JsonWebTokenHandler().CreateToken(token);
    }

    private string GenerateRefreshToken()
    {
        throw new NotImplementedException();
    }
    private string ComputeAtHash(string token)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            byte[] halfHash = new byte[16]; // First 128 bits
            Array.Copy(hash, halfHash, 16);
            return Base64UrlEncode(halfHash);
        }
    }
    private string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
