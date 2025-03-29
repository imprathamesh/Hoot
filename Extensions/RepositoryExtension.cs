using Hoot.Security;
using Hoot.Security.JwtTokens;
using Hoot.Services;
using Hoot.Services.Users;

namespace Hoot.Extensions;

public static class RepositoryExtension
{
    public static IServiceCollection AddRepository(this IServiceCollection services)
    {
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IJwtTokenProvider, JwtTokenProvider>();

        services.AddScoped<CustomAuthenticationService>();
        return services;
    }
}
