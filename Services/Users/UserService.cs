using Hoot.Data;
using Hoot.Models;
using Microsoft.EntityFrameworkCore;

namespace Hoot.Services.Users
{
    public class UserService : IUserService
    {
        readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
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
    }
}
