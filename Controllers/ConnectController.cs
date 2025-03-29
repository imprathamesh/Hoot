using Hoot.Data;
using Hoot.Helpers;
using Hoot.Helpers.Store;
using Hoot.Security.JwtTokens;
using Hoot.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Hoot.Controllers;

[Route("connect")]
[ApiController]
public class ConnectController : Controller
{
    private readonly ApplicationDbContext _context;
    private UserManager<ApplicationUser> _userManager;
    readonly ILogger<ConnectController> _logger;
    readonly IHttpContextAccessor _httpContextAccessor;
    readonly IConfiguration _configuration;

    readonly IJwtTokenProvider _jwtTokenProvider;
    public ConnectController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<ConnectController> logger, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IJwtTokenProvider jwtTokenProvider)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _jwtTokenProvider = jwtTokenProvider;
    }
    /// <summary>
    /// Step 1: call to check the configuration of OIDC from clients
    /// </summary>
    /// <param name="client_id"></param>
    /// <param name="redirect_uri"></param>
    /// <param name="response_type"></param>
    /// <param name="scope"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    [HttpGet("authorize")]
    public async Task<IActionResult> Authorize(
        [FromQuery] string client_id,
        [FromQuery] string redirect_uri,
        [FromQuery] string response_type,
        [FromQuery] string scope,
        [FromQuery] string state
        //[FromQuery] string code_challenge,
        //[FromQuery] string code_challenge_method
        )
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

        // If user is authenticated, generate authorization code
        if (User.Identity.IsAuthenticated)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found.");

            string authCode = Guid.NewGuid().ToString("N");

            AuthCodeStore.Store(authCode, userId);

            var callbackUrl = $"{redirect_uri}?code={authCode}&state={state}";

            return Redirect(callbackUrl);
        }

        //  var loginUrl = $"https://localhost:7098/account/login?returnurl={currentUrl}&code={authCode}&state={state}";
        var loginUrl = $"https://localhost:7098/account/login?returnurl={currentUrl}";
        return Redirect(loginUrl);

    }

    [HttpPost("authorize/callback")]
    public IActionResult TokenExchange([FromForm] string code, [FromForm] string redirect_uri)
    {
        // Validate the authorization code
        var userId = ValidateAuthorizationRequest.ValidateAuthorizationCode(code);
        if (userId == null)
        {
            return BadRequest("Invalid or expired authorization code.");
        }

        // Generate an access token
        var accessToken = Guid.NewGuid().ToString(); //GenerateAccessToken(userId);

        // Return the access token
        return Ok(new { access_token = accessToken, token_type = "Bearer", expires_in = 3600 });
    }
    /// <summary>
    /// Step 2: Call to check the token is generated to authenticate clients
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("token")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Token([FromForm] TokenRequestViewModel request)
    {
        var client = await _context.Client.FirstOrDefaultAsync(c => c.ClientId == request.ClientId);
        if (client == null)
            return BadRequest(new { error = "invalid_client" });

        //if (client.RequirePkce && !PkceValidator.Validate(request.CodeVerifier, request.CodeChallenge))
        //    return BadRequest(new { error = "invalid_request" });

        // Retrieve userId from the authorization code grant
        string userId = AuthCodeStore.GetUserId(request.Code);

        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(new { error = "invalid_grant" });
        }

        AuthCodeStore.Remove(request.Code);

        var user = await _userManager.FindByIdAsync(userId);

        var result = _jwtTokenProvider.Create(request, user);

        return Ok(result);
    }

    /// <summary>
    /// Step 3: Get logged in user into this method to retrieve the information of authorized user
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpGet("UserInfo")]
    public async Task<IActionResult> GetUserInfo()
    {
        var user = User;

        if (user == null || !user.Identity.IsAuthenticated)
        {
            Console.WriteLine("User is not authenticated. Claims: " + string.Join(", ", user?.Claims.Select(c => c.Type + "=" + c.Value)));
            return Unauthorized(new { error = "Not Authenticated" });
        }

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            Console.WriteLine("User ID not found in token.");
            return Unauthorized(new { error = "Invalid token or user not found" });
        }

        var appUser = await _userManager.FindByIdAsync(userId);
        if (appUser == null)
        {
            Console.WriteLine("User not found in database.");
            return NotFound(new { error = "User not found" });
        }

        return Ok(new { sub = appUser.Id, name = appUser.UserName, email = appUser.Email });
    }

}

