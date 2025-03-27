using Hoot.Data;
using Hoot.Extensions;
using Hoot.Helpers;
using Hoot.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Hoot.Controllers;

[Route("connect")]
[ApiController]
public class ConnectController : Controller
{
    private readonly ApplicationDbContext _context;
    private UserManager<ApplicationUser> _userManager;
    readonly ILogger<ConnectController> _logger;
    readonly IHttpContextAccessor _httpContextAccessor;
    public ConnectController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<ConnectController> logger, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpGet("authorize")]
    public async Task<IActionResult> Authorize(
        [FromQuery] string client_id,
        [FromQuery] string redirect_uri,
        [FromQuery] string response_type,
        [FromQuery] string scope,
        [FromQuery] string state,
        [FromQuery] string code_challenge,
        [FromQuery] string code_challenge_method)
    {

        var client = await _context.Client.FirstOrDefaultAsync(c => c.ClientId == client_id);

        if (client == null)
        {
            return BadRequest("Invalid client ID.");
        }

        if (client.RedirectUri != redirect_uri)
        {
            return BadRequest("Invalid redirect URI.");
        }

        if (!scope.Split(" ").All(s => client.Scopes.Split(",").Contains(s)))
        {
            return BadRequest("Invalid scopes.");
        }

        // Validate client_id and redirect_uri
        if (client_id != "clientone" || redirect_uri != "https://localhost:7067/authentication/login-callback")
        {
            return BadRequest("Invalid client ID or redirect URI");
        }

        //collecting response_type = form_post from client
        var currentUrl = HttpContext.Request.GetEncodedUrl();

        // Generate authorization code
        var authCode = Guid.NewGuid().ToString("N");

        _logger.LogInformation("Current Authorize URL: " + currentUrl);

        // Redirect user to login page
        //var loginUrl = $"https://localhost:7098/account/login?redirect={redirect_uri}&code={authCode}&state={state}";
        var loginUrl = $"https://localhost:7098/account/login?returnurl={currentUrl}&code={authCode}&state={state}";
        return Redirect(loginUrl);
    }

    [HttpPost("authorize/callback")]
    public IActionResult TokenExchange([FromForm] string code, [FromForm] string redirect_uri)
    {
        // Validate the authorization code
        var userId = ValidateAuthorizationCode(code);
        if (userId == null)
        {
            return BadRequest("Invalid or expired authorization code.");
        }

        // Generate an access token
        var accessToken = GenerateAccessToken(userId);

        // Return the access token
        return Ok(new { access_token = accessToken, token_type = "Bearer", expires_in = 3600 });
    }

    [HttpPost("token")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Token([FromForm] TokenRequest request)
    {
        var client = await _context.Client.FirstOrDefaultAsync(c => c.ClientId == request.ClientId);
        if (client == null)
            return BadRequest(new { error = "invalid_client" });

        //    // if (!AuthCodeStore.IsValid(request.Code))
        //    //   return BadRequest(new { error = "invalid_grant" });

        //    //if (client.RequirePkce && !PkceValidator.Validate(request.CodeVerifier, request.CodeChallenge))
        //    //    return BadRequest(new { error = "invalid_request" });
        AuthCodeStore.Remove(request.Code); // Prevent reuse

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourSuperLongSecureSecretKeyHere"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var header = new JwtHeader(credentials);

        header["kid"] = "YourSuperLongSecureSecretKeyHere"; // ✅ Explicitly set `kid`

        var token = new JwtSecurityToken(
            issuer: "https://localhost:7098",
            audience: "api1",
            claims: new List<Claim> {
                new(JwtRegisteredClaimNames.Sub, request.ClientId),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("Role", Roles.Tenant),
                //new(ClaimTypes.Name,_httpContextAccessor.HttpContext.User.)
            },
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials
        );
        token.Header["kid"] = "YourSuperLongSecureSecretKeyHere";
        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        var result = new TokenViewModel(accessToken, accessToken, "Bearer", 3600);

        _logger.LogInformation(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));

        return Ok(result);
    }

    [HttpGet("UserInfo")]
    public async Task<IActionResult> GetUserInfo()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { error = "Invalid token or user not found" });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { error = "User not found" });
        }

        return Ok(new
        {
            sub = user.Id, // Subject ID
            name = user.UserName,
            email = user.Email,
            roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)
        });
    }

    private bool IsValidClient(string clientId, string redirectUri)
    {
        // Example: A dictionary of allowed clients and their redirect URIs
        var allowedClients = new Dictionary<string, string>
    {
        { "innovusstack", "https://stack.innovustech.in/authentication/login-callback" }
    };

        return allowedClients.TryGetValue(clientId, out var validRedirectUri) && validRedirectUri == redirectUri;
    }
    private static Dictionary<string, (string userId, DateTime expiresAt)> _authCodes = new();

    private string GenerateAuthorizationCode(string userId, string clientId)
    {
        var code = Guid.NewGuid().ToString("N"); // Generate a random code
        _authCodes[code] = (userId, DateTime.UtcNow.AddMinutes(10)); // Store for 10 mins
        return code;
    }
    private string? ValidateAuthorizationCode(string code)
    {
        if (_authCodes.TryGetValue(code, out var data))
        {
            if (data.expiresAt > DateTime.UtcNow)
            {
                _authCodes.Remove(code); // One-time use code, remove after validation
                return data.userId;
            }
        }
        return null;
    }

    private string GenerateAccessToken(string userId)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Your_Secret_Key_Here")); // Keep this secure
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, userId),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        var token = new JwtSecurityToken(
            issuer: "https://accounts.innovustech.in",
            audience: "https://stack.innovustech.in",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1), // Token valid for 1 hour
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}

public record TokenViewModel(string Id_token, string Access_token, string Token_type, int Expires_in);
