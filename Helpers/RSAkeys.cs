using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Hoot.Helpers;

public class RSAkeys
{
    private readonly IConfiguration _configuration;

    public SymmetricSecurityKey GenerateKeyId()
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes("907A6D3546002CE8744DC03DC455A556"));
    }
}
