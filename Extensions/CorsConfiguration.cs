using Hoot.Data;

namespace Hoot.Extensions;

public static class CorsConfiguration
{
    public static void ConfigureDynamicCors(this IServiceCollection services, string connenction)
    {
        // Create a temporary service provider to access the database
        using (var scope = services.BuildServiceProvider().CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var allowedOrigins = dbContext.ClientCorsOrigin.Select(o => o.Url).ToList();

            services.AddCors(options =>
            {
                options.AddPolicy("DynamicCorsPolicy", policy =>
                {
                    policy.WithOrigins(allowedOrigins.ToArray())
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });
        }
    }
}
