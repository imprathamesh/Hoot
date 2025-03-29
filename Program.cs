using Blazored.SessionStorage;
using Hoot.Components;
using Hoot.Components.Account;
using Hoot.Data;
using Hoot.Extensions;
using Hoot.Filters;
using Hoot.Security;
using Hoot.Security.JwtTokens;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBlazoredSessionStorage();

if (builder.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;
    IdentityModelEventSource.LogCompleteSecurityArtifact = true;
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
// .AddAuthenticationStateSerialization(options => options.SerializeAllClaims = true); 

//builder.Services.AddAntiforgery();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();

//builder.Services.AddAuthentication(options =>
//    {
//        options.DefaultScheme = IdentityConstants.ApplicationScheme;
//        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
//    }).AddIdentityCookies();

// Configure Authentication (Cookie for web apps, JWT for APIs)
builder.Services.AddAuthentication(options =>
{
    // ✅ Default authentication for web-based logins (Auth Server UI)
    //options.DefaultScheme = IdentityConstants.ApplicationScheme;
    //options.DefaultSignInScheme = IdentityConstants.ExternalScheme;

    //// ✅ Default authentication for APIs (JWT)
    //options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

    //// ✅ Default challenge for API requests
    //options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; ;

    //// ✅ Ensure users remain authenticated across requests
    //options.DefaultForbidScheme = IdentityConstants.ApplicationScheme;

    #region works but active user not showing
    // 🔹 Use JWT as the default authentication for APIs
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

    // 🔹 Keep cookies for web-based authentication
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    options.DefaultForbidScheme = IdentityConstants.ApplicationScheme;
    #endregion

    #region MyRegion My setting not consume jwt
    //// Default scheme for web apps (cookies)
    //options.DefaultScheme = IdentityConstants.ApplicationScheme;
    ////// options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;

    ////// Default challenge scheme for OpenID Connect
    //options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    //////  options.DefaultChallengeScheme = "oidc";

    ////// Default sign-in scheme for external authentication
    //options.DefaultSignInScheme = IdentityConstants.ExternalScheme;

    //options.DefaultForbidScheme = IdentityConstants.ApplicationScheme;
    #endregion

    //options.DefaultScheme = IdentityConstants.ApplicationScheme;

    //// ✅ Default challenge for login (OIDC)
    //options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;

    //// ✅ Default sign-in scheme for external authentication providers
    //options.DefaultSignInScheme = IdentityConstants.ExternalScheme;

    //// ✅ Default authentication scheme for API requests (JWT)
    //options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

    //// ✅ Default scheme for handling authorization failures
    //options.DefaultForbidScheme = IdentityConstants.ApplicationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => //cookie autheticationscheme
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.None;
}) // Cookie authentication for web apps
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => // JWT authentication for APIs
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ClockSkew = TimeSpan.Zero,
        SaveSigninToken = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]) ?? throw new InvalidOperationException("Secret key not found"))
    };
    //options.Events = new JwtBearerEvents
    //{
    //    //OnMessageReceived = context =>
    //    //{
    //    //    Console.WriteLine($"Authorization Header: {context.Request.Headers["Authorization"]}");
    //    //    return Task.CompletedTask;
    //    //},
    //    //OnTokenValidated = context =>
    //    //{
    //    //    Console.WriteLine("Token successfully validated!");
    //    //    return Task.CompletedTask;
    //    //},
    //    //OnAuthenticationFailed = context =>
    //    //{
    //    //    Console.WriteLine($"Token validation failed: {context.Exception.Message}");
    //    //    return Task.CompletedTask;
    //    //}

    //};
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            //context.Response.Cookies.Append("Program-cs", "Program.cs");
            Console.WriteLine("Token successfully validated!");
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Token validation failed: {context.Exception.Message}");
            return Task.CompletedTask;
        }
    };
})
.AddIdentityCookies(); // Identity-specific cookie configuration

builder.Services.ConfigureOptions<JwtConfigOption>();
//builder.Services.ConfigureOptions<JwtBearerOptionConfig>();

// Configure OpenID Connect-like functionality
builder.Services.AddAuthorizationCore();
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

//app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();



app.Run();
