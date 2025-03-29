using Hoot.Data;
using Hoot.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace Hoot.Services.Users
{
    public class UserService : IUserService
    {
        readonly ApplicationDbContext _context;
        IHttpContextAccessor _acc;
        public UserService(ApplicationDbContext context, IHttpContextAccessor acc)
        {
            _context = context;
            _acc = acc;
        }

        public async ValueTask<IEnumerable<UserViewModel>?> Get()
        {
            return await _context.Users
                .Select(a => new UserViewModel
                {
                    Name = a.Name,
                    Email = a.Email,
                    EmailConfirmed = a.EmailConfirmed,
                    CreatedOn = a.CreatedOn
                }).ToListAsync();
        }
        public async ValueTask<UserViewModel?> GetById()
        {
            return await _context.Users
                .Select(a => new UserViewModel
                {
                    Name = a.Name,
                    Email = a.Email,
                    EmailConfirmed = a.EmailConfirmed,
                    CreatedOn = a.CreatedOn
                }).FirstOrDefaultAsync();
        }
        public async Task<bool> GetUser()
        {
            return true;
        }

        public Dictionary<string, string> GetClaims()
        {
            var httpContext = _acc.HttpContext;
            if (httpContext == null)
            {
                throw new InvalidOperationException("null");
            }

            var token = httpContext.Request.Cookies["AuthToken"]; // Replace with your cookie name

            if (!string.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var claims = jwtToken.Claims.Select(c => new { c.Type, c.Value }).ToDictionary(a => a.Type, a => a.Value);

                return claims;
            }
            return new();
        }
    }
}
