using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Hoot.Helpers;

public class RSAkeys
{
    private readonly IConfiguration _configuration;

    public RSAkeys(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public SymmetricSecurityKey GenerateKeyId()
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]) ?? throw new InvalidOperationException("Secret Key not found"));
    }
}
