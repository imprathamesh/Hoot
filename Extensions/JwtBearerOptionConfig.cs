using Hoot.Security.JwtTokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Hoot.Extensions;

public class JwtBearerOptionConfig : IConfigureOptions<JwtBearerOptions>
{
    private readonly JwtConfig _jwtConfig;
    public JwtBearerOptionConfig(IOptions<JwtConfig> jwtConfig)
    {
        _jwtConfig = jwtConfig.Value;
    }

    public void Configure(JwtBearerOptions options)
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ClockSkew = TimeSpan.Zero,
            SaveSigninToken = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = false,

            ValidIssuer = _jwtConfig.Issuer,
            ValidAudience = _jwtConfig.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.SecretKey))
        };
    }
}
