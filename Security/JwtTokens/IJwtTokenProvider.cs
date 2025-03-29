using Hoot.Data;
using Hoot.ViewModels;

namespace Hoot.Security.JwtTokens
{
    public interface IJwtTokenProvider
    {
        TokenViewModel Create(TokenRequestViewModel tokenRequest, ApplicationUser user);
        TokenViewModel Generate(ApplicationUser user);
    }
}