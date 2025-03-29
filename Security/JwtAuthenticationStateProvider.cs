using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Hoot.Security;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    readonly IHttpContextAccessor _acc;
    //private AuthenticationState authenticationState;
    public JwtAuthenticationStateProvider(IHttpContextAccessor acc) //CustomAuthenticationService service
    {
        _acc = acc;
        //service.UserChanged += (newUser) =>
        //{
        //    authenticationState = new AuthenticationState(newUser);
        //    NotifyAuthenticationStateChanged(Task.FromResult(authenticationState));
        //};
    }

    public async override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var httpContext = _acc.HttpContext;
        if (httpContext == null)
        {
            throw new InvalidOperationException("null");
        }

        var token = httpContext.Request.Cookies["access_token"]; // Replace with your cookie name

        if (string.IsNullOrEmpty(token))
        {
            return new AuthenticationState(new ClaimsPrincipal());
        }

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var identity = new ClaimsIdentity(jwtToken.Claims, "Bearer");
        var user = new ClaimsPrincipal(identity);
        var authState = new AuthenticationState(user);

        NotifyAuthenticationStateChanged(Task.FromResult(authState));

        return authState;
    }
    public void NotifyUserAuthentication(ClaimsPrincipal user)
    {
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public void NotifyUserLogout()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
    }
}
