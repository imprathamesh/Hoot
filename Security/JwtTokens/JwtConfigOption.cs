using Microsoft.Extensions.Options;

namespace Hoot.Security.JwtTokens;

public class JwtConfigOption : IConfigureOptions<JwtConfig>
{
    private const string SectionName = "Jwt";
    private readonly IConfiguration _configuration;
    public JwtConfigOption(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(JwtConfig options)
    {
        (_configuration.GetSection(SectionName).Exists()
            ? _configuration.GetSection(SectionName)
            : throw new InvalidOperationException($"Configuration section {SectionName} is missing."))
            .Bind(options);
    }
}