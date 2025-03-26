using Hoot.Components;
using Hoot.Components.Account;
using Hoot.Data;
using Hoot.Extensions;
using Hoot.Filters;
using Hoot.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;
    IdentityModelEventSource.LogCompleteSecurityArtifact = true;
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
// .AddAuthenticationStateSerialization(options => options.SerializeAllClaims = true); 

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

//builder.Services.AddAuthentication(options =>
//    {
//        options.DefaultScheme = IdentityConstants.ApplicationScheme;
//        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
//    }).AddIdentityCookies();

// Configure Authentication (Cookie for web apps, JWT for APIs)
builder.Services.AddAuthentication(options =>
{
    // Default scheme for web apps (cookies)
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    // options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;

    // Default challenge scheme for OpenID Connect
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    //  options.DefaultChallengeScheme = "oidc";

    // Default sign-in scheme for external authentication
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;

    options.DefaultForbidScheme = IdentityConstants.ApplicationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.None;
}) // Cookie authentication for web apps
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => // JWT authentication for APIs
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        SaveSigninToken = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("907A6D3546002CE8744DC03DC455A556"))
    };
})
.AddIdentityCookies(); // Identity-specific cookie configuration

// Configure OpenID Connect-like functionality
builder.Services.AddAuthorization();
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100MB
});


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.ConfigureDynamicCors(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();



builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiLoggingFilter>();
}); ;

builder.Services.AddRepository();

var app = builder.Build();

app.UseCors("DynamicCorsPolicy");

// Seed roles and admin user
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    await SeedRoleAndAdminUser.Seed(services);
//}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
